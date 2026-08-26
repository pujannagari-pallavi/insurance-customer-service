using CustomerService.Application.Abstractions.Validation;
using CustomerService.Application.Contracts.Customers;

namespace CustomerService.Application.Validation.Customers;

public sealed class UpdateCustomerRequestValidator : IValidator<UpdateCustomerRequest>
{
    private readonly CreateCustomerRequestValidator _createCustomerRequestValidator = new();

    public void Validate(UpdateCustomerRequest value)
    {
        _createCustomerRequestValidator.Validate(new CreateCustomerRequest(
            value.IdentityUserId,
            value.FirstName,
            value.LastName,
            value.Email,
            value.PhoneNumber,
            value.DateOfBirth,
            value.Address,
            value.Nominee,
            value.Kyc));
    }
}
