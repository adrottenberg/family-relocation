using System.Text.RegularExpressions;

namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Email address value object with validation
/// </summary>
public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    // Private parameterless constructor for EF Core
    private Email() => Value = string.Empty;

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email address is required", nameof(value));

        value = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException("Invalid email address format", nameof(value));

        Value = value;
    }

    public static Email? FromString(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : new Email(value);
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
