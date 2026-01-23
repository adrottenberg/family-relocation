using FamilyRelocation.Domain.Entities;

namespace FamilyRelocation.Application.Properties.DTOs;

/// <summary>
/// Extension methods for mapping between Property domain entities and DTOs.
/// </summary>
public static class PropertyMapper
{
    /// <summary>
    /// Maps a Property entity to a PropertyDto.
    /// </summary>
    public static PropertyDto ToDto(this Property property)
    {
        return new PropertyDto
        {
            Id = property.Id,
            Address = property.Address.ToPropertyAddressDto(),
            Price = property.Price.Amount,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            SquareFeet = property.SquareFeet,
            LotSize = property.LotSize,
            YearBuilt = property.YearBuilt,
            AnnualTaxes = property.AnnualTaxes,
            Features = property.Features.ToList(),
            Status = property.Status.ToString(),
            MlsNumber = property.MlsNumber,
            Notes = property.Notes,
            Photos = property.Photos.Select(p => p.ToDto()).ToList(),
            CreatedAt = property.CreatedAt,
            ModifiedAt = property.ModifiedAt
        };
    }

    /// <summary>
    /// Maps a Property entity to a PropertyListDto for list views.
    /// </summary>
    public static PropertyListDto ToListDto(this Property property)
    {
        // Generate proxy URL for primary photo
        var primaryPhotoUrl = property.PrimaryPhoto != null
            ? $"/api/properties/{property.Id}/photos/{property.PrimaryPhoto.Id}/image"
            : null;

        return new PropertyListDto
        {
            Id = property.Id,
            Street = property.Address.Street,
            City = property.Address.City,
            Price = property.Price.Amount,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            SquareFeet = property.SquareFeet,
            Status = property.Status.ToString(),
            MlsNumber = property.MlsNumber,
            PrimaryPhotoUrl = primaryPhotoUrl
        };
    }

    /// <summary>
    /// Maps an Address value object to a PropertyAddressDto.
    /// </summary>
    public static PropertyAddressDto ToPropertyAddressDto(this Domain.ValueObjects.Address address)
    {
        return new PropertyAddressDto
        {
            Street = address.Street,
            Street2 = address.Street2,
            City = address.City,
            State = address.State,
            ZipCode = address.ZipCode
        };
    }

    /// <summary>
    /// Maps a PropertyPhoto entity to a PropertyPhotoDto.
    /// Uses proxy URL for images to avoid direct S3 access issues.
    /// </summary>
    public static PropertyPhotoDto ToDto(this PropertyPhoto photo)
    {
        // Generate proxy URL instead of direct S3 URL
        var proxyUrl = $"/api/properties/{photo.PropertyId}/photos/{photo.Id}/image";

        return new PropertyPhotoDto
        {
            Id = photo.Id,
            Url = proxyUrl,
            Description = photo.Description,
            DisplayOrder = photo.DisplayOrder,
            IsPrimary = photo.IsPrimary,
            UploadedAt = photo.UploadedAt
        };
    }
}
