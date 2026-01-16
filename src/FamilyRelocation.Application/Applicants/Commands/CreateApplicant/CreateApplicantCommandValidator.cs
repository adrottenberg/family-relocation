using FluentValidation;

namespace FamilyRelocation.Application.Applicants.Commands.CreateApplicant;

public class CreateApplicantCommandValidator : AbstractValidator<CreateApplicantCommand>
{
    public CreateApplicantCommandValidator()
    {
        RuleFor(x => x.Husband)
            .NotNull().WithMessage("Husband information is required");

        RuleFor(x => x.Husband.FirstName)
            .NotEmpty().WithMessage("Husband's first name is required")
            .MaximumLength(100).WithMessage("Husband's first name cannot exceed 100 characters")
            .When(x => x.Husband != null);

        RuleFor(x => x.Husband.LastName)
            .NotEmpty().WithMessage("Husband's last name is required")
            .MaximumLength(100).WithMessage("Husband's last name cannot exceed 100 characters")
            .When(x => x.Husband != null);

        RuleFor(x => x.Husband.FatherName)
            .MaximumLength(100).WithMessage("Husband's father name cannot exceed 100 characters")
            .When(x => x.Husband != null && !string.IsNullOrEmpty(x.Husband.FatherName));

        RuleFor(x => x.Husband.Email)
            .EmailAddress().WithMessage("Husband's email must be a valid email address")
            .When(x => x.Husband != null && !string.IsNullOrEmpty(x.Husband.Email));

        // Note: Multiple primary phone numbers are allowed in input - the handler auto-demotes
        // to ensure only the first one marked as primary remains primary

        // Wife validation (optional, but if provided must have first name)
        RuleFor(x => x.Wife!.FirstName)
            .NotEmpty().WithMessage("Wife's first name is required when wife info is provided")
            .MaximumLength(100).WithMessage("Wife's first name cannot exceed 100 characters")
            .When(x => x.Wife != null);

        RuleFor(x => x.Wife!.MaidenName)
            .MaximumLength(100).WithMessage("Wife's maiden name cannot exceed 100 characters")
            .When(x => x.Wife != null && !string.IsNullOrEmpty(x.Wife.MaidenName));

        RuleFor(x => x.Wife!.Email)
            .EmailAddress().WithMessage("Wife's email must be a valid email address")
            .When(x => x.Wife != null && !string.IsNullOrEmpty(x.Wife.Email));

        // Address validation (optional, but if provided must have required fields)
        When(x => x.Address != null, () =>
        {
            RuleFor(x => x.Address!.Street)
                .NotEmpty().WithMessage("Street address is required")
                .MaximumLength(200).WithMessage("Street address cannot exceed 200 characters");

            RuleFor(x => x.Address!.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

            RuleFor(x => x.Address!.State)
                .NotEmpty().WithMessage("State is required")
                .Length(2).WithMessage("State must be a 2-letter code");

            RuleFor(x => x.Address!.ZipCode)
                .NotEmpty().WithMessage("Zip code is required")
                .Matches(@"^\d{5}(-\d{4})?$").WithMessage("Zip code must be in format 12345 or 12345-6789");
        });

        // Children validation
        RuleForEach(x => x.Children).ChildRules(child =>
        {
            child.RuleFor(c => c.Age)
                .InclusiveBetween(0, 50).WithMessage("Child age must be between 0 and 50");

            child.RuleFor(c => c.Gender)
                .Must(g => g == "Male" || g == "Female")
                .WithMessage("Child gender must be 'Male' or 'Female'");
        });

        RuleFor(x => x.CurrentKehila)
            .MaximumLength(200).WithMessage("Current kehila cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.CurrentKehila));

        RuleFor(x => x.ShabbosShul)
            .MaximumLength(200).WithMessage("Shabbos shul cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.ShabbosShul));
    }
}
