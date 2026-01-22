using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Common.Models;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Activities.Queries;

public class GetRecentActivitiesQueryHandler : IRequestHandler<GetRecentActivitiesQuery, List<ActivityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRecentActivitiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActivityDto>> Handle(GetRecentActivitiesQuery request, CancellationToken ct)
    {
        var activities = await _context.Set<ActivityLog>()
            .OrderByDescending(a => a.Timestamp)
            .Take(request.Count)
            .ToListAsync(ct);

        return activities.Select(a => new ActivityDto
        {
            Id = a.Id,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Action = a.Action,
            Description = a.Description,
            UserId = a.UserId,
            UserName = a.UserName,
            Timestamp = a.Timestamp,
            Type = a.Type.ToString(),
            DurationMinutes = a.DurationMinutes,
            Outcome = a.Outcome,
            FollowUpReminderId = a.FollowUpReminderId
        }).ToList();
    }
}

public class GetActivitiesByEntityQueryHandler : IRequestHandler<GetActivitiesByEntityQuery, PaginatedList<ActivityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActivitiesByEntityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ActivityDto>> Handle(GetActivitiesByEntityQuery request, CancellationToken ct)
    {
        var query = _context.Set<ActivityLog>()
            .Where(a => a.EntityType == request.EntityType && a.EntityId == request.EntityId);

        var totalCount = await query.CountAsync(ct);

        var activities = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = activities.Select(a => new ActivityDto
        {
            Id = a.Id,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Action = a.Action,
            Description = a.Description,
            UserId = a.UserId,
            UserName = a.UserName,
            Timestamp = a.Timestamp,
            Type = a.Type.ToString(),
            DurationMinutes = a.DurationMinutes,
            Outcome = a.Outcome,
            FollowUpReminderId = a.FollowUpReminderId
        }).ToList();

        return new PaginatedList<ActivityDto>(items, totalCount, request.Page, request.PageSize);
    }
}
