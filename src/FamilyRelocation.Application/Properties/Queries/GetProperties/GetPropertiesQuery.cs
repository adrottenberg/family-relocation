using FamilyRelocation.Application.Common.Models;
using FamilyRelocation.Application.Properties.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Properties.Queries.GetProperties;

public record GetPropertiesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Status = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int? MinBeds = null,
    string? City = null,
    string? SortBy = null,
    string? SortOrder = null
) : IRequest<PaginatedList<PropertyListDto>>;
