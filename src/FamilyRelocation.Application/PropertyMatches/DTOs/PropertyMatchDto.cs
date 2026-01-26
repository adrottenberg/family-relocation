using FamilyRelocation.Application.Properties.DTOs;

namespace FamilyRelocation.Application.PropertyMatches.DTOs;

/// <summary>
/// Full property match details including nested property and applicant info.
/// </summary>
public record PropertyMatchDto
{
    public required Guid Id { get; init; }
    public required Guid HousingSearchId { get; init; }
    public required Guid PropertyId { get; init; }
    public required string Status { get; init; }
    public required int MatchScore { get; init; }
    public MatchScoreBreakdownDto? MatchDetails { get; init; }
    public string? Notes { get; init; }
    public required bool IsAutoMatched { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }

    // Nested property info
    public required PropertyListDto Property { get; init; }

    // Nested applicant info
    public required MatchApplicantDto Applicant { get; init; }
}

/// <summary>
/// Lightweight property match for list views.
/// </summary>
public record PropertyMatchListDto
{
    public required Guid Id { get; init; }
    public required Guid HousingSearchId { get; init; }
    public required Guid PropertyId { get; init; }
    public required string Status { get; init; }
    public required int MatchScore { get; init; }
    public required bool IsAutoMatched { get; init; }
    public required DateTime CreatedAt { get; init; }

    // Lightweight property info
    public required string PropertyStreet { get; init; }
    public required string PropertyCity { get; init; }
    public required decimal PropertyPrice { get; init; }
    public required int PropertyBedrooms { get; init; }
    public required decimal PropertyBathrooms { get; init; }
    public string? PropertyPhotoUrl { get; init; }

    // Lightweight applicant info
    public required Guid ApplicantId { get; init; }
    public required string ApplicantName { get; init; }

    // All showings for this match
    public List<MatchShowingDto> Showings { get; init; } = [];

    // Convenience properties - returns first future scheduled showing, or last showing if none upcoming
    public DateTime? ScheduledShowingDate
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // First, look for future scheduled showings
            var futureScheduled = Showings
                .Where(s => s.Status == "Scheduled" && s.ScheduledDate >= today)
                .OrderBy(s => s.ScheduledDate)
                .ThenBy(s => s.ScheduledTime)
                .FirstOrDefault();

            if (futureScheduled != null)
                return futureScheduled.ScheduledDate.ToDateTime(futureScheduled.ScheduledTime);

            // If no future scheduled showings, return the most recent showing (any status)
            var lastShowing = Showings
                .OrderByDescending(s => s.ScheduledDate)
                .ThenByDescending(s => s.ScheduledTime)
                .FirstOrDefault();

            return lastShowing?.ScheduledDate.ToDateTime(lastShowing.ScheduledTime);
        }
    }

    public TimeOnly? ScheduledShowingTime
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // First, look for future scheduled showings
            var futureScheduled = Showings
                .Where(s => s.Status == "Scheduled" && s.ScheduledDate >= today)
                .OrderBy(s => s.ScheduledDate)
                .ThenBy(s => s.ScheduledTime)
                .FirstOrDefault();

            if (futureScheduled != null)
                return futureScheduled.ScheduledTime;

            // If no future scheduled showings, return the most recent showing (any status)
            var lastShowing = Showings
                .OrderByDescending(s => s.ScheduledDate)
                .ThenByDescending(s => s.ScheduledTime)
                .FirstOrDefault();

            return lastShowing?.ScheduledTime;
        }
    }
}

/// <summary>
/// Showing info embedded in property match DTO.
/// </summary>
public record MatchShowingDto
{
    public required Guid Id { get; init; }
    public required DateOnly ScheduledDate { get; init; }
    public required TimeOnly ScheduledTime { get; init; }
    public required string Status { get; init; }
    public Guid? BrokerUserId { get; init; }
    public string? BrokerUserName { get; init; }
    public string? Notes { get; init; }
    public DateTime? CompletedAt { get; init; }
}

/// <summary>
/// Match score breakdown showing how points were calculated.
/// </summary>
public record MatchScoreBreakdownDto
{
    public int BudgetScore { get; init; }
    public int MaxBudgetScore { get; init; }
    public string? BudgetNotes { get; init; }

    public int BedroomsScore { get; init; }
    public int MaxBedroomsScore { get; init; }
    public string? BedroomsNotes { get; init; }

    public int BathroomsScore { get; init; }
    public int MaxBathroomsScore { get; init; }
    public string? BathroomsNotes { get; init; }

    public int CityScore { get; init; }
    public int MaxCityScore { get; init; }
    public string? CityNotes { get; init; }

    public int FeaturesScore { get; init; }
    public int MaxFeaturesScore { get; init; }
    public string? FeaturesNotes { get; init; }

    public int TotalScore { get; init; }
    public int MaxTotalScore { get; init; }
}

/// <summary>
/// Minimal applicant info for match display.
/// </summary>
public record MatchApplicantDto
{
    public required Guid Id { get; init; }
    public required string FamilyName { get; init; }
    public string? HusbandFirstName { get; init; }
    public string? WifeFirstName { get; init; }
}

/// <summary>
/// Request to create a new property match.
/// </summary>
public record CreatePropertyMatchRequest
{
    public Guid? HousingSearchId { get; init; }
    public Guid? PropertyId { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request to update property match status.
/// </summary>
public record UpdatePropertyMatchStatusRequest
{
    public required string Status { get; init; }
    public string? Notes { get; init; }
}
