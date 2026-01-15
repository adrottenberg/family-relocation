namespace FamilyRelocation.Application.Auth.Models;

public class ChallengeResponseRequest
{
    public required string Email { get; init; }
    public required string ChallengeName { get; init; }
    public required string Session { get; init; }
    public Dictionary<string, string> Responses { get; init; } = new();
}
