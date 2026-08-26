namespace CustomerService.Domain.Entities;

public sealed class KycProfile
{
    private KycProfile()
    {
    }

    public KycProfile(string documentType, string documentNumber, KycStatus status, DateTime? verifiedAtUtc)
    {
        DocumentType = documentType.Trim();
        DocumentNumber = documentNumber.Trim();
        Status = status;
        VerifiedAtUtc = verifiedAtUtc;
    }

    public string DocumentType { get; private set; } = string.Empty;

    public string DocumentNumber { get; private set; } = string.Empty;

    public KycStatus Status { get; private set; }

    public DateTime? VerifiedAtUtc { get; private set; }
}
