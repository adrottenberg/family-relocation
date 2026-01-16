using System.Text.RegularExpressions;

namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Physical address value object (US addresses)
/// </summary>
public sealed record Address
{
    private static readonly Regex ZipCodeRegex = new(@"^\d{5}(-\d{4})?$", RegexOptions.Compiled);
    private static readonly HashSet<string> ValidStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY",
        "DC", "PR", "VI", "GU", "AS", "MP"
    };

    public string Street { get; }
    public string? Street2 { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }

    // Private parameterless constructor for EF Core
    private Address()
    {
        Street = string.Empty;
        City = string.Empty;
        State = string.Empty;
        ZipCode = string.Empty;
    }

    public Address(string street, string city, string state, string zipCode, string? street2 = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street address is required", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State is required", nameof(state));

        state = state.Trim().ToUpperInvariant();
        if (state.Length != 2 || !ValidStates.Contains(state))
            throw new ArgumentException("State must be a valid 2-letter US state code", nameof(state));

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("Zip code is required", nameof(zipCode));

        zipCode = zipCode.Trim();
        if (!ZipCodeRegex.IsMatch(zipCode))
            throw new ArgumentException("Zip code must be in format 12345 or 12345-6789", nameof(zipCode));

        Street = street.Trim();
        Street2 = street2?.Trim();
        City = city.Trim();
        State = state;
        ZipCode = zipCode;
    }

    public string FullAddress => Street2 != null
        ? $"{Street}, {Street2}, {City}, {State} {ZipCode}"
        : $"{Street}, {City}, {State} {ZipCode}";

    public override string ToString() => FullAddress;
}
