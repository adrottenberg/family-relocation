using FamilyRelocation.Application.Properties.DTOs;
using FamilyRelocation.Application.PropertyMatches.Services;
using FamilyRelocation.Domain.Entities;

namespace FamilyRelocation.Application.PropertyMatches.DTOs;

/// <summary>
/// Extension methods for mapping PropertyMatch entities to DTOs.
/// </summary>
public static class PropertyMatchMapper
{
    /// <summary>
    /// Maps a PropertyMatch entity to a PropertyMatchDto with full details.
    /// Requires Property and HousingSearch with Applicant to be loaded.
    /// </summary>
    public static PropertyMatchDto ToDto(this PropertyMatch match, IPropertyMatchingService matchingService)
    {
        return new PropertyMatchDto
        {
            Id = match.Id,
            HousingSearchId = match.HousingSearchId,
            PropertyId = match.PropertyId,
            Status = match.Status.ToString(),
            MatchScore = match.MatchScore,
            MatchDetails = matchingService.DeserializeMatchDetails(match.MatchDetails),
            Notes = match.Notes,
            IsAutoMatched = match.IsAutoMatched,
            OfferAmount = match.OfferAmount,
            CreatedAt = match.CreatedAt,
            ModifiedAt = match.ModifiedAt,
            Property = match.Property.ToListDto(),
            Applicant = new MatchApplicantDto
            {
                Id = match.HousingSearch.ApplicantId,
                FamilyName = match.HousingSearch.Applicant.FamilyName,
                HusbandFirstName = match.HousingSearch.Applicant.Husband?.FirstName,
                WifeFirstName = match.HousingSearch.Applicant.Wife?.FirstName
            }
        };
    }

    /// <summary>
    /// Maps a PropertyMatch entity to a PropertyMatchListDto for list views.
    /// Requires Property and HousingSearch with Applicant to be loaded.
    /// </summary>
    public static PropertyMatchListDto ToListDto(this PropertyMatch match, List<MatchShowingDto>? showings = null)
    {
        return new PropertyMatchListDto
        {
            Id = match.Id,
            HousingSearchId = match.HousingSearchId,
            PropertyId = match.PropertyId,
            Status = match.Status.ToString(),
            MatchScore = match.MatchScore,
            IsAutoMatched = match.IsAutoMatched,
            CreatedAt = match.CreatedAt,
            OfferAmount = match.OfferAmount,
            PropertyStreet = match.Property.Address.Street,
            PropertyCity = match.Property.Address.City,
            PropertyPrice = match.Property.Price.Amount,
            PropertyBedrooms = match.Property.Bedrooms,
            PropertyBathrooms = match.Property.Bathrooms,
            PropertyPhotoUrl = match.Property.PrimaryPhoto?.Url,
            ApplicantId = match.HousingSearch.ApplicantId,
            ApplicantName = match.HousingSearch.Applicant.FamilyName,
            Showings = showings ?? []
        };
    }
}
