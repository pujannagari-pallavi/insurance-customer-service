namespace CustomerService.Domain.Entities;

public sealed class Nominee
{
    private Nominee()
    {
    }

    public Nominee(string fullName, string relationship, string phoneNumber, string? email)
    {
        FullName = fullName.Trim();
        Relationship = relationship.Trim();
        PhoneNumber = phoneNumber.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    public string FullName { get; private set; } = string.Empty;

    public string Relationship { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public string? Email { get; private set; }
}
