using System.Net.Sockets;
using System.Security.Cryptography;
using CustomerService.Application.Exceptions;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace CustomerService.Infrastructure.Kyc;

public sealed class KycSecurityService(CustomerDbContext dbContext, IConfiguration configuration)
{
    private const long MaxDocumentBytes = 10 * 1024 * 1024;
    private readonly string[] allowedContentTypes = ["application/pdf", "image/jpeg", "image/png"];

    public async Task<Guid> UploadAsync(Guid customerId, Guid actorId, string documentType, string fileName, string contentType, Stream content, long length, CancellationToken cancellationToken)
    {
        if (length is <= 0 or > MaxDocumentBytes || !allowedContentTypes.Contains(contentType))
            throw new ValidationException("KYC documents must be PDF, JPEG, or PNG files no larger than 10 MB.");

        var customer = await dbContext.Customers.SingleOrDefaultAsync(item => item.Id == customerId, cancellationToken)
            ?? throw new NotFoundException("Customer not found.");
        if (customer.IdentityUserId != actorId) throw new UnauthorizedAccessException("You do not have access to this customer record.");

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var plaintext = buffer.ToArray();
        if (!bool.TryParse(configuration["Kyc:SkipMalwareScan"], out var skipMalwareScan) || !skipMalwareScan)
        {
            await ScanAsync(plaintext, cancellationToken);
        }

        var fingerprint = Convert.ToHexString(SHA256.HashData(plaintext));
        if (await dbContext.KycDocuments.AnyAsync(item => item.Fingerprint == fingerprint, cancellationToken))
            throw new ValidationException("This document has already been submitted for verification.");

        var kycCase = new KycCase(Guid.NewGuid(), customerId, "v1", DateTime.UtcNow.AddYears(7));
        var nonce = RandomNumberGenerator.GetBytes(12);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];
        using (var cipher = new AesGcm(GetEncryptionKey(), tag.Length)) cipher.Encrypt(nonce, plaintext, ciphertext, tag);

