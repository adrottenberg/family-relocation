namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Physical address value object
/// </summary>
public sealed record Address
{
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

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("Zip code is required", nameof(zipCode));

        Street = street.Trim();
        Street2 = street2?.Trim();
        City = city.Trim();
        State = state.Trim().ToUpperInvariant();
        ZipCode = zipCode.Trim();
    }

    public string FullAddress => Street2 != null
        ? $"{Street}, {Street2}, {City}, {State} {ZipCode}"
        : $"{Street}, {City}, {State} {ZipCode}";

    public string SingleLine => FullAddress;

    public override string ToString() => FullAddress;
}
