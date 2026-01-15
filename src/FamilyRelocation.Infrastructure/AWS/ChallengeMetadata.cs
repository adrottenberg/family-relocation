namespace FamilyRelocation.Infrastructure.AWS;

internal static class ChallengeMetadata
{
    private static readonly Dictionary<string, ChallengeTypeInfo> Challenges = new()
    {
        ["NEW_PASSWORD_REQUIRED"] = new(
            "Please set a new password",
            new[] { "newPassword" },
            new Dictionary<string, string> { ["newPassword"] = "NEW_PASSWORD" }),
        ["SMS_MFA"] = new(
            "Enter the verification code sent to your phone",
            new[] { "mfaCode" },
            new Dictionary<string, string> { ["mfaCode"] = "SMS_MFA_CODE" }),
        ["SOFTWARE_TOKEN_MFA"] = new(
            "Enter the code from your authenticator app",
            new[] { "totpCode" },
            new Dictionary<string, string> { ["totpCode"] = "SOFTWARE_TOKEN_MFA_CODE" }),
        ["MFA_SETUP"] = new(
            "Set up multi-factor authentication",
            new[] { "totpCode" },
            new Dictionary<string, string> { ["totpCode"] = "SOFTWARE_TOKEN_MFA_CODE" }),
        ["SELECT_MFA_TYPE"] = new(
            "Choose your preferred MFA method",
            new[] { "mfaSelection" },
            new Dictionary<string, string> { ["mfaSelection"] = "ANSWER" }),
    };

    public static ChallengeTypeInfo GetInfo(string challengeName) =>
        Challenges.TryGetValue(challengeName, out var info)
            ? info
            : new ChallengeTypeInfo(
                "Additional verification required",
                Array.Empty<string>(),
                new Dictionary<string, string>());

    public static Dictionary<string, string> MapResponsesToCognito(
        string challengeName,
        Dictionary<string, string> userResponses)
    {
        var info = GetInfo(challengeName);
        var cognitoResponses = new Dictionary<string, string>();

        foreach (var (key, value) in userResponses)
        {
            if (info.FieldMapping.TryGetValue(key, out var cognitoKey))
            {
                cognitoResponses[cognitoKey] = value;
            }
            else
            {
                // Pass through unknown fields as-is (for edge cases)
                cognitoResponses[key] = value;
            }
        }

        return cognitoResponses;
    }
}

internal record ChallengeTypeInfo(
    string Message,
    string[] RequiredFields,
    Dictionary<string, string> FieldMapping);
