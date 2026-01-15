namespace FamilyRelocation.Application.Auth.Models;

public class AuthTokens
{
    public required string AccessToken { get; init; }
    public required string IdToken { get; init; }
    public string? RefreshToken { get; init; }
    public int ExpiresIn { get; init; }
}
