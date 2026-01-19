namespace FamilyRelocation.Application.Auth.Models;

/// <summary>
/// Result of a simple authentication operation (password reset, email confirmation, etc.).
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Success message.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Error message on failure.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Type of error that occurred.
    /// </summary>
    public AuthErrorType? ErrorType { get; init; }

    /// <summary>
    /// Creates a successful operation result.
    /// </summary>
    public static OperationResult SuccessResult(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    /// <summary>
    /// Creates a failed operation result with error details.
    /// </summary>
    public static OperationResult ErrorResult(string message, AuthErrorType errorType) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = errorType
    };
}
