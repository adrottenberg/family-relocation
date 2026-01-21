namespace FamilyRelocation.Application.Common.Interfaces;

public interface IActivityLogger
{
    Task LogAsync(string entityType, Guid entityId, string action, string description, CancellationToken ct = default);
}
