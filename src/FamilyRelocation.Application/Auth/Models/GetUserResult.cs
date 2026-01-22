namespace FamilyRelocation.Application.Auth.Models;

/// <summary>
/// Result of getting a single user from Cognito.
/// </summary>
public class GetUserResult
{
    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The user details.
    /// </summary>
    public UserDto? User { get; init; }

    /// <summary>
    /// Error message on failure.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Type of error that occurred.
    /// </summary>
    public AuthErrorType? ErrorType { get; init; }

    /// <summary>
    /// Creates a successful result with user details.
    /// </summary>
    public static GetUserResult SuccessResult(UserDto user) => new()
    {
        Success = true,
        User = user
    };

    /// <summary>
    /// Creates a failed result with error details.
    /// </summary>
    public static GetUserResult ErrorResult(string message, AuthErrorType errorType) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = errorType
    };
}
