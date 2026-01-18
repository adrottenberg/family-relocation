using System.Diagnostics.CodeAnalysis;

namespace FamilyRelocation.Application.Auth.Models;

public class TokenRefreshResult
{
    [MemberNotNullWhen(true, nameof(Tokens))]
    public bool Success { get; init; }

    public AuthTokens? Tokens { get; init; }
    public string? ErrorMessage { get; init; }
    public AuthErrorType? ErrorType { get; init; }

    public static TokenRefreshResult SuccessResult(AuthTokens tokens) => new()
    {
        Success = true,
        Tokens = tokens
    };

    public static TokenRefreshResult ErrorResult(string message, AuthErrorType errorType) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = errorType
    };
}
