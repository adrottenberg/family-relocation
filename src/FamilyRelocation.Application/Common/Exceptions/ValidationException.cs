using FluentValidation.Results;

namespace FamilyRelocation.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a validation rule is violated.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Creates a new validation exception with the specified message.
    /// </summary>
    public ValidationException(string message) : base(message)
    {
        Errors = [message];
    }

    /// <summary>
    /// Creates a new validation exception with multiple errors.
    /// </summary>
    public ValidationException(IEnumerable<string> errors)
        : base(string.Join("; ", errors))
    {
        Errors = errors.ToList();
    }

    /// <summary>
    /// Creates a new validation exception from FluentValidation failures.
    /// </summary>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Errors = failures.Select(f => f.ErrorMessage).ToList();
    }

    /// <summary>
    /// List of validation errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; } = [];
}
