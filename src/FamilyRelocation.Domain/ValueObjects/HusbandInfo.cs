namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Value object representing husband information
/// </summary>
public sealed record HusbandInfo
{
    // Properties use 'private set' for EF Core ToJson() serialization compatibility
    // while maintaining immutability from external code
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? FatherName { get; private set; }
    /// <summary>
    /// Email address stored as normalized lowercase string for simpler JSON serialization
    /// </summary>
    public string? Email { get; private set; }
    public List<PhoneNumber> PhoneNumbers { get; private set; }
    public string? Occupation { get; private set; }
    public string? EmployerName { get; private set; }

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
        string? email = null,
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
        // Validate and normalize email using the Email value object
        Email = ValueObjects.Email.FromString(email)?.Value;
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
