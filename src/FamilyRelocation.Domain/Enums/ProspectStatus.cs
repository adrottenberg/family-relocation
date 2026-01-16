namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Status of a prospect in the pipeline
/// </summary>
public enum ProspectStatus
{
    New,
    Contacted,
    Interested,
    NotInterested,
    ConvertedToApplicant,
    Inactive
}
