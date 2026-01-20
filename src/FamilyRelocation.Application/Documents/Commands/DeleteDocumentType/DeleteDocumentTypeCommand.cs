using MediatR;

namespace FamilyRelocation.Application.Documents.Commands.DeleteDocumentType;

/// <summary>
/// Command to deactivate (soft delete) a document type.
/// </summary>
public record DeleteDocumentTypeCommand(Guid Id) : IRequest<bool>;
