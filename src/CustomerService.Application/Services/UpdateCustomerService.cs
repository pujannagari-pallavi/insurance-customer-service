using CustomerService.Application.Abstractions.Validation;
using CustomerService.Application.Contracts.Customers;
using CustomerService.Application.Exceptions;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;

namespace CustomerService.Application.Services;

public sealed class UpdateCustomerService(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IValidator<UpdateCustomerRequest> validator,
    CustomerResponseFactory customerResponseFactory) : IUpdateCustomerService
{
    public async Task<CustomerResponse> UpdateAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        validator.Validate(request);

        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new NotFoundException("Customer was not found.");

        var existingCustomer = await customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingCustomer is not null && existingCustomer.Id != customerId)
        {
            throw new ValidationException("A customer with this email already exists.");
        }

        customer.UpdateProfile(
            request.IdentityUserId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DateOfBirth,
            request.IsActive);

        customer.SetAddress(MapAddress(request.Address));
        customer.SetNominee(MapNominee(request.Nominee));
        if (request.Kyc is not null)
        {
            customer.SetKyc(MapKyc(request.Kyc));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return customerResponseFactory.Create(customer);
    }

    private static Address? MapAddress(AddressRequest? request)
    {
        return request is null
            ? null
            : new Address(request.Line1, request.Line2, request.City, request.State, request.PostalCode, request.Country);
    }

    private static Nominee? MapNominee(NomineeRequest? request)
    {
        return request is null
            ? null
            : new Nominee(request.FullName, request.Relationship, request.PhoneNumber, request.Email);
    }

    private static KycProfile? MapKyc(KycRequest? request)
    {
        return request is null
            ? null
            : new KycProfile(request.DocumentType, request.DocumentNumber, request.Status, request.VerifiedAtUtc);
    }
}
