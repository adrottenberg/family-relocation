using System.Text.RegularExpressions;

namespace FamilyRelocation.Domain.ValueObjects;

public enum PhoneType
{
    Mobile,
    Home,
    Work,
    Other
}

/// <summary>
/// Phone number value object with type (mobile, home, work)
/// </summary>
public sealed record PhoneNumber
{
    // Properties use 'private set' for EF Core ToJson() serialization compatibility
    public string Number { get; private set; }
    public PhoneType Type { get; private set; }
    public bool IsPrimary { get; private set; }

    // Private parameterless constructor for EF Core
    private PhoneNumber()
    {
        Number = string.Empty;
        Type = PhoneType.Mobile;
        IsPrimary = false;
    }

    public PhoneNumber(string number, PhoneType type = PhoneType.Mobile, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Phone number is required", nameof(number));

        // Normalize: remove all non-digit characters
        var digits = Regex.Replace(number, @"[^\d]", "");

        // Handle US numbers
        if (digits.Length == 10)
            digits = "1" + digits;

        if (digits.Length != 11 || !digits.StartsWith("1"))
            throw new ArgumentException("Invalid US phone number format", nameof(number));

        Number = digits;
        Type = type;
        IsPrimary = isPrimary;
    }

    public string Formatted => $"({Number[1..4]}) {Number[4..7]}-{Number[7..11]}";

    public string E164 => $"+{Number}";

    public override string ToString() => Formatted;
}
