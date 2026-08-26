using CustomerService.Application.Abstractions.Validation;
using CustomerService.Application.Contracts.Customers;
using CustomerService.Application.Exceptions;
using CustomerService.Application.Services;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;

namespace CustomerService.Application.Tests;

public sealed class UpdateCustomerServiceTests
{
    [Fact]
    public async Task UpdateAsync_WhenCustomerExists_UpdatesProfileAndSavesChanges()
    {
        var customer = TestCustomerFactory.CreateCustomer();
        var repository = new FakeCustomerRepository(customer, null);
        var unitOfWork = new FakeUnitOfWork();
        var service = new UpdateCustomerService(
            repository,
            unitOfWork,
            new PassThroughValidator<UpdateCustomerRequest>(),
            new CustomerResponseFactory());

        var response = await service.UpdateAsync(customer.Id, TestRequests.UpdateCustomerRequest());

        Assert.Equal("Janet", customer.FirstName);
        Assert.Equal("janet@example.com", customer.Email);
        Assert.False(customer.IsActive);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal("Bengaluru", response.Address!.City);
        Assert.Equal(KycStatus.Pending, response.Kyc!.Status);
    }

    [Fact]
    public async Task UpdateAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
    {
        var repository = new FakeCustomerRepository(null, null);
        var service = new UpdateCustomerService(
            repository,
            new FakeUnitOfWork(),
            new PassThroughValidator<UpdateCustomerRequest>(),
            new CustomerResponseFactory());

        var action = () => service.UpdateAsync(Guid.NewGuid(), TestRequests.UpdateCustomerRequest());

        var exception = await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal("Customer was not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_WhenAnotherCustomerOwnsEmail_ThrowsValidationException()
    {
        var customer = TestCustomerFactory.CreateCustomer();
        var duplicate = TestCustomerFactory.CreateCustomer("janet@example.com");
        var repository = new FakeCustomerRepository(customer, duplicate);
        var service = new UpdateCustomerService(
            repository,
            new FakeUnitOfWork(),
            new PassThroughValidator<UpdateCustomerRequest>(),
            new CustomerResponseFactory());

        var action = () => service.UpdateAsync(customer.Id, TestRequests.UpdateCustomerRequest());

        var exception = await Assert.ThrowsAsync<ValidationException>(action);
        Assert.Equal("A customer with this email already exists.", exception.Message);
    }

    private sealed class FakeCustomerRepository(Customer? customerById, Customer? customerByEmail) : ICustomerRepository
    {
        public Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(customerById);
        }

        public Task<Customer?> GetByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Customer?>(null);
        }

        public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(customerByEmail);
        }

        public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
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
