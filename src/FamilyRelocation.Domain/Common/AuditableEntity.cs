namespace FamilyRelocation.Domain.Common;

/// <summary>
/// Base class for entities that require audit tracking (CreatedBy, CreatedAt, ModifiedBy, ModifiedAt).
/// </summary>
public abstract class AuditableEntity
{
    public Guid CreatedBy { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public Guid? ModifiedBy { get; protected set; }
    public DateTime? ModifiedAt { get; protected set; }
}
