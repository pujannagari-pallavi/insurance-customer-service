using CustomerService.Application.Contracts.Customers;
using CustomerService.Application.Exceptions;
using CustomerService.Domain.Repositories;

namespace CustomerService.Application.Services;

public sealed class GetCustomerService(
    ICustomerRepository customerRepository,
    CustomerResponseFactory customerResponseFactory) : IGetCustomerService
{
    public async Task<CustomerResponse> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new NotFoundException("Customer was not found.");

        return customerResponseFactory.Create(customer);
    }

    public async Task<CustomerResponse> GetByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken)
            ?? throw new NotFoundException("Customer was not found.");

        return customerResponseFactory.Create(customer);
    }
}
