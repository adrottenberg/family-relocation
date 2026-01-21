namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// Full details of an applicant family.
/// </summary>
public record ApplicantDto
{
    /// <summary>
    /// Unique identifier for the applicant.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Application status (Submitted, Approved, Rejected).
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Information about the husband/primary applicant.
    /// </summary>
    public required HusbandInfoDto Husband { get; init; }

    /// <summary>
    /// Information about the wife/spouse.
    /// </summary>
    public SpouseInfoDto? Wife { get; init; }

    /// <summary>
    /// Current residential address.
    /// </summary>
    public AddressDto? Address { get; init; }

    /// <summary>
    /// List of children in the family.
    /// </summary>
    public List<ChildDto>? Children { get; init; }

    /// <summary>
    /// Current kehila/community affiliation.
    /// </summary>
    public string? CurrentKehila { get; init; }

    /// <summary>
    /// Shul attended on Shabbos.
    /// </summary>
    public string? ShabbosShul { get; init; }

    /// <summary>
    /// Family name (typically husband's last name).
    /// </summary>
    public required string FamilyName { get; init; }

    /// <summary>
    /// Total number of children in the family.
    /// </summary>
    public required int NumberOfChildren { get; init; }

    /// <summary>
    /// Whether the applicant is awaiting board review.
    /// </summary>
    public required bool IsPendingBoardReview { get; init; }

    /// <summary>
    /// Whether the application was self-submitted (vs referred).
    /// </summary>
    public required bool IsSelfSubmitted { get; init; }

    /// <summary>
    /// Date and time the applicant record was created.
    /// </summary>
    public required DateTime CreatedDate { get; init; }

    /// <summary>
    /// Board review decision (null if not yet reviewed).
    /// </summary>
    public BoardReviewDto? BoardReview { get; init; }

    /// <summary>
    /// Housing search details (null if not yet approved).
    /// </summary>
    public HousingSearchDto? HousingSearch { get; init; }

    /// <summary>
    /// Housing preferences (effective preferences).
    /// Before approval: shows Applicant.Preferences (set at creation).
    /// After approval: shows HousingSearch.Preferences (updateable).
    /// </summary>
    public HousingPreferencesDto? Preferences { get; init; }
}
