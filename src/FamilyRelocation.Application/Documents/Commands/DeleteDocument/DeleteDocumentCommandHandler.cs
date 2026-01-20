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

    public DeleteDocumentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteDocumentCommand command, CancellationToken cancellationToken)
    {
        var document = await _context.Set<ApplicantDocument>()
            .FirstOrDefaultAsync(d => d.Id == command.DocumentId, cancellationToken)
            ?? throw new NotFoundException("Document", command.DocumentId);

        _context.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
