using System.Text.Json;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FamilyRelocation.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically creates audit log entries for tracked entity changes.
/// </summary>
public class AuditingInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditingInterceptor(
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return ValueTask.FromResult(result);

        CreateAuditEntries(context);

        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context == null) return result;

        CreateAuditEntries(context);

        return result;
    }

    private void CreateAuditEntries(DbContext context)
    {
        var auditEntries = new List<AuditLogEntry>();
        var userId = _currentUserService.UserId;
        var userEmail = _currentUserService.Email;
        var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Skip non-audited entities
            if (entry.Entity is AuditLogEntry) continue;
            if (!ShouldAudit(entry.Entity)) continue;
            if (entry.State == EntityState.Unchanged || entry.State == EntityState.Detached) continue;

            var entityId = GetEntityId(entry);
            if (entityId == Guid.Empty) continue;

            string? oldValues = null;
            string? newValues = null;

            switch (entry.State)
            {
                case EntityState.Added:
                    newValues = SerializeCurrentValues(entry);
                    break;
                case EntityState.Modified:
                    oldValues = SerializeModifiedOriginalValues(entry);
                    newValues = SerializeModifiedCurrentValues(entry);
                    break;
                case EntityState.Deleted:
                    oldValues = SerializeCurrentValues(entry);
                    break;
            }

            var auditEntry = AuditLogEntry.Create(
                entityType: entry.Entity.GetType().Name,
                entityId: entityId,
                action: entry.State.ToString(),
                oldValues: oldValues,
                newValues: newValues,
                userId: userId,
                userEmail: userEmail,
                ipAddress: ipAddress
            );

            auditEntries.Add(auditEntry);
        }

        if (auditEntries.Count > 0)
        {
            context.Set<AuditLogEntry>().AddRange(auditEntries);
        }
    }

    private static bool ShouldAudit(object entity)
    {
        // Audit Applicant and HousingSearch entities
        return entity is Applicant or HousingSearch;
    }

    private static Guid GetEntityId(EntityEntry entry)
    {
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        return idProperty?.CurrentValue is Guid id ? id : Guid.Empty;
    }

    private static string SerializeCurrentValues(EntityEntry entry)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            // Skip shadow properties and navigation properties
            if (property.Metadata.IsShadowProperty()) continue;

            var name = property.Metadata.Name;
            var value = property.CurrentValue;

            // Handle complex types by converting to string representation
            dict[name] = ConvertValue(value);
        }

        return JsonSerializer.Serialize(dict, JsonOptions);
    }

    private static string SerializeModifiedOriginalValues(EntityEntry entry)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (!property.IsModified) continue;

            var name = property.Metadata.Name;
            var value = property.OriginalValue;
            dict[name] = ConvertValue(value);
        }

        return JsonSerializer.Serialize(dict, JsonOptions);
    }

    private static string SerializeModifiedCurrentValues(EntityEntry entry)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (!property.IsModified) continue;

            var name = property.Metadata.Name;
            var value = property.CurrentValue;
            dict[name] = ConvertValue(value);
        }

        return JsonSerializer.Serialize(dict, JsonOptions);
    }

    private static object? ConvertValue(object? value)
    {
        return value switch
        {
            null => null,
            DateTime dt => dt.ToString("O"),
            DateTimeOffset dto => dto.ToString("O"),
            Enum e => e.ToString(),
            Guid g => g.ToString(),
            _ when value.GetType().IsPrimitive || value is string || value is decimal => value,
            _ => value.ToString()
        };
    }
}
