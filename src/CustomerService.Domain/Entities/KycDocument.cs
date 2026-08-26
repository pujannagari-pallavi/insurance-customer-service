namespace CustomerService.Domain.Entities;

public sealed class KycDocument
{
    private KycDocument() { }

    public KycDocument(Guid id, Guid kycCaseId, string documentType, string objectKey, string contentType, long length, string fingerprint, byte[] nonce, byte[] tag)
    {
        Id = id;
        KycCaseId = kycCaseId;
        DocumentType = documentType.Trim();
        ObjectKey = objectKey;
        ContentType = contentType;
        Length = length;
        Fingerprint = fingerprint;
        Nonce = nonce;
        AuthenticationTag = tag;
        UploadedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid KycCaseId { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public string ObjectKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Length { get; private set; }
    public string Fingerprint { get; private set; } = string.Empty;
    public byte[] Nonce { get; private set; } = [];
    public byte[] AuthenticationTag { get; private set; } = [];
    public DateTime UploadedAtUtc { get; private set; }
}