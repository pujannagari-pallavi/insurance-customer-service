using System.Net.Mail;
using CustomerService.Application.Abstractions.Validation;
using CustomerService.Application.Contracts.Customers;
using CustomerService.Application.Exceptions;
using CustomerService.Domain.Entities;

namespace CustomerService.Application.Validation.Customers;

public sealed class CreateCustomerRequestValidator : IValidator<CreateCustomerRequest>
{
    public void Validate(CreateCustomerRequest value)
    {
        ValidateName(value.FirstName, "First name");
        ValidateName(value.LastName, "Last name");
        ValidateEmail(value.Email);
        ValidatePhoneNumber(value.PhoneNumber);
        ValidateDateOfBirth(value.DateOfBirth);
        ValidateAddress(value.Address);
        ValidateNominee(value.Nominee);
        ValidateKyc(value.Kyc);
    }

    private static void ValidateName(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{label} is required.");
        }
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("Email is required.");
        }

        try
        {
            _ = new MailAddress(email.Trim());
        }
        catch (FormatException)
        {
            throw new ValidationException("Email format is invalid.");
        }
    }

    private static void ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ValidationException("Phone number is required.");
        }
    }

    private static void ValidateDateOfBirth(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dateOfBirth > today)
        {
            throw new ValidationException("Date of birth cannot be in the future.");
        }
    }

    private static void ValidateAddress(AddressRequest? address)
    {
        if (address is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(address.Line1) ||
            string.IsNullOrWhiteSpace(address.City) ||
            string.IsNullOrWhiteSpace(address.State) ||
            string.IsNullOrWhiteSpace(address.PostalCode) ||
            string.IsNullOrWhiteSpace(address.Country))
        {
            throw new ValidationException("Address is incomplete.");
        }
    }

    private static void ValidateNominee(NomineeRequest? nominee)
    {
        if (nominee is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(nominee.FullName) ||
            string.IsNullOrWhiteSpace(nominee.Relationship) ||
            string.IsNullOrWhiteSpace(nominee.PhoneNumber))
        {
            throw new ValidationException("Nominee information is incomplete.");
        }

        if (!string.IsNullOrWhiteSpace(nominee.Email))
        {
            ValidateEmail(nominee.Email);
        }
    }

    private static void ValidateKyc(KycRequest? kyc)
    {
        if (kyc is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(kyc.DocumentType) || string.IsNullOrWhiteSpace(kyc.DocumentNumber))
        {
            throw new ValidationException("KYC information is incomplete.");
        }

        if (kyc.Status == KycStatus.Verified && kyc.VerifiedAtUtc is null)
        {
            throw new ValidationException("Verified KYC records must include the verification timestamp.");
        }
    }
}
