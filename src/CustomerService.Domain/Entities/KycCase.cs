namespace CustomerService.Domain.Entities;

public sealed class KycCase
{
    private KycCase() { }

    public KycCase(Guid id, Guid customerId, string consentVersion, DateTime retentionUntilUtc)
    {
        Id = id;
        CustomerId = customerId;
        ConsentVersion = consentVersion;
        ConsentedAtUtc = DateTime.UtcNow;
        SubmittedAtUtc = DateTime.UtcNow;
        RetentionUntilUtc = retentionUntilUtc;
        Status = KycCaseStatus.PendingReview;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public KycCaseStatus Status { get; private set; }
    public DateTime SubmittedAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public string ConsentVersion { get; private set; } = string.Empty;
    public DateTime ConsentedAtUtc { get; private set; }
    public DateTime? ConsentWithdrawnAtUtc { get; private set; }
    public DateTime RetentionUntilUtc { get; private set; }
    public int RiskScore { get; private set; }

    public void SetRiskScore(int riskScore) => RiskScore = riskScore;

    public void Verify(Guid reviewerId, DateTime expiresAtUtc)
    {
        Status = KycCaseStatus.Verified;
        ReviewedByUserId = reviewerId;
        ReviewedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
        RejectionReason = null;
    }

    public void Reject(Guid reviewerId, string reason)
    {
        Status = KycCaseStatus.Rejected;
        ReviewedByUserId = reviewerId;
        ReviewedAtUtc = DateTime.UtcNow;
        RejectionReason = reason.Trim();
    }

    public void RequireReverification() => Status = KycCaseStatus.ReverificationRequired;
    public void WithdrawConsent() { Status = KycCaseStatus.ConsentWithdrawn; ConsentWithdrawnAtUtc = DateTime.UtcNow; }
}