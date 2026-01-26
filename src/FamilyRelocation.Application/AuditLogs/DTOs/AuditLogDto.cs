using System.Text.Json;
using FamilyRelocation.Domain.Entities;

namespace FamilyRelocation.Application.AuditLogs.DTOs;

/// <summary>
/// DTO for audit log entries returned by the API.
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; init; }
    public required string EntityType { get; init; }
    public Guid EntityId { get; init; }
    public required string Action { get; init; }
    public Dictionary<string, object?>? OldValues { get; init; }
    public Dictionary<string, object?>? NewValues { get; init; }
    public Guid? UserId { get; init; }
    public string? UserEmail { get; init; }
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Resolved friendly names for IDs found in OldValues/NewValues.
    /// Key is the GUID string, value is the friendly display name.
    /// </summary>
    public Dictionary<string, string>? ResolvedNames { get; set; }

    /// <summary>
    /// Friendly description of the entity being changed (e.g., property address, applicant name).
    /// </summary>
    public string? EntityDescription { get; set; }
}

/// <summary>
/// Extension methods for mapping AuditLogEntry to DTO.
/// </summary>
public static class AuditLogMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static AuditLogDto ToDto(this AuditLogEntry entry)
    {
        return new AuditLogDto
        {
            Id = entry.Id,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            Action = entry.Action,
            OldValues = DeserializeJson(entry.OldValues),
            NewValues = DeserializeJson(entry.NewValues),
            UserId = entry.UserId,
            UserEmail = entry.UserEmail,
            Timestamp = entry.Timestamp
        };
    }

    private static Dictionary<string, object?>? DeserializeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
