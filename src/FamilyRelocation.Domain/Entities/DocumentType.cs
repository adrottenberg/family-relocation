using FamilyRelocation.Domain.Common;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Configurable document type that can be required for stage transitions.
/// Examples: BrokerAgreement, CommunityTakanos, etc.
/// </summary>
public class DocumentType : Entity<Guid>
{
    /// <summary>
    /// System name/identifier (e.g., "BrokerAgreement")
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// User-friendly display name (e.g., "Broker Agreement")
    /// </summary>
    public string DisplayName { get; private set; } = null!;

    /// <summary>
    /// Optional description of the document type
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Whether this document type is currently active/available
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// System types cannot be deleted (e.g., BrokerAgreement, CommunityTakanos)
    /// </summary>
    public bool IsSystemType { get; private set; }

    /// <summary>
    /// When this document type was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When this document type was last modified
    /// </summary>
    public DateTime? ModifiedAt { get; private set; }

    private DocumentType() { }

    /// <summary>
    /// Factory method to create a new document type
    /// </summary>
    public static DocumentType Create(
        string name,
        string displayName,
        string? description = null,
        bool isSystemType = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));

        return new DocumentType
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            DisplayName = displayName.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            IsSystemType = isSystemType,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update the document type details
    /// </summary>
    public void Update(string displayName, string? description)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));

        DisplayName = displayName.Trim();
        Description = description?.Trim();
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate the document type (soft delete)
    /// </summary>
    public void Deactivate()
    {
        if (IsSystemType)
            throw new InvalidOperationException("Cannot deactivate a system document type");

        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivate a previously deactivated document type
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedAt = DateTime.UtcNow;
    }
}
