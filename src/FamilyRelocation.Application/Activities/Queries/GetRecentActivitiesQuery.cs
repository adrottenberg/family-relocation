using MediatR;

namespace FamilyRelocation.Application.Activities.Queries;

public record GetRecentActivitiesQuery(int Count = 10) : IRequest<List<ActivityDto>>;

public record GetActivitiesByEntityQuery(string EntityType, Guid EntityId) : IRequest<List<ActivityDto>>;

public class ActivityDto
{
    public required Guid Id { get; init; }
    public required string EntityType { get; init; }
    public required Guid EntityId { get; init; }
    public required string Action { get; init; }
    public required string Description { get; init; }
    public Guid? UserId { get; init; }
    public string? UserName { get; init; }
    public required DateTime Timestamp { get; init; }
}
