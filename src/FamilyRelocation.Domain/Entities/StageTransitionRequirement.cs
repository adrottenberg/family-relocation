using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Configures which document types are required for a specific stage transition.
/// Example: BoardApproved → HouseHunting requires BrokerAgreement and CommunityTakanos.
/// </summary>
public class StageTransitionRequirement : Entity<Guid>
{
    /// <summary>
    /// The stage transitioning from
    /// </summary>
    public HousingSearchStage FromStage { get; private set; }

    /// <summary>
    /// The stage transitioning to
    /// </summary>
    public HousingSearchStage ToStage { get; private set; }

    /// <summary>
    /// The document type required for this transition
    /// </summary>
    public Guid DocumentTypeId { get; private set; }

    /// <summary>
    /// Whether this requirement is mandatory (true) or optional (false)
    /// </summary>
    public bool IsRequired { get; private set; }

    // Navigation property
    public virtual DocumentType DocumentType { get; private set; } = null!;

    private StageTransitionRequirement() { }

    /// <summary>
    /// Factory method to create a new stage transition requirement
    /// </summary>
    public static StageTransitionRequirement Create(
        HousingSearchStage fromStage,
        HousingSearchStage toStage,
        Guid documentTypeId,
        bool isRequired = true)
    {
        if (documentTypeId == Guid.Empty)
            throw new ArgumentException("Document type ID is required", nameof(documentTypeId));

        return new StageTransitionRequirement
        {
            Id = Guid.NewGuid(),
            FromStage = fromStage,
            ToStage = toStage,
            DocumentTypeId = documentTypeId,
            IsRequired = isRequired
        };
    }

    /// <summary>
    /// Update whether this requirement is mandatory
    /// </summary>
    public void SetRequired(bool isRequired)
    {
        IsRequired = isRequired;
    }
}
