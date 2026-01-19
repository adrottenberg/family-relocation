namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Represents an audit log entry tracking changes to domain entities.
/// </summary>
public class AuditLogEntry
{
    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = null!;
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public Guid? UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? IpAddress { get; private set; }

    private AuditLogEntry() { }

    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    public static AuditLogEntry Create(
        string entityType,
        Guid entityId,
        string action,
        string? oldValues,
        string? newValues,
        Guid? userId,
        string? userEmail,
        string? ipAddress)
    {
        return new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = userId,
            UserEmail = userEmail,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress
        };
    }
}
