namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// DTO for board review decision information
/// </summary>
public record BoardReviewDto
{
    /// <summary>
    /// Board decision: Pending, Approved, Rejected, or Deferred
    /// </summary>
    public required string Decision { get; init; }

    /// <summary>
    /// Date of the board review
    /// </summary>
    public required DateTime ReviewDate { get; init; }

    /// <summary>
    /// Optional notes from the board review
    /// </summary>
    public string? Notes { get; init; }
}
