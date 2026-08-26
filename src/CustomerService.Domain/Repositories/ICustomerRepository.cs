using CustomerService.Domain.Entities;

namespace CustomerService.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default);

    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
}
