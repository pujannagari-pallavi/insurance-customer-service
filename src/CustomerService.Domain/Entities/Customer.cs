namespace CustomerService.Domain.Entities;

public sealed class Customer
{
    private Customer()
    {
    }

    public Customer(
        Guid id,
        Guid? identityUserId,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateOnly dateOfBirth)
    {
        Id = id;
        IdentityUserId = identityUserId;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim();
        PhoneNumber = phoneNumber.Trim();
        DateOfBirth = dateOfBirth;
        CreatedAtUtc = DateTime.UtcNow;
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public Guid? IdentityUserId { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public DateOnly DateOfBirth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public Address? Address { get; private set; }

    public Nominee? Nominee { get; private set; }

    public KycProfile? Kyc { get; private set; }

    public void UpdateProfile(
        Guid? identityUserId,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateOnly dateOfBirth,
        bool isActive)
    {
        IdentityUserId = identityUserId;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim();
        PhoneNumber = phoneNumber.Trim();
        DateOfBirth = dateOfBirth;
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetAddress(Address? address)
    {
        Address = address;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetNominee(Nominee? nominee)
    {
        Nominee = nominee;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetKyc(KycProfile? kyc)
    {
        Kyc = kyc;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
