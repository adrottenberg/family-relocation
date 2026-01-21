using FluentValidation;

namespace FamilyRelocation.Application.Documents.Commands.DeleteDocument;

/// <summary>
/// Validates the DeleteDocumentCommand before processing.
/// </summary>
public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
{
    /// <summary>
    /// Initializes validation rules for deleting a document.
    /// </summary>
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");
    }
}
