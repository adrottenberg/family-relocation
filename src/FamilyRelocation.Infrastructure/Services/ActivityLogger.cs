using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;

namespace FamilyRelocation.Infrastructure.Services;

public class ActivityLogger : IActivityLogger
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ActivityLogger(IApplicationDbContext context, ICurrentUserService currentUserService)
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

        _context.Add(activity);
        await _context.SaveChangesAsync(ct);
    }
}