        var useDatabaseStorage = string.Equals(configuration["Kyc:Storage:Provider"], "Database", StringComparison.OrdinalIgnoreCase);
        var objectKey = useDatabaseStorage
            ? $"database://{kycCase.Id:N}"
            : $"{customerId:N}/{kycCase.Id:N}/{Guid.NewGuid():N}";
        if (!useDatabaseStorage)
        {
            await StoreAsync(objectKey, contentType, ciphertext, cancellationToken);
        }
        dbContext.KycCases.Add(kycCase);
        dbContext.KycDocuments.Add(new KycDocument(
            Guid.NewGuid(),
            kycCase.Id,
            documentType,
            objectKey,
            contentType,
            length,
            fingerprint,
            nonce,
            tag,
            useDatabaseStorage ? ciphertext : null));
        dbContext.KycAuditEvents.Add(new KycAuditEvent(Guid.NewGuid(), kycCase.Id, "document.submitted", actorId, $"Submitted {documentType} document."));
        await dbContext.SaveChangesAsync(cancellationToken);
        return kycCase.Id;
    }

    public async Task DecideAsync(Guid kycCaseId, Guid reviewerId, bool verify, string? rejectionReason, CancellationToken cancellationToken)
    {
        var kycCase = await dbContext.KycCases.SingleOrDefaultAsync(item => item.Id == kycCaseId, cancellationToken)
            ?? throw new NotFoundException("KYC case not found.");
        if (verify) kycCase.Verify(reviewerId, DateTime.UtcNow.AddYears(3));
        else if (!string.IsNullOrWhiteSpace(rejectionReason)) kycCase.Reject(reviewerId, rejectionReason);
        else throw new ValidationException("A rejection reason is required.");
        dbContext.KycAuditEvents.Add(new KycAuditEvent(Guid.NewGuid(), kycCase.Id, verify ? "kyc.verified" : "kyc.rejected", reviewerId, verify ? "KYC approved." : rejectionReason!));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<KycDocumentContent> GetDocumentAsync(Guid kycCaseId, CancellationToken cancellationToken)
    {
        var document = await dbContext.KycDocuments.AsNoTracking()
            .SingleOrDefaultAsync(item => item.KycCaseId == kycCaseId, cancellationToken)
            ?? throw new NotFoundException("KYC document not found.");

        if (document.EncryptedContent is null)
        {
            throw new NotFoundException("This KYC document is not stored in the database.");
        }

        var plaintext = new byte[document.EncryptedContent.Length];
        using var cipher = new AesGcm(GetEncryptionKey(), document.AuthenticationTag.Length);
        cipher.Decrypt(document.Nonce, document.EncryptedContent, document.AuthenticationTag, plaintext);

        return new KycDocumentContent(plaintext, document.ContentType);
    }

    public Task<IReadOnlyList<KycCaseSummary>> GetPendingCasesAsync(CancellationToken cancellationToken) =>
        (from kycCase in dbContext.KycCases.AsNoTracking()
         join customer in dbContext.Customers.AsNoTracking() on kycCase.CustomerId equals customer.Id
         join document in dbContext.KycDocuments.AsNoTracking() on kycCase.Id equals document.KycCaseId
         where kycCase.Status == KycCaseStatus.PendingReview
         orderby kycCase.SubmittedAtUtc
         select new KycCaseSummary(
             kycCase.Id,
             kycCase.CustomerId,
             customer.FirstName + " " + customer.LastName,
             customer.Email,
             document.DocumentType,
             document.ContentType,
             document.Length,
             kycCase.SubmittedAtUtc,
             kycCase.RiskScore))
        .ToListAsync(cancellationToken)
        .ContinueWith(task => (IReadOnlyList<KycCaseSummary>)task.Result, cancellationToken);

    private byte[] GetEncryptionKey()
    {
        try
        {
            var key = Convert.FromBase64String(configuration["Kyc:EncryptionKeyBase64"] ?? string.Empty);
            if (key.Length == 32) return key;
        }
        catch (FormatException) { }
        throw new InvalidOperationException("KYC encryption is not configured with a valid 32-byte key.");
    }

    private async Task StoreAsync(string objectKey, string contentType, byte[] ciphertext, CancellationToken cancellationToken)
    {
        var endpoint = configuration["Kyc:Storage:Endpoint"] ?? throw new InvalidOperationException("KYC storage endpoint is missing.");
        var accessKey = configuration["Kyc:Storage:AccessKey"] ?? throw new InvalidOperationException("KYC storage access key is missing.");
        var secretKey = configuration["Kyc:Storage:SecretKey"] ?? throw new InvalidOperationException("KYC storage secret key is missing.");
        var bucket = configuration["Kyc:Storage:BucketName"] ?? "kyc-documents";
        var client = new MinioClient().WithEndpoint(endpoint).WithCredentials(accessKey, secretKey).Build();
        if (!await client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket), cancellationToken))
            await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), cancellationToken);
        await using var encrypted = new MemoryStream(ciphertext, writable: false);
        await client.PutObjectAsync(new PutObjectArgs().WithBucket(bucket).WithObject(objectKey).WithStreamData(encrypted).WithObjectSize(ciphertext.Length).WithContentType("application/octet-stream"), cancellationToken);
    }

    private async Task ScanAsync(byte[] bytes, CancellationToken cancellationToken)
    {
        var host = configuration["Kyc:MalwareScannerHost"] ?? "localhost";
        using var client = new TcpClient();
        await client.ConnectAsync(host, 3310, cancellationToken);
        await using var stream = client.GetStream();
        await stream.WriteAsync("zINSTREAM\0"u8.ToArray(), cancellationToken);
        var length = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(bytes.Length));
        await stream.WriteAsync(length, cancellationToken);
        await stream.WriteAsync(bytes, cancellationToken);
        await stream.WriteAsync(new byte[4], cancellationToken);
        var response = new byte[512];
        var read = await stream.ReadAsync(response, cancellationToken);
        var result = System.Text.Encoding.UTF8.GetString(response, 0, read);
        if (!result.Contains("OK", StringComparison.Ordinal)) throw new ValidationException("Document malware scan failed.");
    }
}

public sealed record KycCaseSummary(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string DocumentType,
    string ContentType,
    long SizeBytes,
    DateTime SubmittedAtUtc,
    int RiskScore);

public sealed record KycDocumentContent(byte[] Content, string ContentType);