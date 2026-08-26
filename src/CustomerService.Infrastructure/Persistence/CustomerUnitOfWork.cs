using CustomerService.Domain.Repositories;

namespace CustomerService.Infrastructure.Persistence;

public sealed class CustomerUnitOfWork(CustomerDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
