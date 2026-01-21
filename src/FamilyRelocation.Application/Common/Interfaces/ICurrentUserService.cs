namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Service for accessing information about the current authenticated user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The current user's ID, or null if not authenticated.
    /// Consumers should decide how to handle null (e.g., use a well-known ID for anonymous operations).
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// The current user's email address, or null if not authenticated.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// The current user's display name, or null if not authenticated.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Indicates whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
