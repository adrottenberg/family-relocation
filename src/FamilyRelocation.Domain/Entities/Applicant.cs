using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Applicant aggregate root - represents a family applying for relocation assistance.
/// Board review is at APPLICANT level (not HousingSearch level).
/// An approved applicant can have multiple housing searches (one-to-many).
/// </summary>
public class Applicant : Entity<Guid>
{
    public Guid ApplicantId
    {
        get => Id;
        private set => Id = value;
    }

    // Application Status (Submitted -> Approved/Rejected)
    public ApplicationStatus Status { get; private set; }

    // Family Members
    public HusbandInfo Husband { get; private set; } = null!;
    public SpouseInfo? Wife { get; private set; }

    // Family Address (shared)
    public Address? Address { get; private set; }

    // Children
    public List<Child> Children { get; private set; } = new();
    public int NumberOfChildren => Children.Count;

    // Community
    public string? CurrentKehila { get; private set; }
    public string? ShabbosShul { get; private set; }

    // Board Review (value object - set by staff after review)
    public BoardReview? BoardReview { get; private set; }

    // Navigation - collection of housing searches (one-to-many)
    private readonly List<HousingSearch> _housingSearches = new();
    public IReadOnlyCollection<HousingSearch> HousingSearches => _housingSearches.AsReadOnly();

    // Navigation - documents uploaded for this applicant
    private readonly List<ApplicantDocument> _documents = new();
    public IReadOnlyCollection<ApplicantDocument> Documents => _documents.AsReadOnly();

    // Audit
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public Guid ModifiedBy { get; private set; }
    public DateTime ModifiedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    private Applicant() { }

    /// <summary>
    /// Factory method to create a new applicant (family) from intake.
    /// Starts in Submitted status - no HousingSearch created until approved.
    /// </summary>
    public static Applicant Create(
        HusbandInfo husband,
        SpouseInfo? wife,
        Address? address,
        List<Child>? children,
        string? currentKehila,
        string? shabbosShul,
        Guid createdBy)
    {
        ArgumentNullException.ThrowIfNull(husband);

        var applicant = new Applicant
        {
            ApplicantId = Guid.NewGuid(),
            Status = ApplicationStatus.Submitted,
            Husband = husband,
            Wife = wife,
            Address = address,
            Children = children ?? new List<Child>(),
            CurrentKehila = currentKehila?.Trim(),
            ShabbosShul = shabbosShul?.Trim(),
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
            ModifiedBy = createdBy,
            ModifiedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        applicant.AddDomainEvent(new ApplicantCreated(applicant.ApplicantId));

        return applicant;
    }

    /// <summary>
    /// Update husband information
    /// </summary>
    public void UpdateHusband(HusbandInfo husband, Guid modifiedBy)
    {
        Husband = husband ?? throw new ArgumentNullException(nameof(husband));
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Update wife information
    /// </summary>
    public void UpdateWife(SpouseInfo? wife, Guid modifiedBy)
    {
        Wife = wife;
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Update family address
    /// </summary>
    public void UpdateAddress(Address? address, Guid modifiedBy)
    {
        Address = address;
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Update children information
    /// </summary>
    public void UpdateChildren(List<Child> children, Guid modifiedBy)
    {
        Children = children ?? new List<Child>();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Update community information
    /// </summary>
    public void UpdateCommunityInfo(
        string? currentKehila,
        string? shabbosShul,
        Guid modifiedBy)
    {
        CurrentKehila = currentKehila?.Trim();
        ShabbosShul = shabbosShul?.Trim();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Set board decision for this applicant.
    /// If approved, automatically creates first HousingSearch in Searching stage.
    /// </summary>
    public void SetBoardDecision(
        BoardDecision decision,
        string? notes,
        Guid reviewedByUserId,
        DateTime? reviewDate = null)
    {
        if (Status != ApplicationStatus.Submitted)
            throw new InvalidOperationException(
                $"Can only set board decision when status is Submitted. Current status: {Status}");

        BoardReview = new BoardReview(decision, reviewedByUserId, notes, reviewDate);
        SetModified(reviewedByUserId);

        // Update application status based on decision
        Status = decision switch
        {
            BoardDecision.Approved => ApplicationStatus.Approved,
            BoardDecision.Rejected => ApplicationStatus.Rejected,
            _ => Status // Pending or Deferred don't change status
        };

        AddDomainEvent(new ApplicantBoardDecisionMade(ApplicantId, decision, reviewedByUserId));

        // If approved, create the first housing search
        if (decision == BoardDecision.Approved)
        {
            var housingSearch = HousingSearch.Create(ApplicantId, reviewedByUserId);
            _housingSearches.Add(housingSearch);
        }
    }

    /// <summary>
    /// Start a new housing search (for approved applicants who need to search again).
    /// Use case: Previous search ended (moved in, or decided not to move), family wants to search again.
    /// </summary>
    public HousingSearch StartNewHousingSearch(Guid createdBy)
    {
        if (Status != ApplicationStatus.Approved)
            throw new InvalidOperationException(
                $"Can only start housing search for approved applicants. Current status: {Status}");

        // Deactivate any active searches
        foreach (var search in _housingSearches.Where(s => s.IsActive))
        {
            search.Deactivate(createdBy);
        }

        var newSearch = HousingSearch.Create(ApplicantId, createdBy);
        _housingSearches.Add(newSearch);
        SetModified(createdBy);

        return newSearch;
    }

    /// <summary>
    /// Get the active housing search (most common case - 99% of applicants have one)
    /// </summary>
    public HousingSearch? ActiveHousingSearch =>
        _housingSearches.FirstOrDefault(s => s.IsActive);

    /// <summary>
    /// Get the most recent housing search
    /// </summary>
    public HousingSearch? LatestHousingSearch =>
        _housingSearches.OrderByDescending(s => s.CreatedDate).FirstOrDefault();

    /// <summary>
    /// Check if applicant is approved by board
    /// </summary>
    public bool IsApproved => Status == ApplicationStatus.Approved;

    /// <summary>
    /// Check if applicant has pending board review
    /// </summary>
    public bool IsPendingBoardReview => Status == ApplicationStatus.Submitted;

    /// <summary>
    /// Convenience: Get family display name (husband's full name)
    /// </summary>
    public string FamilyName => Husband.FullName;

    /// <summary>
    /// True if this application was self-submitted (public application, not created by staff)
    /// </summary>
    public bool IsSelfSubmitted => CreatedBy == WellKnownIds.SelfSubmittedUserId;

    /// <summary>
    /// Soft delete the applicant
    /// </summary>
    public void Delete(Guid deletedBy)
    {
        IsDeleted = true;
        SetModified(deletedBy);
    }

    /// <summary>
    /// Restore a soft-deleted applicant
    /// </summary>
    public void Restore(Guid restoredBy)
    {
        IsDeleted = false;
        SetModified(restoredBy);
    }

    private void SetModified(Guid modifiedBy)
    {
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }
}
