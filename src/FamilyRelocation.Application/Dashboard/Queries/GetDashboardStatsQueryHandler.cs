using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Dashboard.Queries;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        // Applicant stats
        var applicants = await _context.Set<Applicant>()
            .Where(a => !a.IsDeleted)
            .ToListAsync(ct);

        var byBoardDecision = applicants
            .GroupBy(a => a.BoardReview?.Decision.ToString() ?? "Pending")
            .ToDictionary(g => g.Key.ToLower(), g => g.Count());

        // Get housing searches for stage counts
        var housingSearches = await _context.Set<HousingSearch>().ToListAsync(ct);
        var searchByApplicant = housingSearches.ToDictionary(h => h.ApplicantId, h => h.Stage);

        var byStage = new Dictionary<string, int>
        {
            ["submitted"] = applicants.Count(a =>
                a.Status == ApplicationStatus.Submitted ||
                (a.BoardReview?.Decision == BoardDecision.Pending) ||
                (a.BoardReview?.Decision == BoardDecision.Deferred)),
            ["awaitingAgreements"] = applicants.Count(a =>
                a.Status == ApplicationStatus.Approved &&
                searchByApplicant.TryGetValue(a.Id, out var stage) &&
                stage == HousingSearchStage.AwaitingAgreements),
            ["searching"] = applicants.Count(a =>
                a.Status == ApplicationStatus.Approved &&
                searchByApplicant.TryGetValue(a.Id, out var stage) &&
                stage == HousingSearchStage.Searching),
            ["underContract"] = applicants.Count(a =>
                a.Status == ApplicationStatus.Approved &&
                searchByApplicant.TryGetValue(a.Id, out var stage) &&
                stage == HousingSearchStage.UnderContract),
            ["closed"] = applicants.Count(a =>
                a.Status == ApplicationStatus.Approved &&
                searchByApplicant.TryGetValue(a.Id, out var stage) &&
                (stage == HousingSearchStage.Closed || stage == HousingSearchStage.MovedIn))
        };

        // Property stats
        var properties = await _context.Set<Property>().ToListAsync(ct);
        var byStatus = properties
            .GroupBy(p => p.Status.ToString())
            .ToDictionary(g => g.Key.ToLower(), g => g.Count());

        return new DashboardStatsDto
        {
            Applicants = new ApplicantStatsDto
            {
                Total = applicants.Count,
                ByBoardDecision = byBoardDecision,
                ByStage = byStage
            },
            Properties = new PropertyStatsDto
            {
                Total = properties.Count,
                ByStatus = byStatus
            }
        };
    }
}
