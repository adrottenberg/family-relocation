namespace FamilyRelocation.Application.Showings.DTOs;

/// <summary>
/// Full showing details with nested property and applicant info.
/// </summary>
public record ShowingDto
{
    public required Guid Id { get; init; }
    public required Guid PropertyMatchId { get; init; }
    public required DateOnly ScheduledDate { get; init; }
    public required TimeOnly ScheduledTime { get; init; }
    public required string Status { get; init; }
    public string? Notes { get; init; }
    public Guid? BrokerUserId { get; init; }
    public string? BrokerUserName { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public DateTime? CompletedAt { get; init; }

    // Property info
    public required Guid PropertyId { get; init; }
    public required string PropertyStreet { get; init; }
    public required string PropertyCity { get; init; }
    public required decimal PropertyPrice { get; init; }
    public string? PropertyPhotoUrl { get; init; }

    // Applicant info
    public required Guid ApplicantId { get; init; }
    public required string ApplicantName { get; init; }
}

/// <summary>
/// Lightweight showing for list/calendar views.
/// </summary>
public record ShowingListDto
{
    public required Guid Id { get; init; }
    public required Guid PropertyMatchId { get; init; }
    public required DateOnly ScheduledDate { get; init; }
    public required TimeOnly ScheduledTime { get; init; }
    public required string Status { get; init; }
    public Guid? BrokerUserId { get; init; }

    // Property summary
    public required Guid PropertyId { get; init; }
    public required string PropertyStreet { get; init; }
    public required string PropertyCity { get; init; }
    public string? PropertyPhotoUrl { get; init; }

    // Applicant summary
    public required Guid ApplicantId { get; init; }
    public required string ApplicantName { get; init; }
}

/// <summary>
/// Request to schedule a new showing.
/// </summary>
public record ScheduleShowingRequest
{
    public required Guid PropertyMatchId { get; init; }
    public required DateOnly ScheduledDate { get; init; }
    public required TimeOnly ScheduledTime { get; init; }
    public string? Notes { get; init; }
    public Guid? BrokerUserId { get; init; }
}

/// <summary>
/// Request to reschedule a showing.
/// </summary>
public record RescheduleShowingRequest
{
    public required DateOnly NewDate { get; init; }
    public required TimeOnly NewTime { get; init; }
}

/// <summary>
/// Request to update showing status.
/// </summary>
public record UpdateShowingStatusRequest
{
    public required string Status { get; init; }
    public string? Notes { get; init; }
}
