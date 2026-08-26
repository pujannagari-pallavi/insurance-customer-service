namespace CustomerService.Domain.Entities;

public sealed class KycAuditEvent
{
    private KycAuditEvent() { }

    public KycAuditEvent(Guid id, Guid kycCaseId, string eventType, Guid? actorUserId, string details)
    {
        Id = id;
        KycCaseId = kycCaseId;
        EventType = eventType;
        ActorUserId = actorUserId;
        Details = details;
        OccurredAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid KycCaseId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public Guid? ActorUserId { get; private set; }
    public string Details { get; private set; } = string.Empty;
    public DateTime OccurredAtUtc { get; private set; }
}