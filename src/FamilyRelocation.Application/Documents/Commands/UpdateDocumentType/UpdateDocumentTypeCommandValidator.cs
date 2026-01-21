using FluentValidation;

namespace FamilyRelocation.Application.Documents.Commands.UpdateDocumentType;

/// <summary>
/// Validates the UpdateDocumentTypeCommand before processing.
/// </summary>
public class UpdateDocumentTypeCommandValidator : AbstractValidator<UpdateDocumentTypeCommand>
{
    /// <summary>
    /// Initializes validation rules for updating a document type.
    /// </summary>
    public UpdateDocumentTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Document type ID is required");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(200).WithMessage("Display name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
