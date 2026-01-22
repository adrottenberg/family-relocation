using FamilyRelocation.Domain.Entities;

namespace FamilyRelocation.Application.Showings.DTOs;

/// <summary>
/// Extension methods for mapping Showing entities to DTOs.
/// </summary>
public static class ShowingMapper
{
    /// <summary>
    /// Maps a Showing entity to a ShowingDto.
    /// Requires PropertyMatch with Property and HousingSearch with Applicant to be loaded.
    /// </summary>
    public static ShowingDto ToDto(this Showing showing, string? brokerUserName = null)
    {
        return new ShowingDto
        {
            Id = showing.Id,
            PropertyMatchId = showing.PropertyMatchId,
            ScheduledDate = showing.ScheduledDate,
            ScheduledTime = showing.ScheduledTime,
            Status = showing.Status.ToString(),
            Notes = showing.Notes,
            BrokerUserId = showing.BrokerUserId,
            BrokerUserName = brokerUserName,
            CreatedAt = showing.CreatedAt,
            ModifiedAt = showing.ModifiedAt,
            CompletedAt = showing.CompletedAt,
            PropertyId = showing.PropertyMatch.PropertyId,
            PropertyStreet = showing.PropertyMatch.Property.Address.Street,
            PropertyCity = showing.PropertyMatch.Property.Address.City,
            PropertyPrice = showing.PropertyMatch.Property.Price.Amount,
            PropertyPhotoUrl = showing.PropertyMatch.Property.PrimaryPhoto?.Url,
            ApplicantId = showing.PropertyMatch.HousingSearch.ApplicantId,
            ApplicantName = showing.PropertyMatch.HousingSearch.Applicant.FamilyName
        };
    }

    /// <summary>
    /// Maps a Showing entity to a ShowingListDto.
    /// Requires PropertyMatch with Property and HousingSearch with Applicant to be loaded.
    /// </summary>
    public static ShowingListDto ToListDto(this Showing showing)
    {
        return new ShowingListDto
        {
            Id = showing.Id,
            PropertyMatchId = showing.PropertyMatchId,
            ScheduledDate = showing.ScheduledDate,
            ScheduledTime = showing.ScheduledTime,
            Status = showing.Status.ToString(),
            BrokerUserId = showing.BrokerUserId,
            PropertyId = showing.PropertyMatch.PropertyId,
            PropertyStreet = showing.PropertyMatch.Property.Address.Street,
            PropertyCity = showing.PropertyMatch.Property.Address.City,
            PropertyPhotoUrl = showing.PropertyMatch.Property.PrimaryPhoto?.Url,
            ApplicantId = showing.PropertyMatch.HousingSearch.ApplicantId,
            ApplicantName = showing.PropertyMatch.HousingSearch.Applicant.FamilyName
        };
    }
}
