using CustomerService.Application.Contracts.Customers;

namespace CustomerService.Application.Services;

public interface ICreateCustomerService
{
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
}
