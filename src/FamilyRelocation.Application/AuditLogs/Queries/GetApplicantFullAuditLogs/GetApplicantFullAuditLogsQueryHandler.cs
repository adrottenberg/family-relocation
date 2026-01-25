using System.Text.Json;
using System.Text.RegularExpressions;
using FamilyRelocation.Application.AuditLogs.DTOs;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Common.Models;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.AuditLogs.Queries.GetApplicantFullAuditLogs;

/// <summary>
/// Handles the GetApplicantFullAuditLogsQuery to retrieve audit logs for an applicant
/// including their housing search, property matches, and showings.
/// </summary>
public class GetApplicantFullAuditLogsQueryHandler : IRequestHandler<GetApplicantFullAuditLogsQuery, PaginatedList<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetApplicantFullAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AuditLogDto>> Handle(
        GetApplicantFullAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        // Find the housing search ID(s) for this applicant
        var housingSearchIds = await _context.Set<HousingSearch>()
            .Where(h => h.ApplicantId == request.ApplicantId)
            .Select(h => h.Id)
            .ToListAsync(cancellationToken);

        // Find all property match IDs for these housing searches with property info
        var propertyMatches = await _context.Set<PropertyMatch>()
            .Where(pm => housingSearchIds.Contains(pm.HousingSearchId))
            .Select(pm => new { pm.Id, pm.PropertyId })
            .ToListAsync(cancellationToken);

        var propertyMatchIds = propertyMatches.Select(pm => pm.Id).ToList();

        // Find all showing IDs for these property matches with their property match IDs
        var showings = await _context.Set<Showing>()
            .Where(s => propertyMatchIds.Contains(s.PropertyMatchId))
            .Select(s => new { s.Id, s.PropertyMatchId })
            .ToListAsync(cancellationToken);

        var showingIds = showings.Select(s => s.Id).ToList();

        // Build a list of entity IDs to filter by
        var applicantIds = new HashSet<Guid> { request.ApplicantId };
        var housingSearchIdSet = housingSearchIds.ToHashSet();
        var propertyMatchIdSet = propertyMatchIds.ToHashSet();
        var showingIdSet = showingIds.ToHashSet();

        // Query audit logs that match any of the entity filters
        var query = _context.Set<AuditLogEntry>()
            .Where(a =>
                (a.EntityType == "Applicant" && applicantIds.Contains(a.EntityId)) ||
                (a.EntityType == "HousingSearch" && housingSearchIdSet.Contains(a.EntityId)) ||
                (a.EntityType == "PropertyMatch" && propertyMatchIdSet.Contains(a.EntityId)) ||
                (a.EntityType == "Showing" && showingIdSet.Contains(a.EntityId)));

        // Order by timestamp descending (most recent first)
        query = query.OrderByDescending(a => a.Timestamp);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and fetch
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Fetch applicant info for name resolution
        var applicant = await _context.Set<Applicant>()
            .Where(a => a.Id == request.ApplicantId)
            .Select(a => new { a.Id, a.Husband.FirstName, a.Husband.LastName })
            .FirstOrDefaultAsync(cancellationToken);

        var applicantName = applicant != null ? $"{applicant.FirstName} {applicant.LastName}" : "Unknown";

        // Fetch property info for property matches
        var propertyIds = propertyMatches.Select(pm => pm.PropertyId).Distinct().ToList();
        var properties = await _context.Set<Property>()
            .Where(p => propertyIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Address.Street, p.Address.City })
            .ToListAsync(cancellationToken);

        var propertyLookup = properties.ToDictionary(p => p.Id, p => $"{p.Street}, {p.City}");

        // Build property match to property address lookup
        var matchToPropertyLookup = propertyMatches.ToDictionary(
            pm => pm.Id,
            pm => propertyLookup.GetValueOrDefault(pm.PropertyId, "Unknown Property"));

        // Build showing to property address lookup
        var showingToPropertyLookup = showings.ToDictionary(
            s => s.Id,
            s => matchToPropertyLookup.GetValueOrDefault(s.PropertyMatchId, "Unknown Property"));

        // Extract all GUIDs from OldValues/NewValues to resolve
        var allGuidsInChanges = new HashSet<Guid>();
        var guidRegex = new Regex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");

        foreach (var item in items)
        {
            ExtractGuids(item.OldValues, allGuidsInChanges, guidRegex);
            ExtractGuids(item.NewValues, allGuidsInChanges, guidRegex);
        }

        // Look up user info from multiple sources
        var userIds = allGuidsInChanges.ToList();
        var userIdStrings = userIds.Select(id => id.ToString()).ToList();

        // Source 1: Audit logs themselves (they store UserId + UserEmail)
        var userEmailsFromAudit = await _context.Set<AuditLogEntry>()
            .Where(a => a.UserId != null && userIds.Contains(a.UserId.Value) && a.UserEmail != null)
            .Select(a => new { a.UserId, a.UserEmail })
            .Distinct()
            .ToListAsync(cancellationToken);

        // Source 2: UserRole table (stores CognitoUserId as string + Email)
        var userEmailsFromRoles = await _context.Set<UserRole>()
            .Where(ur => userIdStrings.Contains(ur.CognitoUserId))
            .Select(ur => new { ur.CognitoUserId, ur.Email })
            .Distinct()
            .ToListAsync(cancellationToken);

        // Build combined user lookup
        var userLookup = new Dictionary<Guid, string>();

        foreach (var u in userEmailsFromAudit.Where(u => u.UserId.HasValue && !string.IsNullOrEmpty(u.UserEmail)))
        {
            userLookup[u.UserId!.Value] = u.UserEmail!;
        }

        foreach (var ur in userEmailsFromRoles.Where(ur => !string.IsNullOrEmpty(ur.Email)))
        {
            if (Guid.TryParse(ur.CognitoUserId, out var cognitoGuid) && !userLookup.ContainsKey(cognitoGuid))
            {
                userLookup[cognitoGuid] = ur.Email;
            }
        }

        // Build resolved names dictionary for common IDs
        var resolvedNames = new Dictionary<string, string>
        {
            [request.ApplicantId.ToString()] = applicantName
        };

        // Add user names (using email from audit logs)
        foreach (var (userId, email) in userLookup)
        {
            resolvedNames[userId.ToString()] = email;
        }

        // Add housing search IDs (resolve to applicant name since it's their search)
        foreach (var hsId in housingSearchIds)
        {
            resolvedNames[hsId.ToString()] = $"{applicantName}'s Housing Search";
        }

        // Add property match IDs with their property addresses
        foreach (var pm in propertyMatches)
        {
            resolvedNames[pm.Id.ToString()] = matchToPropertyLookup.GetValueOrDefault(pm.Id, "Unknown Property");
            resolvedNames[pm.PropertyId.ToString()] = propertyLookup.GetValueOrDefault(pm.PropertyId, "Unknown Property");
        }

        // Add showing IDs
        foreach (var s in showings)
        {
            resolvedNames[s.Id.ToString()] = showingToPropertyLookup.GetValueOrDefault(s.Id, "Unknown Property");
        }

        // Find any GUIDs in changes that we haven't resolved yet
        var unresolvedGuids = allGuidsInChanges
            .Where(g => !resolvedNames.ContainsKey(g.ToString()))
            .ToList();

        if (unresolvedGuids.Count > 0)
        {
            // Try to resolve as properties
            var additionalProperties = await _context.Set<Property>()
                .Where(p => unresolvedGuids.Contains(p.Id))
                .Select(p => new { p.Id, p.Address.Street, p.Address.City })
                .ToListAsync(cancellationToken);

            foreach (var p in additionalProperties)
            {
                resolvedNames[p.Id.ToString()] = $"{p.Street}, {p.City}";
            }

            // Try to resolve as property matches
            var additionalMatches = await _context.Set<PropertyMatch>()
                .Where(pm => unresolvedGuids.Contains(pm.Id))
                .Select(pm => new { pm.Id, pm.Property.Address.Street, pm.Property.Address.City })
                .ToListAsync(cancellationToken);

            foreach (var pm in additionalMatches)
            {
                resolvedNames[pm.Id.ToString()] = $"{pm.Street}, {pm.City}";
            }

            // Try to resolve as showings
            var additionalShowings = await _context.Set<Showing>()
                .Where(s => unresolvedGuids.Contains(s.Id))
                .Select(s => new { s.Id, s.PropertyMatch.Property.Address.Street, s.PropertyMatch.Property.Address.City })
                .ToListAsync(cancellationToken);

            foreach (var s in additionalShowings)
            {
                resolvedNames[s.Id.ToString()] = $"{s.Street}, {s.City}";
            }

            // Try to resolve as applicants
            var additionalApplicants = await _context.Set<Applicant>()
                .Where(a => unresolvedGuids.Contains(a.Id))
                .Select(a => new { a.Id, a.Husband.FirstName, a.Husband.LastName })
                .ToListAsync(cancellationToken);

            foreach (var a in additionalApplicants)
            {
                resolvedNames[a.Id.ToString()] = $"{a.FirstName} {a.LastName}";
            }

            // Try to resolve as housing searches
            var additionalHousingSearches = await _context.Set<HousingSearch>()
                .Where(hs => unresolvedGuids.Contains(hs.Id))
                .Select(hs => new { hs.Id, hs.Applicant.Husband.FirstName, hs.Applicant.Husband.LastName })
                .ToListAsync(cancellationToken);

            foreach (var hs in additionalHousingSearches)
            {
                resolvedNames[hs.Id.ToString()] = $"{hs.FirstName} {hs.LastName}'s Housing Search";
            }
        }

        // Map to DTOs with resolved names
        var dtos = items.Select(a =>
        {
            var dto = a.ToDto();

            // Add resolved names
            dto.ResolvedNames = resolvedNames;

            // Add entity description based on entity type
            dto.EntityDescription = a.EntityType switch
            {
                "Applicant" => applicantName,
                "HousingSearch" => applicantName,
                "PropertyMatch" => matchToPropertyLookup.GetValueOrDefault(a.EntityId, "Unknown Property"),
                "Showing" => showingToPropertyLookup.GetValueOrDefault(a.EntityId, "Unknown Property"),
                _ => null
            };

            return dto;
        }).ToList();

        return new PaginatedList<AuditLogDto>(dtos, totalCount, page, pageSize);
    }

    private static void ExtractGuids(string? json, HashSet<Guid> guids, Regex guidRegex)
    {
        if (string.IsNullOrWhiteSpace(json)) return;

        foreach (Match match in guidRegex.Matches(json))
        {
            if (Guid.TryParse(match.Value, out var guid))
            {
                guids.Add(guid);
            }
        }
    }
}
