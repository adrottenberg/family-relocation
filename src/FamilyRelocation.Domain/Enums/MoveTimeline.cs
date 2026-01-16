namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// When family plans to move
/// Updated: Added Never for investors (Correction #4)
/// </summary>
public enum MoveTimeline
{
    /// <summary>Less than 3 months</summary>
    Immediate,

    /// <summary>3-6 months</summary>
    ShortTerm,

    /// <summary>6-12 months</summary>
    MediumTerm,

    /// <summary>1-2 years</summary>
    LongTerm,

    /// <summary>2+ years</summary>
    Extended,

    /// <summary>Whenever right property found</summary>
    Flexible,

    /// <summary>Haven't decided</summary>
    NotSure,

    /// <summary>Investors, not relocating</summary>
    Never
}
