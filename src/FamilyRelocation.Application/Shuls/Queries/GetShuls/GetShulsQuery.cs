using FamilyRelocation.Application.Common.Models;
using FamilyRelocation.Application.Shuls.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Shuls.Queries.GetShuls;

public record GetShulsQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null,
    string? City = null,
    string? Denomination = null,
    bool IncludeInactive = false
) : IRequest<PaginatedList<ShulListDto>>;
