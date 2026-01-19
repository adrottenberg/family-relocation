namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// Information about a child in the family.
/// </summary>
public record ChildDto
{
    /// <summary>
    /// Age of the child in years.
    /// </summary>
    public required int Age { get; init; }

    /// <summary>
    /// Gender of the child (e.g., "Male", "Female").
    /// </summary>
    public required string Gender { get; init; }

    /// <summary>
    /// First name of the child.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// School the child currently attends.
    /// </summary>
    public string? School { get; init; }
}
