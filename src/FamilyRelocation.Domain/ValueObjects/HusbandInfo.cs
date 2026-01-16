namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Value object representing husband information
/// </summary>
public sealed record HusbandInfo
{
    public string FirstName { get; }
    public string LastName { get; }
    public string? FatherName { get; }
    public Email? Email { get; }
    public List<PhoneNumber> PhoneNumbers { get; }
    public string? Occupation { get; }
    public string? EmployerName { get; }

    // Private parameterless constructor for EF Core
    private HusbandInfo()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        PhoneNumbers = new List<PhoneNumber>();
    }

    public HusbandInfo(
        string firstName,
        string lastName,
        string? fatherName = null,
        Email? email = null,
        List<PhoneNumber>? phoneNumbers = null,
        string? occupation = null,
        string? employerName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        FatherName = fatherName?.Trim();
        Email = email;
        PhoneNumbers = phoneNumbers ?? new List<PhoneNumber>();
        Occupation = occupation?.Trim();
        EmployerName = employerName?.Trim();
    }

    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Full name with father's name (e.g., "Moshe ben Yaakov Cohen")
    /// </summary>
    public string FullNameWithFather => FatherName != null
        ? $"{FirstName} {LastName} (ben {FatherName})"
        : FullName;

    public override string ToString() => FullName;
}
