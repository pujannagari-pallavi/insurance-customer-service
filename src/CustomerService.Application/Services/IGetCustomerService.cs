using CustomerService.Application.Contracts.Customers;

namespace CustomerService.Application.Services;

public interface IGetCustomerService
{
    Task<CustomerResponse> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<CustomerResponse> GetByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default);
}
