using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// A synagogue (shul) in the area, used for calculating walking distances from properties.
/// </summary>
public class Shul : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public Coordinates? Location { get; private set; }
    public string? Rabbi { get; private set; }
    public string? Denomination { get; private set; }
    public string? Website { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }

    private readonly List<PropertyShulDistance> _propertyDistances = new();
    public IReadOnlyList<PropertyShulDistance> PropertyDistances => _propertyDistances.AsReadOnly();

    private Shul() { }

    public static Shul Create(
        string name,
        Address address,
        Guid createdBy,
        Coordinates? location = null,
        string? rabbi = null,
        string? denomination = null,
        string? website = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Shul name is required", nameof(name));

        var shul = new Shul
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Address = address ?? throw new ArgumentNullException(nameof(address)),
            Location = location,
            Rabbi = rabbi?.Trim(),
            Denomination = denomination?.Trim(),
            Website = website?.Trim(),
            Notes = notes?.Trim(),
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        shul.AddDomainEvent(new ShulCreated(shul.Id));

        return shul;
    }

    public void Update(
        string name,
        Address address,
        Guid modifiedBy,
        Coordinates? location = null,
        string? rabbi = null,
        string? denomination = null,
        string? website = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Shul name is required", nameof(name));

        Name = name.Trim();
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Location = location;
        Rabbi = rabbi?.Trim();
        Denomination = denomination?.Trim();
        Website = website?.Trim();
        Notes = notes?.Trim();
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void SetLocation(Coordinates location, Guid modifiedBy)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate(Guid modifiedBy)
    {
        IsActive = false;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Activate(Guid modifiedBy)
    {
        IsActive = true;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Stores the calculated walking distance between a property and a shul.
/// </summary>
public class PropertyShulDistance
{
    public Guid Id { get; private set; }
    public Guid PropertyId { get; private set; }
    public Guid ShulId { get; private set; }

    /// <summary>
    /// Walking distance in miles
    /// </summary>
    public double DistanceMiles { get; private set; }

    /// <summary>
    /// Estimated walking time in minutes
    /// </summary>
    public int WalkingTimeMinutes { get; private set; }

    /// <summary>
    /// When the distance was last calculated
    /// </summary>
    public DateTime CalculatedAt { get; private set; }

    private PropertyShulDistance() { }

    public static PropertyShulDistance Create(
        Guid propertyId,
        Guid shulId,
        double distanceMiles,
        int walkingTimeMinutes)
    {
        if (distanceMiles < 0)
            throw new ArgumentException("Distance cannot be negative", nameof(distanceMiles));
        if (walkingTimeMinutes < 0)
            throw new ArgumentException("Walking time cannot be negative", nameof(walkingTimeMinutes));

        return new PropertyShulDistance
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            ShulId = shulId,
            DistanceMiles = distanceMiles,
            WalkingTimeMinutes = walkingTimeMinutes,
            CalculatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDistance(double distanceMiles, int walkingTimeMinutes)
    {
        if (distanceMiles < 0)
            throw new ArgumentException("Distance cannot be negative", nameof(distanceMiles));
        if (walkingTimeMinutes < 0)
            throw new ArgumentException("Walking time cannot be negative", nameof(walkingTimeMinutes));

        DistanceMiles = distanceMiles;
        WalkingTimeMinutes = walkingTimeMinutes;
        CalculatedAt = DateTime.UtcNow;
    }
}
