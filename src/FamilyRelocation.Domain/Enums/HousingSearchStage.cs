namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Housing search lifecycle stages.
/// This tracks the house-hunting journey for an approved applicant.
/// Application-level stages (Submitted, Approved, Rejected) are tracked via ApplicationStatus on Applicant.
/// </summary>
public enum HousingSearchStage
{
    /// <summary>Board approved, waiting for agreements to be signed</summary>
    AwaitingAgreements = 0,

    /// <summary>Actively searching for homes</summary>
    Searching = 1,

    /// <summary>Property under contract</summary>
    UnderContract = 2,

    /// <summary>Closing completed</summary>
    Closed = 3,

    /// <summary>Family has moved in - journey complete</summary>
    MovedIn = 4,

    /// <summary>Search temporarily paused</summary>
    Paused = 5
}
