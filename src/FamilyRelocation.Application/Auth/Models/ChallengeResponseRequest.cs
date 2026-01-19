namespace FamilyRelocation.Application.Auth.Models;

/// <summary>
/// Request to respond to an authentication challenge.
/// </summary>
public class ChallengeResponseRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Type of challenge being responded to.
    /// </summary>
    public required string ChallengeName { get; init; }

    /// <summary>
    /// Session token from the challenge response.
    /// </summary>
    public required string Session { get; init; }

    /// <summary>
    /// Challenge responses keyed by field name.
    /// </summary>
    public Dictionary<string, string> Responses { get; init; } = new();
}
