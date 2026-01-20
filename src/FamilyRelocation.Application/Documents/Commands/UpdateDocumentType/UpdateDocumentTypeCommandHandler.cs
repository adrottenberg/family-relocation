using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Documents.Commands.UpdateDocumentType;

public class UpdateDocumentTypeCommandHandler : IRequestHandler<UpdateDocumentTypeCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateDocumentTypeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        var documentType = await _context.Set<DocumentType>()
            .FirstOrDefaultAsync(dt => dt.Id == request.Id, cancellationToken);

        if (documentType is null)
            return false;

        documentType.Update(request.DisplayName, request.Description);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
