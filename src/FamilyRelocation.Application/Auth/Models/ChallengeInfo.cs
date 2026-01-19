namespace FamilyRelocation.Application.Auth.Models;

/// <summary>
/// Information about an authentication challenge that requires user response.
/// </summary>
public class ChallengeInfo
{
    /// <summary>
    /// Type of challenge (e.g., NEW_PASSWORD_REQUIRED, SMS_MFA).
    /// </summary>
    public required string ChallengeName { get; init; }

    /// <summary>
    /// Session token to use when responding to the challenge.
    /// </summary>
    public required string Session { get; init; }

    /// <summary>
    /// Human-readable message describing the challenge.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// List of field names required to respond to this challenge.
    /// </summary>
    public required string[] RequiredFields { get; init; }
}
