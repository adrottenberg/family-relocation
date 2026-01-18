using FamilyRelocation.Application.Auth.Models;

namespace FamilyRelocation.Application.Auth;

public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RespondToChallengeAsync(ChallengeResponseRequest request);
    Task<TokenRefreshResult> RefreshTokensAsync(string username, string refreshToken);
    Task<OperationResult> RequestPasswordResetAsync(string email);
    Task<OperationResult> ConfirmPasswordResetAsync(string email, string code, string newPassword);
    Task<OperationResult> ResendConfirmationCodeAsync(string email);
    Task<OperationResult> ConfirmEmailAsync(string email, string code);

    /// <summary>
    /// Admin-only: Creates a new user in Cognito with a temporary password.
    /// The user will be required to change their password on first login.
    /// </summary>
    Task<RegisterUserResult> RegisterUserAsync(string email, string? temporaryPassword = null);
}
