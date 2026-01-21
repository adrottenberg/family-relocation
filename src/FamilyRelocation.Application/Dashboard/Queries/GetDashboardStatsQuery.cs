using MediatR;

namespace FamilyRelocation.Application.Dashboard.Queries;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class DashboardStatsDto
{
    public ApplicantStatsDto Applicants { get; init; } = new();
    public PropertyStatsDto Properties { get; init; } = new();
}

public class ApplicantStatsDto
{
    public int Total { get; init; }
    public Dictionary<string, int> ByBoardDecision { get; init; } = new();
    public Dictionary<string, int> ByStage { get; init; } = new();
}

public class PropertyStatsDto
{
    public int Total { get; init; }
    public Dictionary<string, int> ByStatus { get; init; } = new();
}
