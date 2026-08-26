namespace CustomerService.Application.Contracts.Customers;

public sealed record NomineeRequest(
    string FullName,
    string Relationship,
    string PhoneNumber,
    string? Email);
