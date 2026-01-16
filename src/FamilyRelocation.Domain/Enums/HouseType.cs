namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Type of house structure
/// NEW: Added per Correction #6
/// </summary>
public enum HouseType
{
    Colonial,
    CapeCod,
    /// <summary>Single-level ranch</summary>
    Flat,
    SplitLevel,
    BiLevel,
    Townhouse,
    Duplex,
    Condo,
    Victorian,
    Contemporary,
    Other
}
