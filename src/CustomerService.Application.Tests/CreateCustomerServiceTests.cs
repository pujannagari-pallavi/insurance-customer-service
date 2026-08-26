using CustomerService.Application.Abstractions.Validation;
using CustomerService.Application.Contracts.Customers;
using CustomerService.Application.Exceptions;
using CustomerService.Application.Services;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;

namespace CustomerService.Application.Tests;

public sealed class CreateCustomerServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_PersistsCustomerAndReturnsResponse()
    {
        var repository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new CreateCustomerService(
            repository,
            unitOfWork,
            new PassThroughValidator<CreateCustomerRequest>(),
            new CustomerResponseFactory());

        var response = await service.CreateAsync(TestRequests.CreateCustomerRequest());

        Assert.NotNull(repository.AddedCustomer);
        Assert.Equal("jane@example.com", repository.AddedCustomer!.Email);
        Assert.Equal("Jane", repository.AddedCustomer.FirstName);
        Assert.Equal("Doe", repository.AddedCustomer.LastName);
        Assert.NotNull(repository.AddedCustomer.Address);
        Assert.NotNull(repository.AddedCustomer.Nominee);
        Assert.NotNull(repository.AddedCustomer.Kyc);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(repository.AddedCustomer.Id, response.Id);
        Assert.Equal("jane@example.com", response.Email);
        Assert.Equal(KycStatus.Verified, response.Kyc!.Status);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ThrowsValidationException()
    {
        var repository = new FakeCustomerRepository
        {
            CustomerByEmail = TestCustomerFactory.CreateCustomer("jane@example.com")
        };
        var unitOfWork = new FakeUnitOfWork();
        var service = new CreateCustomerService(
            repository,
            unitOfWork,
            new PassThroughValidator<CreateCustomerRequest>(),
            new CustomerResponseFactory());

        var action = () => service.CreateAsync(TestRequests.CreateCustomerRequest());

        var exception = await Assert.ThrowsAsync<ValidationException>(action);
        Assert.Equal("A customer with this email already exists.", exception.Message);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Null(repository.AddedCustomer);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public Customer? CustomerById { get; init; }

        public Customer? CustomerByEmail { get; init; }

        public Customer? AddedCustomer { get; private set; }

        public Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CustomerById);
        }

        public Task<Customer?> GetByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Customer?>(null);
        }

        public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CustomerByEmail);
        }

        public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            AddedCustomer = customer;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class PassThroughValidator<T> : IValidator<T>
    {
        public void Validate(T value)
        {
        }
    }
}

internal static class TestRequests
{
    public static CreateCustomerRequest CreateCustomerRequest()
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
            new KycRequest("Passport", "P1234567", KycStatus.Verified, new DateTime(2026, 1, 5, 10, 30, 0, DateTimeKind.Utc)));
    }

    public static UpdateCustomerRequest UpdateCustomerRequest()
    {
        return new UpdateCustomerRequest(
            Guid.NewGuid(),
            "Janet",
            "Doe",
            "janet@example.com",
            "+15550001111",
            new DateOnly(1993, 5, 11),
            false,
            new AddressRequest("99 Lake View", "Apt 4", "Bengaluru", "Karnataka", "560001", "India"),
            new NomineeRequest("Jenny Doe", "Sister", "+15550999999", null),
            new KycRequest("Aadhaar", "9999-8888-7777", KycStatus.Pending, null));
    }
}

internal static class TestCustomerFactory
{
    public static Customer CreateCustomer(string email = "jane@example.com")
    {
        var customer = new Customer(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Jane",
            "Doe",
            email,
            "+15551234567",
            new DateOnly(1994, 6, 12));
        customer.SetAddress(new Address("12 Main Street", null, "Hyderabad", "Telangana", "500001", "India"));
        customer.SetNominee(new Nominee("John Doe", "Spouse", "+15557654321", "john@example.com"));
        customer.SetKyc(new KycProfile("Passport", "P1234567", KycStatus.Verified, new DateTime(2026, 1, 5, 10, 30, 0, DateTimeKind.Utc)));
        return customer;
    }
}
