using MediatR;

namespace FamilyRelocation.Application.Documents.Commands.DeleteDocument;

/// <summary>
/// Command to delete a document.
/// </summary>
public record DeleteDocumentCommand(Guid DocumentId) : IRequest<bool>;
