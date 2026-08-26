namespace CustomerService.Application.Contracts.Customers;

public sealed record CreateCustomerRequest(
    Guid? IdentityUserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    AddressRequest? Address,
    NomineeRequest? Nominee,
    KycRequest? Kyc);
