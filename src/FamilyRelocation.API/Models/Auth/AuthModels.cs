namespace FamilyRelocation.API.Models.Auth;

// Login
public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class LoginResponse
{
    public required string AccessToken { get; set; }
    public required string IdToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
}

// Refresh Token
public class RefreshTokenRequest
{
    /// <summary>
    /// The Cognito username (sub/UUID) from the tokens. Required for computing SECRET_HASH.
    /// </summary>
    public required string Username { get; set; }
    public required string RefreshToken { get; set; }
}

public class RefreshTokenResponse
{
    public required string AccessToken { get; set; }
    public required string IdToken { get; set; }
    public int ExpiresIn { get; set; }
}

// Password Reset
public class ForgotPasswordRequest
{
    public required string Email { get; set; }
}

public class ConfirmForgotPasswordRequest
{
    public required string Email { get; set; }
    public required string Code { get; set; }
    public required string NewPassword { get; set; }
}

// Email Confirmation
public class ResendConfirmationRequest
{
    public required string Email { get; set; }
}

public class ConfirmEmailRequest
{
    public required string Email { get; set; }
    public required string Code { get; set; }
}

// Auth Challenges (e.g., NEW_PASSWORD_REQUIRED)
public class ChallengeResponse
{
    public required string ChallengeName { get; set; }
    public required string Session { get; set; }
    public required string Message { get; set; }
}

public class ChallengeRequest
{
    public required string Email { get; set; }
    public required string Session { get; set; }
    public required string ChallengeName { get; set; }
    public required string NewPassword { get; set; }
}
