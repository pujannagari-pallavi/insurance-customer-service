using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(CustomerDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return dbContext.Customers
            .SingleOrDefaultAsync(customer => customer.Id == customerId, cancellationToken);
    }

    public Task<Customer?> GetByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default)
    {
        return dbContext.Customers
            .SingleOrDefaultAsync(customer => customer.IdentityUserId == identityUserId, cancellationToken);
    }

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return dbContext.Customers
            .SingleOrDefaultAsync(customer => customer.Email == email.Trim(), cancellationToken);
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        return dbContext.Customers.AddAsync(customer, cancellationToken).AsTask();
    }
}
