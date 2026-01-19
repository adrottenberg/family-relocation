using System.Diagnostics.CodeAnalysis;

namespace FamilyRelocation.Application.Auth.Models;

/// <summary>
/// Result of a login or challenge response operation.
/// </summary>
public class AuthResult
{
    /// <summary>
    /// Indicates if authentication was successful.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Tokens))]
    public bool Success { get; init; }

    /// <summary>
    /// JWT tokens on successful authentication.
    /// </summary>
    public AuthTokens? Tokens { get; init; }

    /// <summary>
    /// Challenge information if further verification is required.
    /// </summary>
    public ChallengeInfo? Challenge { get; init; }

    /// <summary>
    /// Error message on failure.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Type of error that occurred.
    /// </summary>
    public AuthErrorType? ErrorType { get; init; }

    /// <summary>
    /// Creates a successful authentication result with tokens.
    /// </summary>
    public static AuthResult SuccessResult(AuthTokens tokens) => new()
    {
        Success = true,
        Tokens = tokens
    };

    /// <summary>
    /// Creates a result indicating a challenge is required.
    /// </summary>
    public static AuthResult ChallengeResult(ChallengeInfo challenge) => new()
    {
        Success = false,
        Challenge = challenge
    };

    /// <summary>
    /// Creates a failed authentication result with error details.
    /// </summary>
    public static AuthResult ErrorResult(string message, AuthErrorType errorType) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = errorType
    };
}

/// <summary>
/// Types of authentication errors.
/// </summary>
public enum AuthErrorType
{
    /// <summary>Invalid email or password.</summary>
    InvalidCredentials,
    /// <summary>User email not yet confirmed.</summary>
    UserNotConfirmed,
    /// <summary>User must reset their password.</summary>
    PasswordResetRequired,
    /// <summary>Rate limit exceeded.</summary>
    TooManyRequests,
    /// <summary>Invalid or expired session token.</summary>
    InvalidSession,
    /// <summary>Invalid verification code.</summary>
    InvalidCode,
    /// <summary>Verification code has expired.</summary>
    ExpiredCode,
    /// <summary>Password does not meet requirements.</summary>
    InvalidPassword,
    /// <summary>User with this email already exists.</summary>
    UserAlreadyExists,
    /// <summary>Unknown or unspecified error.</summary>
    Unknown
}
