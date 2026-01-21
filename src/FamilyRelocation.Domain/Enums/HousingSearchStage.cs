namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Housing search lifecycle stages.
/// This tracks the house-hunting journey for an approved applicant.
/// Application-level stages (Submitted, Approved, Rejected) are tracked via ApplicationStatus on Applicant.
/// </summary>
public enum HousingSearchStage
{
    /// <summary>Actively searching for homes</summary>
    Searching = 0,

    /// <summary>Property under contract</summary>
    UnderContract = 1,

    /// <summary>Closing completed</summary>
    Closed = 2,

    /// <summary>Family has moved in - journey complete</summary>
    MovedIn = 3,

    /// <summary>Search temporarily paused</summary>
    Paused = 4
}
