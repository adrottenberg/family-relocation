namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Housing search lifecycle stages
/// </summary>
public enum HousingSearchStage
{
    Submitted,
    Rejected,
    HouseHunting,
    UnderContract,
    Closed,
    MovedIn,
    Paused
}
