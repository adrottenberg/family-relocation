namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Status of a property match between a HousingSearch and a Property.
/// </summary>
public enum PropertyMatchStatus
{
    /// <summary>Initial state - system or user identified potential match</summary>
    MatchIdentified = 0,

    /// <summary>Applicant/coordinator wants to see this property</summary>
    ShowingRequested = 1,

    /// <summary>After showing, applicant is interested in the property</summary>
    ApplicantInterested = 2,

    /// <summary>Offer has been submitted on this property</summary>
    OfferMade = 3,

    /// <summary>Applicant is not interested in this property</summary>
    ApplicantRejected = 4
}
