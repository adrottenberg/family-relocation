using FluentValidation;

namespace FamilyRelocation.Application.Documents.Commands.CreateDocumentType;

/// <summary>
/// Validates the CreateDocumentTypeCommand before processing.
/// </summary>
public class CreateDocumentTypeCommandValidator : AbstractValidator<CreateDocumentTypeCommand>
{
    /// <summary>
    /// Initializes validation rules for creating a document type.
    /// </summary>
    public CreateDocumentTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]*$")
            .WithMessage("Name must start with a letter and contain only letters, numbers, and underscores");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(200).WithMessage("Display name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
