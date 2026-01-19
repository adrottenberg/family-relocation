namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Housing search lifecycle stages
/// </summary>
public enum HousingSearchStage
{
    /// <summary>Application submitted, pending board review</summary>
    Submitted,

    /// <summary>Board approved, awaiting signed agreements (broker + takanos)</summary>
    BoardApproved,

    /// <summary>Application rejected by board</summary>
    Rejected,

    /// <summary>Actively searching for homes (agreements signed)</summary>
    HouseHunting,

    /// <summary>Property under contract</summary>
    UnderContract,

    /// <summary>Closing completed</summary>
    Closed,

    /// <summary>Family has moved in - journey complete</summary>
    MovedIn,

    /// <summary>Search temporarily paused</summary>
    Paused
}
