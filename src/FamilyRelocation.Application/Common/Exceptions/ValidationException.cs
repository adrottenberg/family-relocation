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
    /// List of validation errors if multiple were provided.
    /// </summary>
    public IReadOnlyList<string> Errors { get; } = [];
}
