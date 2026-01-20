namespace FamilyRelocation.Application.Documents.DTOs;

/// <summary>
/// DTO for document type information.
/// </summary>
public record DocumentTypeDto
{
    /// <summary>
    /// Unique identifier for the document type.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// System name/identifier (e.g., "BrokerAgreement").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// User-friendly display name (e.g., "Broker Agreement").
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Optional description of the document type.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Whether this document type is currently active/available.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// System types cannot be deleted (e.g., BrokerAgreement, CommunityTakanos).
    /// </summary>
    public required bool IsSystemType { get; init; }
}
