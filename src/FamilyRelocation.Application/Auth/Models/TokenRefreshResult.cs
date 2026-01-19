using System.Diagnostics.CodeAnalysis;

namespace FamilyRelocation.Application.Auth.Models;

/// <summary>
/// Result of a token refresh operation.
/// </summary>
public class TokenRefreshResult
{
    /// <summary>
    /// Indicates if token refresh was successful.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Tokens))]
    public bool Success { get; init; }

    /// <summary>
    /// New JWT tokens on successful refresh.
    /// </summary>
    public AuthTokens? Tokens { get; init; }

    /// <summary>
    /// Error message on failure.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Type of error that occurred.
    /// </summary>
    public AuthErrorType? ErrorType { get; init; }

    /// <summary>
    /// Creates a successful token refresh result.
    /// </summary>
    public static TokenRefreshResult SuccessResult(AuthTokens tokens) => new()
    {
        Success = true,
        Tokens = tokens
    };

    /// <summary>
    /// Creates a failed token refresh result with error details.
    /// </summary>
    public static TokenRefreshResult ErrorResult(string message, AuthErrorType errorType) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = errorType
    };
}
