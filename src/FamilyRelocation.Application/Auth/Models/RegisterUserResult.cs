using System.Diagnostics.CodeAnalysis;

namespace FamilyRelocation.Application.Auth.Models;

/// <summary>
/// Result of an admin user registration operation.
/// </summary>
public class RegisterUserResult
{
    /// <summary>
    /// Indicates if user registration was successful.
    /// </summary>
    [MemberNotNullWhen(true, nameof(UserId), nameof(TemporaryPassword), nameof(Message))]
    public bool Success { get; init; }

    /// <summary>
    /// Cognito user ID (sub) of the created user.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Temporary password to share with the new user.
    /// </summary>
    public string? TemporaryPassword { get; init; }

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
    /// Creates a successful registration result.
    /// </summary>
    public static RegisterUserResult SuccessResult(string userId, string temporaryPassword, string message) => new()
    {
        Success = true,
        UserId = userId,
        TemporaryPassword = temporaryPassword,
        Message = message
    };

    /// <summary>
    /// Creates a failed registration result with error details.
    /// </summary>
    public static RegisterUserResult ErrorResult(string message, AuthErrorType errorType) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = errorType
    };
}
