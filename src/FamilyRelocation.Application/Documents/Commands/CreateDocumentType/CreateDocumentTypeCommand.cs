using MediatR;

namespace FamilyRelocation.Application.Documents.Commands.CreateDocumentType;

/// <summary>
/// Command to create a new document type.
/// </summary>
public record CreateDocumentTypeCommand(
    string Name,
    string DisplayName,
    string? Description
) : IRequest<Guid>;
