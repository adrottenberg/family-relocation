using System.Diagnostics.CodeAnalysis;

namespace FamilyRelocation.Application.Auth.Models;

public class RegisterUserResult
{
    [MemberNotNullWhen(true, nameof(UserId), nameof(TemporaryPassword), nameof(Message))]
    public bool Success { get; init; }

    public string? UserId { get; init; }
    public string? TemporaryPassword { get; init; }
    public string? Message { get; init; }
    public string? ErrorMessage { get; init; }
    public AuthErrorType? ErrorType { get; init; }

    public static RegisterUserResult SuccessResult(string userId, string temporaryPassword, string message) => new()
    {
        Success = true,
        UserId = userId,
        TemporaryPassword = temporaryPassword,
        Message = message
    };

    public static RegisterUserResult ErrorResult(string message, AuthErrorType errorType) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = errorType
    };
}
