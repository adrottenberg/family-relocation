namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// Information about the wife/spouse.
/// </summary>
public record SpouseInfoDto
{
    /// <summary>
    /// First name of the wife.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Maiden name (family name before marriage).
    /// </summary>
    public string? MaidenName { get; init; }

    /// <summary>
    /// Father's name (for identification purposes).
    /// </summary>
    public string? FatherName { get; init; }

    /// <summary>
    /// Email address for contact.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// List of phone numbers with type and primary designation.
    /// </summary>
    public List<PhoneNumberDto>? PhoneNumbers { get; init; }

    /// <summary>
    /// Current occupation or profession.
    /// </summary>
    public string? Occupation { get; init; }

    /// <summary>
    /// Name of the current employer.
    /// </summary>
    public string? EmployerName { get; init; }

    /// <summary>
    /// High school attended (for community identification).
    /// </summary>
    public string? HighSchool { get; init; }
}
