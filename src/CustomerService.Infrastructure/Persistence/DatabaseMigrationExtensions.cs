using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomerService.Infrastructure.Persistence;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 10;

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("CustomerDatabaseMigration");

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Customer database migration completed.");
                return;
            }
            catch (Exception exception) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    exception,
                    "Customer database migration attempt {Attempt} failed. Retrying.",
                    attempt);
            }
        }

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
