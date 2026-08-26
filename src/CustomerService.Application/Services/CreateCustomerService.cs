using CustomerService.Application.Abstractions.Validation;
using CustomerService.Application.Contracts.Customers;
using CustomerService.Application.Exceptions;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;

namespace CustomerService.Application.Services;

public sealed class CreateCustomerService(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateCustomerRequest> validator,
    CustomerResponseFactory customerResponseFactory) : ICreateCustomerService
{
    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        validator.Validate(request);

        var existingCustomer = await customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingCustomer is not null)
        {
            throw new ValidationException("A customer with this email already exists.");
        }

        var customer = new Customer(
            Guid.NewGuid(),
            request.IdentityUserId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DateOfBirth);

        customer.SetAddress(MapAddress(request.Address));
        customer.SetNominee(MapNominee(request.Nominee));
        customer.SetKyc(MapKyc(request.Kyc));

        await customerRepository.AddAsync(customer, cancellationToken);
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
