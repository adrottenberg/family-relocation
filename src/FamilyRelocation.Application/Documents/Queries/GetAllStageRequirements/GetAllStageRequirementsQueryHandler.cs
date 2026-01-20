using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Documents.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Documents.Queries.GetAllStageRequirements;

/// <summary>
/// Handler for GetAllStageRequirementsQuery.
/// </summary>
public class GetAllStageRequirementsQueryHandler : IRequestHandler<GetAllStageRequirementsQuery, List<StageTransitionRequirementDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllStageRequirementsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StageTransitionRequirementDto>> Handle(GetAllStageRequirementsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<StageTransitionRequirement>()
            .Include(r => r.DocumentType)
            .Select(r => new StageTransitionRequirementDto
            {
                Id = r.Id,
                FromStage = r.FromStage.ToString(),
                ToStage = r.ToStage.ToString(),
                DocumentTypeId = r.DocumentTypeId,
                DocumentTypeName = r.DocumentType.DisplayName,
                IsRequired = r.IsRequired
            })
            .ToListAsync(cancellationToken);
    }
}
