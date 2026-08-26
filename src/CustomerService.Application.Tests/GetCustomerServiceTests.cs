using CustomerService.Application.Contracts.Customers;
using CustomerService.Application.Exceptions;
using CustomerService.Application.Services;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;

namespace CustomerService.Application.Tests;

public sealed class GetCustomerServiceTests
{
    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ReturnsMappedResponse()
    {
        var customer = TestCustomerFactory.CreateCustomer();
        var repository = new FakeCustomerRepository(customer);
        var service = new GetCustomerService(repository, new CustomerResponseFactory());

        var response = await service.GetByIdAsync(customer.Id);

        Assert.Equal(customer.Id, response.Id);
        Assert.Equal(customer.Email, response.Email);
        Assert.Equal(customer.Address!.City, response.Address!.City);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
    {
        var repository = new FakeCustomerRepository(null);
        var service = new GetCustomerService(repository, new CustomerResponseFactory());

        var action = () => service.GetByIdAsync(Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal("Customer was not found.", exception.Message);
    }

    [Fact]
    public async Task GetByIdentityUserIdAsync_WhenCustomerExists_ReturnsMappedResponse()
    {
        var customer = TestCustomerFactory.CreateCustomer();
        var repository = new FakeCustomerRepository(customer);
        var service = new GetCustomerService(repository, new CustomerResponseFactory());

        var response = await service.GetByIdentityUserIdAsync(customer.IdentityUserId!.Value);

        Assert.Equal(customer.Id, response.Id);
        Assert.Equal(customer.IdentityUserId, response.IdentityUserId);
    }

    private sealed class FakeCustomerRepository(Customer? customer) : ICustomerRepository
    {
        public Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(customer);
        }

        public Task<Customer?> GetByIdentityUserIdAsync(Guid identityUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(customer);
        }

        public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Customer?>(null);
        }

        public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
