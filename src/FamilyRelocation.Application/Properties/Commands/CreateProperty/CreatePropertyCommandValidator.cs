using FluentValidation;

namespace FamilyRelocation.Application.Properties.Commands.CreateProperty;

public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street address is required")
            .MaximumLength(200).WithMessage("Street address cannot exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required")
            .Length(2).WithMessage("State must be a 2-letter code");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("ZIP code is required")
            .Matches(@"^\d{5}(-\d{4})?$").WithMessage("ZIP code must be in format 12345 or 12345-6789");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Bedrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bedrooms cannot be negative");

        RuleFor(x => x.Bathrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bathrooms cannot be negative");

        RuleFor(x => x.SquareFeet)
            .GreaterThan(0).When(x => x.SquareFeet.HasValue)
            .WithMessage("Square feet must be greater than 0");

        RuleFor(x => x.YearBuilt)
            .InclusiveBetween(1800, DateTime.UtcNow.Year + 1).When(x => x.YearBuilt.HasValue)
            .WithMessage("Year built must be between 1800 and next year");

        RuleFor(x => x.AnnualTaxes)
            .GreaterThanOrEqualTo(0).When(x => x.AnnualTaxes.HasValue)
            .WithMessage("Annual taxes cannot be negative");
    }
}
