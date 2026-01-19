namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// Represents a phone number with type and primary designation.
/// </summary>
public record PhoneNumberDto
{
    /// <summary>
    /// The phone number (e.g., "908-555-1234").
    /// </summary>
    public required string Number { get; init; }

    /// <summary>
    /// Type of phone (e.g., "Mobile", "Home", "Work"). Default is "Mobile".
    /// </summary>
    public string Type { get; init; } = "Mobile";

    /// <summary>
    /// Indicates whether this is the primary contact number.
    /// </summary>
    public bool IsPrimary { get; init; }
}
