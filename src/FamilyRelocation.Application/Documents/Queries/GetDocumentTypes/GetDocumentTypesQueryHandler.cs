using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Documents.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Documents.Queries.GetDocumentTypes;

/// <summary>
/// Handler for GetDocumentTypesQuery.
/// </summary>
public class GetDocumentTypesQueryHandler : IRequestHandler<GetDocumentTypesQuery, List<DocumentTypeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDocumentTypesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DocumentTypeDto>> Handle(GetDocumentTypesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<DocumentType>().AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(dt => dt.IsActive);
        }

        return await query
            .OrderBy(dt => dt.DisplayName)
            .Select(dt => new DocumentTypeDto
            {
                Id = dt.Id,
                Name = dt.Name,
                DisplayName = dt.DisplayName,
                Description = dt.Description,
                IsActive = dt.IsActive,
                IsSystemType = dt.IsSystemType
            })
            .ToListAsync(cancellationToken);
    }
}
