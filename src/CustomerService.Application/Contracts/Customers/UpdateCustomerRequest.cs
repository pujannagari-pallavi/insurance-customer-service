namespace CustomerService.Application.Contracts.Customers;

public sealed record UpdateCustomerRequest(
    Guid? IdentityUserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    bool IsActive,
    AddressRequest? Address,
    NomineeRequest? Nominee,
    KycRequest? Kyc);
