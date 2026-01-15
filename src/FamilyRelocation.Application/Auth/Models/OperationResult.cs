namespace FamilyRelocation.Application.Auth.Models;

public class OperationResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? ErrorMessage { get; init; }
    public AuthErrorType? ErrorType { get; init; }

    public static OperationResult SuccessResult(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    public static OperationResult ErrorResult(string message, AuthErrorType errorType) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = errorType
    };
}
