using MediatR;

namespace FamilyRelocation.Application.Documents.Commands.UpdateDocumentType;

/// <summary>
/// Command to update an existing document type.
/// </summary>
public record UpdateDocumentTypeCommand(
    Guid Id,
    string DisplayName,
    string? Description
) : IRequest<bool>;
