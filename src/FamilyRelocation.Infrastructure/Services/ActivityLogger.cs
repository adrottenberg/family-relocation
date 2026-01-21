using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Infrastructure.Persistence;

namespace FamilyRelocation.Infrastructure.Services;

public class ActivityLogger : IActivityLogger
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ActivityLogger(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task LogAsync(string entityType, Guid entityId, string action, string description, CancellationToken ct = default)
    {
        var activity = ActivityLog.Create(
            entityType: entityType,
            entityId: entityId,
            action: action,
            description: description,
            userId: _currentUserService.UserId,
            userName: _currentUserService.UserName
        );

        _context.ActivityLogs.Add(activity);
        await _context.SaveChangesAsync(ct);
    }
}
