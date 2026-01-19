namespace FamilyRelocation.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// The name of the entity type that was not found.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// The ID of the entity that was not found.
    /// </summary>
    public object EntityId { get; }

    /// <summary>
    /// Initializes a new instance of the exception.
    /// </summary>
    /// <param name="entityName">The entity type name.</param>
    /// <param name="entityId">The entity ID that was not found.</param>
    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with ID '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
