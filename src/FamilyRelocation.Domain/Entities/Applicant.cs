using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Applicant aggregate root
/// Board review is at APPLICANT level (not HousingSearch level)
/// An applicant typically has one active housing search, but may restart if they pause
/// </summary>
public class Applicant : Entity<Guid>
{
    public Guid ApplicantId
    {
        get => Id;
        private set => Id = value;
    }

    public Guid? ProspectId { get; private set; }

    // Husband Info
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? FatherName { get; private set; }
    public string FullName => $"{FirstName} {LastName}";

    // Wife Info
    public string? WifeFirstName { get; private set; }
    public string? WifeMaidenName { get; private set; }
    public string? WifeFatherName { get; private set; }
    public string? WifeHighSchool { get; private set; }
    public string? WifeFullName => WifeFirstName != null
        ? (WifeMaidenName != null ? $"{WifeFirstName} {WifeMaidenName}" : WifeFirstName)
        : null;

    // Contact
    public Email Email { get; private set; } = null!;
    public List<PhoneNumber> PhoneNumbers { get; private set; } = new();
    public Address? Address { get; private set; }

    // Children
    public int NumberOfChildren => Children.Count;
    public List<Child> Children { get; private set; } = new();

    // Community
    public string? CurrentKehila { get; private set; }
    public string? ShabbosShul { get; private set; }

    // Housing Preferences
    public Money? Budget { get; private set; }
    public int? MinBedrooms { get; private set; }
    public decimal? MinBathrooms { get; private set; }
    public List<string> RequiredFeatures { get; private set; } = new();
    public ShulProximityPreference? ShulProximity { get; private set; }
    public MoveTimeline? MoveTimeline { get; private set; }
    public string? HousingNotes { get; private set; }

    // Board Review (AT APPLICANT LEVEL - not Application level)
    public DateTime? BoardReviewDate { get; private set; }
    public BoardDecision? BoardDecision { get; private set; }
    public string? BoardDecisionNotes { get; private set; }
    public Guid? BoardReviewedByUserId { get; private set; }

    // Navigation - one housing search per applicant (failed contracts tracked within)
    public virtual HousingSearch? HousingSearch { get; private set; }

    // Audit
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public Guid ModifiedBy { get; private set; }
    public DateTime ModifiedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    private Applicant() { }

    /// <summary>
    /// Factory method to create a new applicant from an application submission
    /// </summary>
    public static Applicant CreateFromApplication(
        string firstName,
        string lastName,
        string? fatherName,
        Email email,
        Address? address,
        string? currentKehila,
        string? shabbosShul,
        Guid createdBy,
        Guid? prospectId = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        var applicant = new Applicant
        {
            ApplicantId = Guid.NewGuid(),
            ProspectId = prospectId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            FatherName = fatherName?.Trim(),
            Email = email ?? throw new ArgumentNullException(nameof(email)),
            Address = address,
            CurrentKehila = currentKehila?.Trim(),
            ShabbosShul = shabbosShul?.Trim(),
            PhoneNumbers = new List<PhoneNumber>(),
            Children = new List<Child>(),
            RequiredFeatures = new List<string>(),
            ShulProximity = ShulProximityPreference.NoPreference(),
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
            ModifiedBy = createdBy,
            ModifiedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        applicant.AddDomainEvent(new ApplicantCreated(applicant.ApplicantId, prospectId));

        return applicant;
    }

    /// <summary>
    /// Update basic applicant information
    /// </summary>
    public void UpdateBasicInfo(
        string firstName,
        string lastName,
        string? fatherName,
        string? wifeFirstName,
        string? wifeMaidenName,
        string? wifeFatherName,
        string? wifeHighSchool,
        string? currentKehila,
        string? shabbosShul,
        Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        FatherName = fatherName?.Trim();
        WifeFirstName = wifeFirstName?.Trim();
        WifeMaidenName = wifeMaidenName?.Trim();
        WifeFatherName = wifeFatherName?.Trim();
        WifeHighSchool = wifeHighSchool?.Trim();
        CurrentKehila = currentKehila?.Trim();
        ShabbosShul = shabbosShul?.Trim();
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Update contact information
    /// </summary>
    public void UpdateContact(
        Email email,
        Address? address,
        List<PhoneNumber>? phoneNumbers,
        Guid modifiedBy)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Address = address;
        PhoneNumbers = phoneNumbers ?? new List<PhoneNumber>();
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Update children information
    /// </summary>
    public void UpdateChildren(List<Child> children, Guid modifiedBy)
    {
        Children = children ?? new List<Child>();
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Update housing preferences
    /// </summary>
    public void UpdateHousingPreferences(
        Money? budget,
        int? minBedrooms,
        decimal? minBathrooms,
        List<string>? features,
        ShulProximityPreference? shulProximity,
        MoveTimeline? moveTimeline,
        string? notes,
        Guid modifiedBy)
    {
        Budget = budget;
        MinBedrooms = minBedrooms;
        MinBathrooms = minBathrooms;
        RequiredFeatures = features ?? new List<string>();
        ShulProximity = shulProximity ?? ShulProximityPreference.NoPreference();
        MoveTimeline = moveTimeline;
        HousingNotes = notes;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new HousingPreferencesUpdated(ApplicantId));
    }

    /// <summary>
    /// Set board decision for this applicant
    /// Board review is at APPLICANT level, not Application level
    /// </summary>
    public void SetBoardDecision(
        BoardDecision decision,
        string? notes,
        Guid reviewedByUserId)
    {
        BoardDecision = decision;
        BoardDecisionNotes = notes;
        BoardReviewDate = DateTime.UtcNow;
        BoardReviewedByUserId = reviewedByUserId;
        ModifiedBy = reviewedByUserId;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new ApplicantBoardDecisionMade(ApplicantId, decision, reviewedByUserId));
    }

    /// <summary>
    /// Check if applicant is approved by board
    /// </summary>
    public bool IsApproved => BoardDecision == Enums.BoardDecision.Approved;

    /// <summary>
    /// Check if applicant has pending board review
    /// </summary>
    public bool IsPendingBoardReview => BoardDecision == null || BoardDecision == Enums.BoardDecision.Pending;

    /// <summary>
    /// Soft delete the applicant
    /// </summary>
    public void Delete(Guid deletedBy)
    {
        IsDeleted = true;
        ModifiedBy = deletedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore a soft-deleted applicant
    /// </summary>
    public void Restore(Guid restoredBy)
    {
        IsDeleted = false;
        ModifiedBy = restoredBy;
        ModifiedDate = DateTime.UtcNow;
    }
}
