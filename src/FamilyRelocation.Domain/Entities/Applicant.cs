using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Applicant aggregate root - represents a family applying for relocation assistance
/// Board review is at APPLICANT level (not HousingSearch level)
/// Housing preferences are on HousingSearch (they may change between search attempts)
/// </summary>
public class Applicant : Entity<Guid>
{
    public Guid ApplicantId
    {
        get => Id;
        private set => Id = value;
    }

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

    // Navigation - one housing search per applicant
    public virtual HousingSearch? HousingSearch { get; private set; }

    // Audit
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public Guid ModifiedBy { get; private set; }
    public DateTime ModifiedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    private Applicant() { }

    /// <summary>
    /// Factory method to create a new applicant (family) from intake
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
    /// Set board decision for this applicant
    /// Board review is at APPLICANT level, not HousingSearch level
    /// </summary>
    public void SetBoardDecision(
        BoardDecision decision,
        string? notes,
        Guid reviewedByUserId)
    {
        BoardReview = new BoardReview(decision, reviewedByUserId, notes);
        SetModified(reviewedByUserId);

        AddDomainEvent(new ApplicantBoardDecisionMade(ApplicantId, decision, reviewedByUserId));
    }

    /// <summary>
    /// Check if applicant is approved by board
    /// </summary>
    public bool IsApproved => BoardReview?.IsApproved ?? false;

    /// <summary>
    /// Check if applicant has pending board review
    /// </summary>
    public bool IsPendingBoardReview => BoardReview == null || BoardReview.IsPending;

    /// <summary>
    /// Convenience: Get family display name (husband's full name)
    /// </summary>
    public string FamilyName => Husband.FullName;

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
