using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Documents.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Documents.Queries.GetApplicantDocuments;

/// <summary>
/// Handler for GetApplicantDocumentsQuery.
/// </summary>
public class GetApplicantDocumentsQueryHandler : IRequestHandler<GetApplicantDocumentsQuery, List<ApplicantDocumentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetApplicantDocumentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ApplicantDocumentDto>> Handle(GetApplicantDocumentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<ApplicantDocument>()
            .Where(d => d.ApplicantId == request.ApplicantId)
            .Include(d => d.DocumentType)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new ApplicantDocumentDto
            {
                Id = d.Id,
                DocumentTypeId = d.DocumentTypeId,
                DocumentTypeName = d.DocumentType.DisplayName,
                FileName = d.FileName,
                StorageKey = d.StorageKey,
                ContentType = d.ContentType,
                FileSizeBytes = d.FileSizeBytes,
                UploadedAt = d.UploadedAt,
                UploadedBy = d.UploadedBy
            })
            .ToListAsync(cancellationToken);
    }
}
