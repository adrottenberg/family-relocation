using FamilyRelocation.Application.Auth.Models;

namespace FamilyRelocation.Application.Auth;

/// <summary>
/// Authentication service interface for user authentication operations.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="password">User's password.</param>
    /// <returns>Authentication result with tokens or challenge info.</returns>
    Task<AuthResult> LoginAsync(string email, string password);

    /// <summary>
    /// Responds to an authentication challenge.
    /// </summary>
    /// <param name="request">Challenge response details.</param>
    /// <returns>Authentication result with tokens or another challenge.</returns>
    Task<AuthResult> RespondToChallengeAsync(ChallengeResponseRequest request);

    /// <summary>
    /// Refreshes access tokens using a refresh token.
    /// </summary>
    /// <param name="username">Cognito username (sub/UUID).</param>
    /// <param name="refreshToken">Valid refresh token.</param>
    /// <returns>New access and ID tokens.</returns>
    Task<TokenRefreshResult> RefreshTokensAsync(string username, string refreshToken);

    /// <summary>
    /// Initiates a password reset by sending a verification code.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <returns>Operation result indicating success or failure.</returns>
    Task<OperationResult> RequestPasswordResetAsync(string email);

    /// <summary>
    /// Completes a password reset with the verification code.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="code">Verification code from email.</param>
    /// <param name="newPassword">New password to set.</param>
    /// <returns>Operation result indicating success or failure.</returns>
    Task<OperationResult> ConfirmPasswordResetAsync(string email, string code, string newPassword);

    /// <summary>
    /// Resends the email confirmation verification code.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <returns>Operation result indicating success or failure.</returns>
    Task<OperationResult> ResendConfirmationCodeAsync(string email);

    /// <summary>
    /// Confirms a user's email address with the verification code.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="code">Verification code from email.</param>
    /// <returns>Operation result indicating success or failure.</returns>
    Task<OperationResult> ConfirmEmailAsync(string email, string code);

    /// <summary>
    /// Admin-only: Creates a new user in Cognito with a temporary password.
    /// The user will be required to change their password on first login.
    /// </summary>
    /// <param name="email">Email address for the new user.</param>
    /// <param name="temporaryPassword">Optional temporary password. If null, one is generated.</param>
    /// <returns>Registration result with user ID and temporary password.</returns>
    Task<RegisterUserResult> RegisterUserAsync(string email, string? temporaryPassword = null);

    /// <summary>
    /// Admin-only: Lists users from Cognito user pool.
    /// </summary>
    /// <param name="filter">Optional filter string (e.g., "email ^= \"john\"").</param>
    /// <param name="limit">Maximum number of users to return (1-60).</param>
    /// <param name="paginationToken">Token for fetching the next page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of users with pagination token.</returns>
    Task<UserListResult> ListUsersAsync(
        string? filter = null,
        int limit = 60,
        string? paginationToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin-only: Gets a single user's details from Cognito.
    /// </summary>
    /// <param name="userId">The user's Cognito username (sub or email).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User details including roles.</returns>
    Task<GetUserResult> GetUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin-only: Gets the groups (roles) for a user.
    /// </summary>
    /// <param name="userId">The user's Cognito username.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of group names the user belongs to.</returns>
    Task<List<string>> GetUserGroupsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin-only: Updates a user's group memberships (roles).
    /// </summary>
    /// <param name="userId">The user's Cognito username.</param>
    /// <param name="roles">The complete list of roles the user should have.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result.</returns>
    Task<OperationResult> UpdateUserRolesAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin-only: Disables a user account (prevents login).
    /// </summary>
    /// <param name="userId">The user's Cognito username.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result.</returns>
    Task<OperationResult> DisableUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin-only: Enables a previously disabled user account.
    /// </summary>
    /// <param name="userId">The user's Cognito username.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result.</returns>
    Task<OperationResult> EnableUserAsync(string userId, CancellationToken cancellationToken = default);
}
