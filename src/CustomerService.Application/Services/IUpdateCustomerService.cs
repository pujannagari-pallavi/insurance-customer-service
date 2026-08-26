using CustomerService.Application.Contracts.Customers;

namespace CustomerService.Application.Services;

public interface IUpdateCustomerService
{
    Task<CustomerResponse> UpdateAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
}
