using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Housing Search entity - represents an approved family's house-hunting journey.
/// Created when an applicant is approved by the board.
/// An applicant can have multiple housing searches (one-to-many), though 99% will have just one.
/// </summary>
public class HousingSearch : Entity<Guid>
{
    // Valid stage transitions (state machine) - only search-level stages
    private static readonly Dictionary<HousingSearchStage, HousingSearchStage[]> ValidTransitions = new()
    {
        [HousingSearchStage.AwaitingAgreements] = [HousingSearchStage.Searching],
        [HousingSearchStage.Searching] = [HousingSearchStage.UnderContract, HousingSearchStage.Paused],
        [HousingSearchStage.UnderContract] = [HousingSearchStage.Closed, HousingSearchStage.Searching],
        [HousingSearchStage.Closed] = [HousingSearchStage.MovedIn, HousingSearchStage.Searching],
        [HousingSearchStage.Paused] = [HousingSearchStage.Searching],
        [HousingSearchStage.MovedIn] = [],
    };

    public Guid HousingSearchId
    {
        get => Id;
        private set => Id = value;
    }

    // Parent Applicant
    public Guid ApplicantId { get; private set; }
    public virtual Applicant Applicant { get; private set; } = null!;

    // Status
    public HousingSearchStage Stage { get; private set; }
    public DateTime StageChangedDate { get; private set; }

    // Current Contract
    public Contract? CurrentContract { get; private set; }

    // Failed Contract History
    private readonly List<FailedContractAttempt> _failedContracts = new();
    public IReadOnlyList<FailedContractAttempt> FailedContracts => _failedContracts.AsReadOnly();

    // Move In
    public MovedInStatus? MovedInStatus { get; private set; }
    public DateTime? MovedInDate { get; private set; }

    // Housing Preferences
    public HousingPreferences? Preferences { get; private set; }

    // Notes
    public string? Notes { get; private set; }

    // Audit
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public Guid ModifiedBy { get; private set; }
    public DateTime ModifiedDate { get; private set; }
    public bool IsActive { get; private set; }

    private HousingSearch() { }

    /// <summary>
    /// Factory method to create a new housing search.
    /// Always starts in AwaitingAgreements stage (created when applicant is board-approved).
    /// Transitions to Searching once required agreements are signed.
    /// Preferences are typically copied from the Applicant when created on approval.
    /// </summary>
    public static HousingSearch Create(
        Guid applicantId,
        Guid createdBy,
        HousingPreferences? preferences = null)
    {
        if (applicantId == Guid.Empty)
            throw new ArgumentException("Applicant ID is required", nameof(applicantId));

        var housingSearch = new HousingSearch
        {
            HousingSearchId = Guid.NewGuid(),
            ApplicantId = applicantId,
            Stage = HousingSearchStage.AwaitingAgreements,
            StageChangedDate = DateTime.UtcNow,
            Preferences = preferences ?? HousingPreferences.Default(),
            IsActive = true,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
            ModifiedBy = createdBy,
            ModifiedDate = DateTime.UtcNow
        };

        housingSearch.AddDomainEvent(new HousingSearchStarted(housingSearch.HousingSearchId, applicantId));

        return housingSearch;
    }

    /// <summary>
    /// Start house hunting after agreements are signed.
    /// Transitions from AwaitingAgreements to Searching.
    /// </summary>
    public void StartSearching(Guid modifiedBy)
    {
        TransitionTo(HousingSearchStage.Searching, modifiedBy);
    }

