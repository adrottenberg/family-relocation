using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Activities.Queries;

/// <summary>
/// Query to get a single activity by ID.
/// </summary>
public record GetActivityByIdQuery(Guid ActivityId) : IRequest<ActivityDto?>;

/// <summary>
/// Handler for GetActivityByIdQuery.
/// </summary>
public class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, ActivityDto?>
{
    private readonly IApplicationDbContext _context;

    public GetActivityByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityDto?> Handle(GetActivityByIdQuery request, CancellationToken ct)
    {
        var activity = await _context.Set<ActivityLog>()
            .FirstOrDefaultAsync(a => a.Id == request.ActivityId, ct);

        if (activity == null)
            return null;

        // Fetch entity display name for Applicants
        string? entityDisplayName = null;
        if (activity.EntityType == "Applicant")
        {
            var applicant = await _context.Set<Applicant>()
                .Where(a => a.Id == activity.EntityId && !a.IsDeleted)
                .Select(a => new { a.Husband.FirstName, a.Husband.LastName })
                .FirstOrDefaultAsync(ct);

            if (applicant != null)
            {
                entityDisplayName = $"{applicant.FirstName} {applicant.LastName}";
            }
        }

        return new ActivityDto
        {
            Id = activity.Id,
            EntityType = activity.EntityType,
            EntityId = activity.EntityId,
            EntityDisplayName = entityDisplayName,
            Action = activity.Action,
            Description = activity.Description,
            UserId = activity.UserId,
            UserName = activity.UserName,
            Timestamp = activity.Timestamp,
            Type = activity.Type.ToString(),
            DurationMinutes = activity.DurationMinutes,
            Outcome = activity.Outcome,
            FollowUpReminderId = activity.FollowUpReminderId
        };
    }
}
