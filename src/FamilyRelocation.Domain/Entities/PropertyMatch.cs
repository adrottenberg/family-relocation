using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Represents a match between a HousingSearch and a Property.
/// Tracks the status of the match through the showing and offer process.
/// </summary>
public class PropertyMatch : Entity<Guid>
{
    public Guid HousingSearchId { get; private set; }
    public Guid PropertyId { get; private set; }
    public PropertyMatchStatus Status { get; private set; }
    public int MatchScore { get; private set; }
    public string? MatchDetails { get; private set; }
    public string? Notes { get; private set; }
    public bool IsAutoMatched { get; private set; }
    public decimal? OfferAmount { get; private set; }

    // Navigation properties
    public virtual HousingSearch HousingSearch { get; private set; } = null!;
    public virtual Property Property { get; private set; } = null!;

    // Audit fields
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }

    private PropertyMatch() { }

    /// <summary>
    /// Factory method to create a new property match.
    /// </summary>
    public static PropertyMatch Create(
        Guid housingSearchId,
        Guid propertyId,
        int matchScore,
        string? matchDetails,
        bool isAutoMatched,
        Guid createdBy,
        string? notes = null)
    {
        if (housingSearchId == Guid.Empty)
            throw new ArgumentException("Housing search ID is required", nameof(housingSearchId));
        if (propertyId == Guid.Empty)
            throw new ArgumentException("Property ID is required", nameof(propertyId));
        if (matchScore < 0 || matchScore > 100)
            throw new ArgumentException("Match score must be between 0 and 100", nameof(matchScore));

        return new PropertyMatch
        {
            Id = Guid.NewGuid(),
            HousingSearchId = housingSearchId,
            PropertyId = propertyId,
            Status = PropertyMatchStatus.MatchIdentified,
            MatchScore = matchScore,
            MatchDetails = matchDetails,
            Notes = notes,
            IsAutoMatched = isAutoMatched,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the match status.
    /// </summary>
    public void UpdateStatus(PropertyMatchStatus newStatus, Guid modifiedBy, string? notes = null)
    {
        Status = newStatus;
        if (notes != null)
        {
            Notes = notes;
        }
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the match as ShowingRequested.
    /// </summary>
    public void RequestShowing(Guid modifiedBy)
    {
        if (Status != PropertyMatchStatus.MatchIdentified)
            throw new InvalidOperationException("Can only request showing for matches in MatchIdentified status");

        UpdateStatus(PropertyMatchStatus.ShowingRequested, modifiedBy);
    }

    /// <summary>
    /// Marks the match as ApplicantInterested after a showing.
    /// </summary>
    public void MarkInterested(Guid modifiedBy, string? notes = null)
    {
        UpdateStatus(PropertyMatchStatus.ApplicantInterested, modifiedBy, notes);
    }

    /// <summary>
    /// Marks the match as ApplicantRejected.
    /// </summary>
    public void Reject(Guid modifiedBy, string? notes = null)
    {
        UpdateStatus(PropertyMatchStatus.ApplicantRejected, modifiedBy, notes);
    }

    /// <summary>
    /// Marks that an offer has been made on this property.
    /// </summary>
    public void MarkOfferMade(decimal offerAmount, Guid modifiedBy, string? notes = null)
    {
        if (offerAmount <= 0)
            throw new ArgumentException("Offer amount must be greater than zero", nameof(offerAmount));

        OfferAmount = offerAmount;
        UpdateStatus(PropertyMatchStatus.OfferMade, modifiedBy, notes);
    }

    /// <summary>
    /// Updates the notes for this match.
    /// </summary>
    public void UpdateNotes(string? notes, Guid modifiedBy)
    {
        Notes = notes;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Recalculates the match score.
    /// </summary>
    public void UpdateScore(int newScore, string? matchDetails, Guid modifiedBy)
    {
        if (newScore < 0 || newScore > 100)
            throw new ArgumentException("Match score must be between 0 and 100", nameof(newScore));

        MatchScore = newScore;
        MatchDetails = matchDetails;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }
}
