namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Value object representing spouse (wife) information
/// </summary>
public sealed record SpouseInfo
{
    // Properties use 'private set' for EF Core ToJson() serialization compatibility
    // while maintaining immutability from external code
    public string FirstName { get; private set; }
    public string? MaidenName { get; private set; }
    public string? FatherName { get; private set; }
    /// <summary>
    /// Email address stored as normalized lowercase string for simpler JSON serialization
    /// </summary>
    public string? Email { get; private set; }
    public List<PhoneNumber> PhoneNumbers { get; private set; }
    public string? Occupation { get; private set; }
    public string? EmployerName { get; private set; }
    public string? HighSchool { get; private set; }

    // Private parameterless constructor for EF Core
    private SpouseInfo()
    {
        FirstName = string.Empty;
        PhoneNumbers = new List<PhoneNumber>();
    }

    public SpouseInfo(
        string firstName,
        string? maidenName = null,
        string? fatherName = null,
        string? email = null,
        List<PhoneNumber>? phoneNumbers = null,
        string? occupation = null,
        string? employerName = null,
        string? highSchool = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        FirstName = firstName.Trim();
        MaidenName = maidenName?.Trim();
        FatherName = fatherName?.Trim();
        // Validate and normalize email using the Email value object
        Email = ValueObjects.Email.FromString(email)?.Value;
        PhoneNumbers = phoneNumbers ?? new List<PhoneNumber>();
        Occupation = occupation?.Trim();
        EmployerName = employerName?.Trim();
        HighSchool = highSchool?.Trim();
    }

    public string FullName => MaidenName != null
        ? $"{FirstName} ({MaidenName})"
        : FirstName; 

    public override string ToString() => FullName;
}
