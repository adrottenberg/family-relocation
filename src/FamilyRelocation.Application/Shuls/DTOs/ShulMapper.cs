using FamilyRelocation.Domain.Entities;

namespace FamilyRelocation.Application.Shuls.DTOs;

/// <summary>
/// Maps between Shul entities and DTOs.
/// </summary>
public static class ShulMapper
{
    public static ShulDto ToDto(Shul shul)
    {
        return new ShulDto
        {
            Id = shul.Id,
            Name = shul.Name,
            Address = new ShulAddressDto
            {
                Street = shul.Address.Street,
                Street2 = shul.Address.Street2,
                City = shul.Address.City,
                State = shul.Address.State,
                ZipCode = shul.Address.ZipCode
            },
            Location = shul.Location != null
                ? new ShulCoordinatesDto
                {
                    Latitude = shul.Location.Latitude,
                    Longitude = shul.Location.Longitude
                }
                : null,
            Rabbi = shul.Rabbi,
            Denomination = shul.Denomination,
            Website = shul.Website,
            Notes = shul.Notes,
            IsActive = shul.IsActive,
            CreatedAt = shul.CreatedAt,
            ModifiedAt = shul.ModifiedAt
        };
    }

    public static ShulListDto ToListDto(Shul shul)
    {
        return new ShulListDto
        {
            Id = shul.Id,
            Name = shul.Name,
            Street = shul.Address.Street,
            City = shul.Address.City,
            State = shul.Address.State,
            ZipCode = shul.Address.ZipCode,
            Rabbi = shul.Rabbi,
            Denomination = shul.Denomination,
            IsActive = shul.IsActive,
            Latitude = shul.Location?.Latitude,
            Longitude = shul.Location?.Longitude
        };
    }

    public static PropertyShulDistanceDto ToDistanceDto(PropertyShulDistance distance, Shul shul)
    {
        return new PropertyShulDistanceDto
        {
            ShulId = distance.ShulId,
            ShulName = shul.Name,
            DistanceMiles = distance.DistanceMiles,
            WalkingTimeMinutes = distance.WalkingTimeMinutes,
            CalculatedAt = distance.CalculatedAt
        };
    }
}
