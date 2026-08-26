using CustomerService.Domain.Entities;

namespace CustomerService.Application.Contracts.Customers;

public sealed record CustomerResponse(
    Guid Id,
    Guid? IdentityUserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    AddressResponse? Address,
    NomineeResponse? Nominee,
    KycResponse? Kyc);

public sealed record AddressResponse(
    string Line1,
    string? Line2,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record NomineeResponse(
    string FullName,
    string Relationship,
    string PhoneNumber,
    string? Email);

public sealed record KycResponse(
    string DocumentType,
    string DocumentNumber,
    KycStatus Status,
    DateTime? VerifiedAtUtc);
