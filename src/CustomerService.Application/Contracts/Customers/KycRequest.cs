using CustomerService.Domain.Entities;

namespace CustomerService.Application.Contracts.Customers;

public sealed record KycRequest(
    string DocumentType,
    string DocumentNumber,
    KycStatus Status,
    DateTime? VerifiedAtUtc);
