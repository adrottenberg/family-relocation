namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Application status for an applicant (family).
/// This tracks the application/approval lifecycle, separate from the housing search journey.
/// </summary>
public enum ApplicationStatus
{
    /// <summary>Application submitted, pending board review</summary>
    Submitted = 0,

    /// <summary>Board approved - can start housing search</summary>
    Approved = 1,

    /// <summary>Application rejected by board</summary>
    Rejected = 2
}
