using FluentValidation;

namespace FamilyRelocation.Application.Properties.Commands.UpdateProperty;

/// <summary>
/// Validator for UpdatePropertyCommand.
/// </summary>
public class UpdatePropertyCommandValidator : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Property ID is required.");

        // Address validation
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street address is required.")
            .MaximumLength(200).WithMessage("Street address cannot exceed 200 characters.");

        RuleFor(x => x.Street2)
            .MaximumLength(100).WithMessage("Street2 cannot exceed 100 characters.")
            .When(x => x.Street2 != null);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .Length(2).WithMessage("State must be a 2-letter code.");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("ZIP code is required.")
            .Matches(@"^\d{5}(-\d{4})?$").WithMessage("ZIP code must be in format 12345 or 12345-6789.");

        // Property details validation
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be positive.")
            .LessThanOrEqualTo(100_000_000).WithMessage("Price seems unrealistic.");

        RuleFor(x => x.Bedrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bedrooms cannot be negative.")
            .LessThanOrEqualTo(50).WithMessage("Bedrooms count seems unrealistic.");

        RuleFor(x => x.Bathrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bathrooms cannot be negative.")
            .LessThanOrEqualTo(50).WithMessage("Bathrooms count seems unrealistic.");

        RuleFor(x => x.SquareFeet)
            .GreaterThan(0).WithMessage("Square feet must be positive.")
            .LessThanOrEqualTo(100_000).WithMessage("Square feet seems unrealistic.")
            .When(x => x.SquareFeet.HasValue);

        RuleFor(x => x.LotSize)
            .GreaterThan(0).WithMessage("Lot size must be positive.")
            .When(x => x.LotSize.HasValue);

        RuleFor(x => x.YearBuilt)
            .InclusiveBetween(1800, DateTime.UtcNow.Year + 5).WithMessage("Year built must be between 1800 and current year.")
            .When(x => x.YearBuilt.HasValue);

        RuleFor(x => x.AnnualTaxes)
            .GreaterThanOrEqualTo(0).WithMessage("Annual taxes cannot be negative.")
            .When(x => x.AnnualTaxes.HasValue);

        RuleFor(x => x.MlsNumber)
            .MaximumLength(50).WithMessage("MLS number cannot exceed 50 characters.")
            .When(x => x.MlsNumber != null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.")
            .When(x => x.Notes != null);
    }
}
