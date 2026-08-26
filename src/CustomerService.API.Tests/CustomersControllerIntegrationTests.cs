using System.Net;
using System.Net.Http.Json;
using CustomerService.Application.Contracts.Customers;
using CustomerService.Application.Exceptions;
using CustomerService.Application.Services;
using CustomerService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.API.Tests;

public sealed class CustomersControllerIntegrationTests
{
    [Fact]
    public async Task Create_WhenServiceThrowsValidationException_ReturnsBadRequestProblemDetails()
    {
        using var factory = new CustomerApiFactory(
            new ThrowingCreateCustomerService(new ValidationException("Email is required.")),
            new StubGetCustomerService(),
            new StubUpdateCustomerService());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/customers", CreateRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem!.Status);
        Assert.Equal("Validation failed.", problem.Title);
        Assert.Equal("Email is required.", problem.Detail);
    }

    [Fact]
    public async Task GetById_WhenServiceThrowsNotFoundException_ReturnsNotFoundProblemDetails()
    {
        using var factory = new CustomerApiFactory(
            new StubCreateCustomerService(),
            new ThrowingGetCustomerService(new NotFoundException("Customer was not found.")),
            new StubUpdateCustomerService());
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem!.Status);
        Assert.Equal("Resource not found.", problem.Title);
        Assert.Equal("Customer was not found.", problem.Detail);
    }

    [Fact]
    public async Task Health_ReturnsHealthyResponse()
    {
        using var factory = new CustomerApiFactory(
            new StubCreateCustomerService(),
            new StubGetCustomerService(),
            new StubUpdateCustomerService());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_UsesAuthenticatedUserIdInsteadOfRequestValue()
    {
        var createService = new CapturingCreateCustomerService();
        using var factory = new CustomerApiFactory(
            createService,
            new StubGetCustomerService(),
            new StubUpdateCustomerService());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/customers", CreateRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(createService.Request);
        Assert.Equal(TestAuthenticationHandler.UserId, createService.Request!.IdentityUserId);
    }

    [Fact]
    public async Task GetById_WhenCustomerBelongsToAnotherUser_ReturnsForbiddenProblemDetails()
    {
        using var factory = new CustomerApiFactory(
            new StubCreateCustomerService(),
            new StubGetCustomerService(Guid.NewGuid()),
            new StubUpdateCustomerService());
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(403, problem!.Status);
    }

    private sealed class ThrowingCreateCustomerService(Exception exception) : ICreateCustomerService
    {
        public Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromException<CustomerResponse>(exception);
        }
    }

    private sealed class ThrowingGetCustomerService(Exception exception) : IGetCustomerService
    {
        public Task<CustomerResponse> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromException<CustomerResponse>(exception);
        }

        public Task<CustomerResponse> GetByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromException<CustomerResponse>(exception);
        }
    }

    private sealed class StubCreateCustomerService : ICreateCustomerService
    {
        public Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResponse());
        }
    }

    private sealed class CapturingCreateCustomerService : ICreateCustomerService
    {
        public CreateCustomerRequest? Request { get; private set; }

        public Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult(CreateResponse(identityUserId: request.IdentityUserId));
        }
    }

    private sealed class StubGetCustomerService(Guid? identityUserId = null) : IGetCustomerService
    {
        public Task<CustomerResponse> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResponse(customerId, identityUserId));
        }

        public Task<CustomerResponse> GetByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResponse(identityUserId: identityUserId));
        }
    }

    private sealed class StubUpdateCustomerService : IUpdateCustomerService
    {
        public Task<CustomerResponse> UpdateAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResponse(customerId));
        }
    }

    private static CustomerResponse CreateResponse(Guid? customerId = null, Guid? identityUserId = null)
    {
        return new CustomerResponse(
            customerId ?? Guid.NewGuid(),
            identityUserId ?? TestAuthenticationHandler.UserId,
            "Jane",
            "Doe",
            "jane@example.com",
            "+15551234567",
            new DateOnly(1994, 6, 12),
            true,
            DateTime.UtcNow,
            null,
            new AddressResponse("12 Main Street", null, "Hyderabad", "Telangana", "500001", "India"),
            new NomineeResponse("John Doe", "Spouse", "+15557654321", "john@example.com"),
            new KycResponse("Passport", "P1234567", KycStatus.Verified, DateTime.UtcNow));
    }

    private static CreateCustomerRequest CreateRequest()
    {
        return new CreateCustomerRequest(
            Guid.NewGuid(),
            "Jane",
            "Doe",
            "jane@example.com",
            "+15551234567",
            new DateOnly(1994, 6, 12),
            new AddressRequest("12 Main Street", null, "Hyderabad", "Telangana", "500001", "India"),
            new NomineeRequest("John Doe", "Spouse", "+15557654321", "john@example.com"),
            new KycRequest("Passport", "P1234567", KycStatus.Verified, DateTime.UtcNow));
    }
}
