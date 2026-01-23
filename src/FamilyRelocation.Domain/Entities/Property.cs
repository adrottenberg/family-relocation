using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;

namespace FamilyRelocation.Domain.Entities;

public class Property : Entity<Guid>
{
    // Audit fields (managed manually since we extend Entity<Guid> for domain events)
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public Address Address { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public int Bedrooms { get; private set; }
    public decimal Bathrooms { get; private set; }
    public int? SquareFeet { get; private set; }
    public decimal? LotSize { get; private set; }
    public int? YearBuilt { get; private set; }
    public decimal? AnnualTaxes { get; private set; }
    public List<string> Features { get; private set; } = new();
    public ListingStatus Status { get; private set; }
    public string? MlsNumber { get; private set; }
    public string? Notes { get; private set; }
    public bool IsDeleted { get; private set; }

    private readonly List<PropertyPhoto> _photos = new();
    public IReadOnlyList<PropertyPhoto> Photos => _photos.AsReadOnly();

    private Property() { }

    public static Property Create(
        Address address,
        Money price,
        int bedrooms,
        decimal bathrooms,
        Guid createdBy,
        int? squareFeet = null,
        decimal? lotSize = null,
        int? yearBuilt = null,
        decimal? annualTaxes = null,
        List<string>? features = null,
        string? mlsNumber = null,
        string? notes = null)
    {
        if (bedrooms < 0)
            throw new ArgumentException("Bedrooms cannot be negative", nameof(bedrooms));
        if (bathrooms < 0)
            throw new ArgumentException("Bathrooms cannot be negative", nameof(bathrooms));

        var property = new Property
        {
            Id = Guid.NewGuid(),
            Address = address ?? throw new ArgumentNullException(nameof(address)),
            Price = price ?? throw new ArgumentNullException(nameof(price)),
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            SquareFeet = squareFeet,
            LotSize = lotSize,
            YearBuilt = yearBuilt,
            AnnualTaxes = annualTaxes,
            Features = features ?? new List<string>(),
            Status = ListingStatus.Active,
            MlsNumber = mlsNumber,
            Notes = notes,
            IsDeleted = false,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        property.AddDomainEvent(new PropertyCreated(property.Id));

        return property;
    }

    public void Update(
        Address address,
        Money price,
        int bedrooms,
        decimal bathrooms,
        Guid modifiedBy,
        int? squareFeet = null,
        decimal? lotSize = null,
        int? yearBuilt = null,
        decimal? annualTaxes = null,
        List<string>? features = null,
        string? mlsNumber = null,
        string? notes = null)
    {
        if (bedrooms < 0)
            throw new ArgumentException("Bedrooms cannot be negative", nameof(bedrooms));
        if (bathrooms < 0)
            throw new ArgumentException("Bathrooms cannot be negative", nameof(bathrooms));

        Address = address ?? throw new ArgumentNullException(nameof(address));
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;
        SquareFeet = squareFeet;
        LotSize = lotSize;
        YearBuilt = yearBuilt;
        AnnualTaxes = annualTaxes;
        Features = features ?? new List<string>();
        MlsNumber = mlsNumber;
        Notes = notes;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(ListingStatus newStatus, Guid modifiedBy)
    {
        Status = newStatus;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Delete(Guid deletedBy)
    {
        IsDeleted = true;
        ModifiedBy = deletedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void AddPhoto(PropertyPhoto photo)
    {
        if (_photos.Count >= 50)
            throw new InvalidOperationException("Maximum 50 photos allowed per property");
        _photos.Add(photo);
    }

    public void RemovePhoto(Guid photoId)
    {
        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photo != null)
            _photos.Remove(photo);
    }

    /// <summary>
    /// Gets the primary photo, or the first photo by display order if none is marked as primary.
    /// </summary>
    public PropertyPhoto? PrimaryPhoto =>
        _photos.FirstOrDefault(p => p.IsPrimary) ??
        _photos.OrderBy(p => p.DisplayOrder).FirstOrDefault();

    /// <summary>
    /// Sets the specified photo as the primary photo for this property.
    /// Clears primary flag from any other photos.
    /// </summary>
    public void SetPrimaryPhoto(Guid photoId, Guid modifiedBy)
    {
        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            throw new InvalidOperationException($"Photo with ID {photoId} not found on this property");

        // Clear primary flag from all photos
        foreach (var p in _photos)
        {
            p.SetAsPrimary(false);
        }

        // Set the specified photo as primary
        photo.SetAsPrimary(true);

        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public decimal CalculateMonthlyPayment(decimal downPayment, decimal annualInterestRate, int loanTermYears)
    {
        var loanAmount = Price.Amount - downPayment;
        if (loanAmount <= 0) return 0;

        var monthlyRate = annualInterestRate / 100 / 12;
        var numPayments = loanTermYears * 12;

        if (monthlyRate == 0)
            return loanAmount / numPayments;

        var monthlyPrincipalAndInterest = loanAmount *
            (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), numPayments)) /
            ((decimal)Math.Pow((double)(1 + monthlyRate), numPayments) - 1);

        var monthlyTaxes = (AnnualTaxes ?? 0) / 12;
        var estimatedInsurance = Price.Amount * 0.0035m / 12; // ~0.35% annually

        return monthlyPrincipalAndInterest + monthlyTaxes + estimatedInsurance;
    }
}

public class PropertyPhoto
{
    public Guid Id { get; private set; }
    public Guid PropertyId { get; private set; }
    public string Url { get; private set; } = null!;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private PropertyPhoto() { }

    public static PropertyPhoto Create(Guid propertyId, string url, string? description = null, int displayOrder = 0)
    {
        return new PropertyPhoto
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            Url = url ?? throw new ArgumentNullException(nameof(url)),
            Description = description,
            DisplayOrder = displayOrder,
            IsPrimary = false,
            UploadedAt = DateTime.UtcNow
        };
    }

    internal void SetAsPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}
