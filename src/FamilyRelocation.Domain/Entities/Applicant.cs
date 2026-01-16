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

    public Guid? ProspectId { get; private set; }

    // Husband Info
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? FatherName { get; private set; }
    public string FullName => $"{FirstName} {LastName}";

    // Wife Info (value object)
    public SpouseInfo? Wife { get; private set; }

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

    // Board Review (value object)
    public BoardReview? BoardReview { get; private set; }

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
    /// Update husband's basic information
    /// </summary>
    public void UpdateHusbandInfo(
        string firstName,
        string lastName,
        string? fatherName,
        Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        FatherName = fatherName?.Trim();
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Update wife information
    /// </summary>
    public void UpdateWifeInfo(SpouseInfo? wifeInfo, Guid modifiedBy)
    {
        Wife = wifeInfo;
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