    /// <summary>
    /// Transition to a new stage (validates against state machine)
    /// </summary>
    private void TransitionTo(HousingSearchStage newStage, Guid modifiedBy)
    {
        if (!ValidTransitions.TryGetValue(Stage, out var allowedTransitions) ||
            !allowedTransitions.Contains(newStage))
        {
            throw new InvalidOperationException(
                $"Cannot transition from {Stage} to {newStage}. " +
                $"Valid transitions: {(allowedTransitions?.Length > 0 ? string.Join(", ", allowedTransitions) : "none")}");
        }

        var oldStage = Stage;
        Stage = newStage;
        StageChangedDate = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new HousingSearchStageChanged(HousingSearchId, oldStage, newStage));
    }

    /// <summary>
    /// Pause the housing search (family taking a break)
    /// </summary>
    public void Pause(string? reason, Guid modifiedBy)
    {
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = string.IsNullOrEmpty(Notes)
                ? $"Paused: {reason}"
                : $"{Notes}\n\nPaused: {reason}";
        }

        TransitionTo(HousingSearchStage.Paused, modifiedBy);
    }

    /// <summary>
    /// Resume house hunting after a pause
    /// </summary>
    public void Resume(Guid modifiedBy)
    {
        TransitionTo(HousingSearchStage.Searching, modifiedBy);
    }

    /// <summary>
    /// Put property under contract
    /// </summary>
    public void PutUnderContract(
        Guid propertyId,
        Money contractPrice,
        DateTime? expectedClosingDate,
        Guid modifiedBy)
    {
        CurrentContract = new Contract(
            propertyId,
            contractPrice,
            DateTime.UtcNow,
            expectedClosingDate);

        TransitionTo(HousingSearchStage.UnderContract, modifiedBy);
    }

    /// <summary>
    /// Contract fell through - preserve history and return to searching
    /// </summary>
    public void ContractFellThrough(string? reason, Guid modifiedBy)
    {
        // Business rule: Can only mark contract as fallen through when there's a contract
        if (Stage != HousingSearchStage.UnderContract && Stage != HousingSearchStage.Closed)
            throw new InvalidOperationException(
                "Can only mark contract as fallen through when UnderContract or Closed.");

        // Preserve the failed contract in history
        if (CurrentContract != null)
        {
            var failedContract = new FailedContractAttempt(
                contract: CurrentContract,
                failedDate: DateTime.UtcNow,
                reason: reason);

            _failedContracts.Add(failedContract);
        }

        // Clear current contract
        CurrentContract = null;

        TransitionTo(HousingSearchStage.Searching, modifiedBy);
    }

    /// <summary>
    /// Record the closing (after closing has completed)
    /// </summary>
    public void RecordClosing(DateTime closingDate, Guid modifiedBy)
    {
        if (CurrentContract == null)
            throw new InvalidOperationException("Cannot record closing without a contract.");

        CurrentContract = CurrentContract.WithActualClosingDate(closingDate);
        TransitionTo(HousingSearchStage.Closed, modifiedBy);
    }

    /// <summary>
    /// Record that family has moved in
    /// </summary>
    public void RecordMovedIn(DateTime movedInDate, Guid modifiedBy)
    {
        MovedInDate = movedInDate;
        TransitionTo(HousingSearchStage.MovedIn, modifiedBy);
    }

    /// <summary>
    /// Set moved in status
    /// </summary>
    public void SetMovedInStatus(MovedInStatus status, DateTime? movedInDate, Guid modifiedBy)
    {
        MovedInStatus = status;
        MovedInDate = movedInDate ?? DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Update notes
    /// </summary>
    public void UpdateNotes(string? notes, Guid modifiedBy)
    {
        Notes = notes;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Update housing preferences for this search
    /// </summary>
    public void UpdatePreferences(HousingPreferences preferences, Guid modifiedBy)
    {
        Preferences = preferences ?? HousingPreferences.Default();
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new HousingPreferencesUpdated(ApplicantId));
    }

    /// <summary>
    /// Deactivate the housing search
    /// </summary>
    public void Deactivate(Guid modifiedBy)
    {
        IsActive = false;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivate the housing search
    /// </summary>
    public void Reactivate(Guid modifiedBy)
    {
        IsActive = true;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if housing search is currently under contract
    /// </summary>
    public bool IsUnderContract => Stage == HousingSearchStage.UnderContract && CurrentContract != null;

    /// <summary>
    /// Check if housing search is complete (moved in)
    /// </summary>
    public bool IsComplete => Stage == HousingSearchStage.MovedIn;

    /// <summary>
    /// Number of failed contract attempts
    /// </summary>
    public int FailedContractCount => _failedContracts.Count;
}
