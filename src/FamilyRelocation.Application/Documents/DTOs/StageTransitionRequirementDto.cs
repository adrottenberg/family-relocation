namespace FamilyRelocation.Application.Documents.DTOs;

/// <summary>
/// DTO for stage transition requirement information.
/// </summary>
public record StageTransitionRequirementDto
{
    /// <summary>
    /// Unique identifier for the requirement.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The stage transitioning from.
    /// </summary>
    public required string FromStage { get; init; }

    /// <summary>
    /// The stage transitioning to.
    /// </summary>
    public required string ToStage { get; init; }

    /// <summary>
    /// The document type ID required for this transition.
    /// </summary>
    public required Guid DocumentTypeId { get; init; }

    /// <summary>
    /// Display name of the required document type.
    /// </summary>
    public required string DocumentTypeName { get; init; }

    /// <summary>
    /// Whether this requirement is mandatory.
    /// </summary>
    public required bool IsRequired { get; init; }
}

/// <summary>
/// Grouped stage requirements for a specific transition.
/// </summary>
public record StageTransitionRequirementsDto
{
    /// <summary>
    /// The stage transitioning from.
    /// </summary>
    public required string FromStage { get; init; }

    /// <summary>
    /// The stage transitioning to.
    /// </summary>
    public required string ToStage { get; init; }

    /// <summary>
    /// List of required document types for this transition.
    /// </summary>
    public required List<DocumentRequirementDto> Requirements { get; init; }
}

/// <summary>
/// Individual document requirement within a stage transition.
/// </summary>
public record DocumentRequirementDto
{
    /// <summary>
    /// The document type ID.
    /// </summary>
    public required Guid DocumentTypeId { get; init; }

    /// <summary>
    /// Display name of the document type.
    /// </summary>
    public required string DocumentTypeName { get; init; }

    /// <summary>
    /// Whether this document is required (vs optional).
    /// </summary>
    public required bool IsRequired { get; init; }

    /// <summary>
    /// Whether the document has been uploaded (for checking completeness).
    /// </summary>
    public bool IsUploaded { get; init; }
}
