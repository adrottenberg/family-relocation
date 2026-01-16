namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Value object representing spouse (wife) information
/// </summary>
public sealed record SpouseInfo
{
    public string FirstName { get; }
    public string? MaidenName { get; }
    public string? FatherName { get; }
    public Email? Email { get; }
    public List<PhoneNumber> PhoneNumbers { get; }
    public string? Occupation { get; }
    public string? EmployerName { get; }
    public string? HighSchool { get; }

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
        Email? email = null,
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
        Email = email;
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
