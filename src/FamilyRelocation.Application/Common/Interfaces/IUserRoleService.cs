namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Service for managing user roles in the database.
/// </summary>
public interface IUserRoleService
{
    /// <summary>
    /// Gets the roles for a user by their Cognito user ID.
    /// </summary>
    Task<IReadOnlyList<string>> GetUserRolesAsync(string cognitoUserId, CancellationToken ct = default);

    /// <summary>
    /// Gets the roles for a user by their email address.
    /// </summary>
    Task<IReadOnlyList<string>> GetUserRolesByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Sets the roles for a user, replacing any existing roles.
    /// </summary>
    Task SetUserRolesAsync(
        string cognitoUserId,
        string email,
        IEnumerable<string> roles,
        string? createdBy = null,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a role to a user if they don't already have it.
    /// </summary>
    Task AddRoleAsync(
        string cognitoUserId,
        string email,
        string role,
        string? createdBy = null,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    Task RemoveRoleAsync(string cognitoUserId, string role, CancellationToken ct = default);
}
