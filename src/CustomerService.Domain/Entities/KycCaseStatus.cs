namespace CustomerService.Domain.Entities;

public enum KycCaseStatus
{
    PendingReview = 1,
    Verified = 2,
    Rejected = 3,
    ReverificationRequired = 4,
    Expired = 5,
    ConsentWithdrawn = 6
}