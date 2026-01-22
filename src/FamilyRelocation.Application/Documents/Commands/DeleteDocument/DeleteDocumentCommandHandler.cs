using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Documents.Commands.DeleteDocument;

/// <summary>
/// Handler for DeleteDocumentCommand.
/// </summary>
public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IActivityLogger _activityLogger;

    public DeleteDocumentCommandHandler(
        IApplicationDbContext context,
        IActivityLogger activityLogger)
    {
        _context = context;
        _activityLogger = activityLogger;
    }

    public async Task<bool> Handle(DeleteDocumentCommand command, CancellationToken cancellationToken)
    {
        var document = await _context.Set<ApplicantDocument>()
            .Include(d => d.DocumentType)
            .FirstOrDefaultAsync(d => d.Id == command.DocumentId, cancellationToken)
            ?? throw new NotFoundException("Document", command.DocumentId);

        var applicantId = document.ApplicantId;
        var documentTypeName = document.DocumentType?.DisplayName ?? "Unknown";
        var fileName = document.FileName;

        _context.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);

        // Log activity
        await _activityLogger.LogAsync(
            "Applicant",
            applicantId,
            "DocumentDeleted",
            $"Deleted document: {documentTypeName} ({fileName})",
            cancellationToken);

        return true;
    }
}
