using FamilyRelocation.Application.Common.Models;
using MediatR;

namespace FamilyRelocation.Application.Activities.Queries;

public record GetRecentActivitiesQuery(int Count = 10) : IRequest<List<ActivityDto>>;

public record GetActivitiesByEntityQuery(
    string EntityType,
    Guid EntityId,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginatedList<ActivityDto>>;

public class ActivityDto
{
    public required Guid Id { get; init; }
    public required string EntityType { get; init; }
    public required Guid EntityId { get; init; }
    public string? EntityDisplayName { get; init; }
    public required string Action { get; init; }
    public required string Description { get; init; }
    public Guid? UserId { get; init; }
    public string? UserName { get; init; }
    public required DateTime Timestamp { get; init; }

    // New fields for communication logging
    public required string Type { get; init; }
    public int? DurationMinutes { get; init; }
    public string? Outcome { get; init; }
    public Guid? FollowUpReminderId { get; init; }
}
