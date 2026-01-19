using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Common.Models;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Queries.GetApplicants;

/// <summary>
/// Query to get a paginated list of applicants with search, filter, and sort options.
/// </summary>
public record GetApplicantsQuery : IRequest<PaginatedList<ApplicantListDto>>
{
    /// <summary>
    /// Page number (1-based). Default: 1
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of items per page. Default: 20, Max: 100
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Search term to match against name, email, or phone.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Filter by board decision (Pending, Approved, Rejected, Deferred).
    /// </summary>
    public string? BoardDecision { get; init; }

    /// <summary>
    /// Filter by city.
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// Filter by created date (on or after).
    /// </summary>
    public DateTime? CreatedAfter { get; init; }

    /// <summary>
    /// Filter by created date (on or before).
    /// </summary>
    public DateTime? CreatedBefore { get; init; }

    /// <summary>
    /// Filter by housing search stage (Submitted, HouseHunting, UnderContract, Closed, Paused, Rejected).
    /// </summary>
    public string? Stage { get; init; }

    /// <summary>
    /// Sort field. Options: familyName, createdDate, boardReviewDate. Default: createdDate
    /// </summary>
    public string SortBy { get; init; } = "createdDate";

    /// <summary>
    /// Sort order. Options: asc, desc. Default: desc
    /// </summary>
    public string SortOrder { get; init; } = "desc";
}
