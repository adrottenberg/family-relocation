using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Documents.Commands.DeleteDocumentType;

public class DeleteDocumentTypeCommandHandler : IRequestHandler<DeleteDocumentTypeCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteDocumentTypeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        var documentType = await _context.Set<DocumentType>()
            .FirstOrDefaultAsync(dt => dt.Id == request.Id, cancellationToken);

        if (documentType is null)
            return false;

        documentType.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
