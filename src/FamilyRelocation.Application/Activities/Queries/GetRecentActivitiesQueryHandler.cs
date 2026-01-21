using FamilyRelocation.Application.Common.Interfaces;
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
            Timestamp = a.Timestamp
        }).ToList();
    }
}

public class GetActivitiesByEntityQueryHandler : IRequestHandler<GetActivitiesByEntityQuery, List<ActivityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActivitiesByEntityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActivityDto>> Handle(GetActivitiesByEntityQuery request, CancellationToken ct)
    {
        var activities = await _context.Set<ActivityLog>()
            .Where(a => a.EntityType == request.EntityType && a.EntityId == request.EntityId)
            .OrderByDescending(a => a.Timestamp)
            .Take(50)
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
            Timestamp = a.Timestamp
        }).ToList();
    }
}
