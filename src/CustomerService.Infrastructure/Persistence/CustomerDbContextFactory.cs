using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CustomerService.Infrastructure.Persistence;

public sealed class CustomerDbContextFactory : IDesignTimeDbContextFactory<CustomerDbContext>
{
    public CustomerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CustomerDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=InsurancePlatformCustomerDb;Username=postgres;Password=admin");

        return new CustomerDbContext(optionsBuilder.Options);
    }
}
