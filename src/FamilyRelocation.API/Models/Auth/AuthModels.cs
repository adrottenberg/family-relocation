namespace FamilyRelocation.API.Models.Auth;

/// <summary>
/// Request to authenticate a user with email and password.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's password.
    /// </summary>
    public required string Password { get; set; }
}

/// <summary>
/// Successful login response containing JWT tokens.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT access token for API authorization.
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// JWT ID token containing user claims.
    /// </summary>
    public required string IdToken { get; set; }

    /// <summary>
    /// Refresh token for obtaining new access tokens (only on initial login).
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token expiration time in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }
}

/// <summary>
/// Request to refresh access tokens using a refresh token.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The Cognito username (sub/UUID) from the tokens. Required for computing SECRET_HASH.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The refresh token from the initial login.
    /// </summary>
    public required string RefreshToken { get; set; }
}

/// <summary>
/// Response containing refreshed access tokens.
/// </summary>
public class RefreshTokenResponse
{
    /// <summary>
    /// New JWT access token for API authorization.
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// New JWT ID token containing user claims.
    /// </summary>
    public required string IdToken { get; set; }

    /// <summary>
    /// Token expiration time in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }
}

/// <summary>
/// Request to initiate a password reset.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Email address of the user requesting password reset.
    /// </summary>
    public required string Email { get; set; }
}

/// <summary>
/// Request to complete a password reset with verification code.
/// </summary>
public class ConfirmForgotPasswordRequest
{
    /// <summary>
    /// Email address of the user.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Verification code sent to the user's email.
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// The new password to set.
    /// </summary>
    public required string NewPassword { get; set; }
}

/// <summary>
/// Request to resend an email confirmation code.
/// </summary>
public class ResendConfirmationRequest
{
    /// <summary>
    /// Email address to send the confirmation code to.
    /// </summary>
    public required string Email { get; set; }
}

/// <summary>
/// Request to confirm a user's email address.
/// </summary>
public class ConfirmEmailRequest
{
    /// <summary>
    /// Email address to confirm.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Verification code sent to the email.
    /// </summary>
    public required string Code { get; set; }
}

/// <summary>
/// Response indicating an authentication challenge is required.
/// </summary>
public class ChallengeResponse
{
    /// <summary>
    /// Type of challenge (e.g., NEW_PASSWORD_REQUIRED, SMS_MFA, SOFTWARE_TOKEN_MFA).
    /// </summary>
    public required string ChallengeName { get; set; }

    /// <summary>
    /// Session token to use when responding to the challenge.
    /// </summary>
    public required string Session { get; set; }

    /// <summary>
    /// Human-readable message describing the challenge.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// List of field names required to respond to this challenge.
    /// </summary>
    public required string[] RequiredFields { get; set; }
}

/// <summary>
/// Request to respond to an authentication challenge.
/// </summary>
public class ChallengeRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Type of challenge being responded to.
    /// </summary>
    public required string ChallengeName { get; set; }

    /// <summary>
    /// Session token from the challenge response.
    /// </summary>
    public required string Session { get; set; }

    /// <summary>
    /// Challenge responses using abstracted field names:
    /// - newPassword: For NEW_PASSWORD_REQUIRED challenge
    /// - mfaCode: For SMS_MFA challenge
    /// - totpCode: For SOFTWARE_TOKEN_MFA challenge
    /// - mfaSelection: For SELECT_MFA_TYPE challenge
    /// </summary>
    public Dictionary<string, string> Responses { get; set; } = new();
}

/// <summary>
/// Admin request to register a new user.
/// </summary>
public class RegisterUserRequest
{
    /// <summary>
    /// Email address for the new user.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Optional temporary password. If not provided, a random one will be generated.
    /// </summary>
    public string? TemporaryPassword { get; set; }
}

/// <summary>
/// Response after successfully registering a new user.
/// </summary>
public class RegisterUserResponse
{
    /// <summary>
    /// Cognito user ID (sub) of the created user.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Temporary password to share with the new user.
    /// </summary>
    public required string TemporaryPassword { get; set; }

    /// <summary>
    /// Success message.
    /// </summary>
    public required string Message { get; set; }
}
