using CustomerService.Application.Contracts.Customers;
using CustomerService.Domain.Entities;

namespace CustomerService.Application.Services;

public sealed class CustomerResponseFactory
{
    public CustomerResponse Create(Customer customer)
    {
        return new CustomerResponse(
            customer.Id,
            customer.IdentityUserId,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.PhoneNumber,
            customer.DateOfBirth,
            customer.IsActive,
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc,
            customer.Address is null
                ? null
                : new AddressResponse(
                    customer.Address.Line1,
                    customer.Address.Line2,
                    customer.Address.City,
                    customer.Address.State,
                    customer.Address.PostalCode,
                    customer.Address.Country),
            customer.Nominee is null
                ? null
                : new NomineeResponse(
                    customer.Nominee.FullName,
                    customer.Nominee.Relationship,
                    customer.Nominee.PhoneNumber,
                    customer.Nominee.Email),
            customer.Kyc is null
                ? null
                : new KycResponse(
                    customer.Kyc.DocumentType,
                    MaskDocumentNumber(customer.Kyc.DocumentNumber),
                    customer.Kyc.Status,
                    customer.Kyc.VerifiedAtUtc));
    }

    private static string MaskDocumentNumber(string documentNumber)
    {
        if (documentNumber.Length <= 4) return new string('*', documentNumber.Length);
        return new string('*', documentNumber.Length - 4) + documentNumber[^4..];
    }
}
