namespace FamilyRelocation.Application.Auth.Models;

/// <summary>
/// Result of listing users from Cognito.
/// </summary>
public class UserListResult
{
    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// List of users.
    /// </summary>
    public List<UserDto> Users { get; init; } = new();

    /// <summary>
    /// Pagination token for the next page.
    /// </summary>
    public string? PaginationToken { get; init; }

    /// <summary>
    /// Error message on failure.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Type of error that occurred.
    /// </summary>
    public AuthErrorType? ErrorType { get; init; }

    /// <summary>
    /// Creates a successful result with users.
    /// </summary>
    public static UserListResult SuccessResult(List<UserDto> users, string? paginationToken = null) => new()
    {
        Success = true,
        Users = users,
        PaginationToken = paginationToken
    };

    /// <summary>
    /// Creates a failed result with error details.
    /// </summary>
    public static UserListResult ErrorResult(string message, AuthErrorType errorType) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = errorType
    };
}
