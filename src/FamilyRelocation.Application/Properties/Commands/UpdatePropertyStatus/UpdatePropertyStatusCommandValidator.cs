using FluentValidation;

namespace FamilyRelocation.Application.Properties.Commands.UpdatePropertyStatus;

/// <summary>
/// Validator for UpdatePropertyStatusCommand.
/// </summary>
public class UpdatePropertyStatusCommandValidator : AbstractValidator<UpdatePropertyStatusCommand>
{
    private static readonly string[] ValidStatuses = { "Active", "UnderContract", "Sold", "OffMarket" };

    public UpdatePropertyStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Property ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(BeValidStatus).WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
    }

    private static bool BeValidStatus(string status)
    {
        return ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
}
