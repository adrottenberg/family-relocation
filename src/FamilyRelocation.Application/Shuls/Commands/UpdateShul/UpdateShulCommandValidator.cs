using FluentValidation;

namespace FamilyRelocation.Application.Shuls.Commands.UpdateShul;

public class UpdateShulCommandValidator : AbstractValidator<UpdateShulCommand>
{
    public UpdateShulCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Shul ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Shul name is required")
            .MaximumLength(200).WithMessage("Shul name cannot exceed 200 characters");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required")
            .MaximumLength(200).WithMessage("Street cannot exceed 200 characters");

        RuleFor(x => x.Street2)
            .MaximumLength(100).WithMessage("Street2 cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Street2));

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required")
            .Length(2).WithMessage("State must be a 2-letter code");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required")
            .MaximumLength(10).WithMessage("Zip code cannot exceed 10 characters")
            .Matches(@"^\d{5}(-\d{4})?$").WithMessage("Zip code must be in format 12345 or 12345-6789");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.Rabbi)
            .MaximumLength(200).WithMessage("Rabbi name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Rabbi));

        RuleFor(x => x.Denomination)
            .MaximumLength(100).WithMessage("Denomination cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Denomination));

        RuleFor(x => x.Website)
            .MaximumLength(500).WithMessage("Website cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
