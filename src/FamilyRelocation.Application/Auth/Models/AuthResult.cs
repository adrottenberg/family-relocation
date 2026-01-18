using System.Diagnostics.CodeAnalysis;

namespace FamilyRelocation.Application.Auth.Models;

public class AuthResult
{
    [MemberNotNullWhen(true, nameof(Tokens))]
    public bool Success { get; init; }

    public AuthTokens? Tokens { get; init; }
    public ChallengeInfo? Challenge { get; init; }
    public string? ErrorMessage { get; init; }
    public AuthErrorType? ErrorType { get; init; }

    public static AuthResult SuccessResult(AuthTokens tokens) => new()
    {
        Success = true,
        Tokens = tokens
    };

    public static AuthResult ChallengeResult(ChallengeInfo challenge) => new()
    {
        Success = false,
        Challenge = challenge
    };

    public static AuthResult ErrorResult(string message, AuthErrorType errorType) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = errorType
    };
}

public enum AuthErrorType
{
    InvalidCredentials,
    UserNotConfirmed,
    PasswordResetRequired,
    TooManyRequests,
    InvalidSession,
    InvalidCode,
    ExpiredCode,
    InvalidPassword,
    UserAlreadyExists,
    Unknown
}
