using CustomerService.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CustomerService.API.Tests;

public sealed class CustomerApiFactory(
    ICreateCustomerService createCustomerService,
    IGetCustomerService getCustomerService,
    IUpdateCustomerService updateCustomerService) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "CustomerService.API")));
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });

            services.RemoveAll<ICreateCustomerService>();
            services.RemoveAll<IGetCustomerService>();
            services.RemoveAll<IUpdateCustomerService>();

            services.AddSingleton(createCustomerService);
            services.AddSingleton(getCustomerService);
            services.AddSingleton(updateCustomerService);
        });
    }
}
