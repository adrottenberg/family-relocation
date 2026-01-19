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
}
