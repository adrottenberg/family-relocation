namespace FamilyRelocation.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Guid? UserId { get; private set; }
    public string? UserName { get; private set; }
    public DateTime Timestamp { get; private set; }

    private ActivityLog() { }

    public static ActivityLog Create(
        string entityType,
        Guid entityId,
        string action,
        string description,
        Guid? userId = null,
        string? userName = null)
    {
        return new ActivityLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType)),
            EntityId = entityId,
            Action = action ?? throw new ArgumentNullException(nameof(action)),
            Description = description ?? throw new ArgumentNullException(nameof(description)),
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow
        };
    }
}
