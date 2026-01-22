namespace FamilyRelocation.Application.Auth.Models;

/// <summary>
/// Represents a system user from Cognito.
/// </summary>
public class UserDto
{
    /// <summary>
    /// User's unique identifier (Cognito sub).
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's display name (if set).
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// User's assigned roles (from Cognito groups).
    /// </summary>
    public List<string> Roles { get; init; } = new();

    /// <summary>
    /// User's account status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Whether the user's email is verified.
    /// </summary>
    public bool EmailVerified { get; init; }

    /// <summary>
    /// Whether MFA is enabled for this user.
    /// </summary>
    public bool MfaEnabled { get; init; }

    /// <summary>
    /// When the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the user last logged in.
    /// </summary>
    public DateTime? LastLogin { get; init; }
}
