using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;

namespace FamilyRelocation.Application.Documents.Commands.CreateDocumentType;

public class CreateDocumentTypeCommandHandler : IRequestHandler<CreateDocumentTypeCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateDocumentTypeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        var documentType = DocumentType.Create(
            request.Name,
            request.DisplayName,
            request.Description,
            isSystemType: false
        );

        _context.Add(documentType);
        await _context.SaveChangesAsync(cancellationToken);

        return documentType.Id;
    }
}
