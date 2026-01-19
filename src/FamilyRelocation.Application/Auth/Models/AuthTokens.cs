namespace FamilyRelocation.Application.Auth.Models;

/// <summary>
/// Container for JWT authentication tokens.
/// </summary>
public class AuthTokens
{
    /// <summary>
    /// JWT access token for API authorization.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// JWT ID token containing user claims.
    /// </summary>
    public required string IdToken { get; init; }

    /// <summary>
    /// Refresh token for obtaining new access tokens (only on initial login).
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Token expiration time in seconds.
    /// </summary>
    public int ExpiresIn { get; init; }
}
