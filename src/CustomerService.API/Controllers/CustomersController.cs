using CustomerService.Application.Contracts.Customers;
using CustomerService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CustomerService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CustomersController(
    ICreateCustomerService createCustomerService,
    IGetCustomerService getCustomerService,
    IUpdateCustomerService updateCustomerService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var response = await createCustomerService.CreateAsync(request with { IdentityUserId = GetActorId() }, cancellationToken);
        return Ok(response);
    }

    [HttpGet("me")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var response = await getCustomerService.GetByIdentityUserIdAsync(GetActorId(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{customerId:guid}")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid customerId, CancellationToken cancellationToken)
    {
        var response = await getCustomerService.GetByIdAsync(customerId, cancellationToken);
        EnsureOwnership(response);
        return Ok(response);
    }

    [HttpPut("{customerId:guid}")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid customerId, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var existingCustomer = await getCustomerService.GetByIdAsync(customerId, cancellationToken);
        EnsureOwnership(existingCustomer);

        var response = await updateCustomerService.UpdateAsync(
            customerId,
            request with { IdentityUserId = GetActorId() },
            cancellationToken);
        return Ok(response);
    }

    private Guid GetActorId()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(identityUserId, out var userId))
        {
            throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");
        }

        return userId;
    }

    private void EnsureOwnership(CustomerResponse customer)
    {
        if (User.IsInRole("Admin") || customer.IdentityUserId == GetActorId())
        {
            return;
        }

        throw new UnauthorizedAccessException("You do not have access to this customer record.");
    }
}
