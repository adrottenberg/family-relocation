namespace FamilyRelocation.Application.Auth.Models;

public class ChallengeInfo
{
    public required string ChallengeName { get; init; }
    public required string Session { get; init; }
    public required string Message { get; init; }
    public required string[] RequiredFields { get; init; }
}
