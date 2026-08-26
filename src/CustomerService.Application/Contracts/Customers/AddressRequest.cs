namespace CustomerService.Application.Contracts.Customers;

public sealed record AddressRequest(
    string Line1,
    string? Line2,
    string City,
    string State,
    string PostalCode,
    string Country);
