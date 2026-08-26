using CustomerService.Application.Abstractions.Validation;
using CustomerService.Application.Contracts.Customers;
using CustomerService.Application.Services;
using CustomerService.Application.Validation.Customers;
using CustomerService.Domain.Repositories;
using CustomerService.Infrastructure.Persistence;
using CustomerService.Infrastructure.Persistence.Repositories;
using CustomerService.Infrastructure.Kyc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CustomerService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CustomerDbContext>(options =>
            options.UseNpgsql(NormalizePostgresConnectionString(configuration.GetConnectionString("CustomerDatabase"))));

        services.AddScoped<IUnitOfWork, CustomerUnitOfWork>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IValidator<CreateCustomerRequest>, CreateCustomerRequestValidator>();
        services.AddScoped<IValidator<UpdateCustomerRequest>, UpdateCustomerRequestValidator>();
        services.AddScoped<CustomerResponseFactory>();
        services.AddScoped<ICreateCustomerService, CreateCustomerService>();
        services.AddScoped<IGetCustomerService, GetCustomerService>();
        services.AddScoped<IUpdateCustomerService, UpdateCustomerService>();
        services.AddScoped<KycSecurityService>();

        return services;
    }

    private static string NormalizePostgresConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'CustomerDatabase' was not found.");
        }

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var connectionUri)
            || (connectionUri.Scheme is not "postgres" and not "postgresql"))
        {
            return connectionString;
        }

        var credentials = connectionUri.UserInfo.Split(':', 2);
        if (credentials.Length != 2 || string.IsNullOrWhiteSpace(connectionUri.AbsolutePath.Trim('/')))
        {
            throw new InvalidOperationException("The PostgreSQL connection URL must include a username, password, and database name.");
        }

        return new NpgsqlConnectionStringBuilder
        {
            Host = connectionUri.Host,
            Port = connectionUri.IsDefaultPort ? 5432 : connectionUri.Port,
            Database = Uri.UnescapeDataString(connectionUri.AbsolutePath.Trim('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = Uri.UnescapeDataString(credentials[1]),
            SslMode = SslMode.Require
        }.ConnectionString;
    }
}
