using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Documents.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Documents.Queries.GetStageRequirements;

/// <summary>
/// Handler for GetStageRequirementsQuery.
/// </summary>
public class GetStageRequirementsQueryHandler : IRequestHandler<GetStageRequirementsQuery, StageTransitionRequirementsDto>
{
    private readonly IApplicationDbContext _context;

    public GetStageRequirementsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StageTransitionRequirementsDto> Handle(GetStageRequirementsQuery request, CancellationToken cancellationToken)
    {
        var requirements = await _context.Set<StageTransitionRequirement>()
            .Where(r => r.FromStage == request.FromStage && r.ToStage == request.ToStage)
            .Include(r => r.DocumentType)
            .Select(r => new
            {
                r.DocumentTypeId,
                DocumentTypeName = r.DocumentType.DisplayName,
                r.IsRequired
            })
            .ToListAsync(cancellationToken);

        // Get uploaded documents for the applicant if provided
        HashSet<Guid> uploadedDocumentTypeIds = new();
        if (request.ApplicantId.HasValue)
        {
            uploadedDocumentTypeIds = (await _context.Set<ApplicantDocument>()
                .Where(d => d.ApplicantId == request.ApplicantId.Value)
                .Select(d => d.DocumentTypeId)
                .ToListAsync(cancellationToken))
                .ToHashSet();
        }

        return new StageTransitionRequirementsDto
        {
            FromStage = request.FromStage.ToString(),
            ToStage = request.ToStage.ToString(),
            Requirements = requirements
                .Select(r => new DocumentRequirementDto
                {
                    DocumentTypeId = r.DocumentTypeId,
                    DocumentTypeName = r.DocumentTypeName,
                    IsRequired = r.IsRequired,
                    IsUploaded = uploadedDocumentTypeIds.Contains(r.DocumentTypeId)
                })
                .ToList()
        };
    }
}
