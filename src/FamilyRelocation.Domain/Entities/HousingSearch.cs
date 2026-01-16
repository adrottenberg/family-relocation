using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Housing Search aggregate root
/// Represents a family's house-hunting journey from start to completion
/// An applicant has one housing search that tracks their entire journey
/// </summary>
public class HousingSearch : Entity<Guid>
{
    // Valid stage transitions (state machine)
    private static readonly Dictionary<HousingSearchStage, HousingSearchStage[]> ValidTransitions = new()
    {
        [HousingSearchStage.Submitted] = [HousingSearchStage.HouseHunting, HousingSearchStage.Rejected],
        [HousingSearchStage.Rejected] = [],
        [HousingSearchStage.HouseHunting] = [HousingSearchStage.UnderContract, HousingSearchStage.Paused],
        [HousingSearchStage.UnderContract] = [HousingSearchStage.Closed, HousingSearchStage.HouseHunting],
        [HousingSearchStage.Closed] = [HousingSearchStage.MovedIn, HousingSearchStage.HouseHunting],
        [HousingSearchStage.Paused] = [HousingSearchStage.HouseHunting],
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

    // Search Number (e.g., "HS-2026-0001")
    public string SearchNumber { get; private set; } = null!;

    // Status
    public HousingSearchStage Stage { get; private set; }
    public DateTime StageChangedDate { get; private set; }

    // Current Contract (when under contract)
    public Guid? ContractPropertyId { get; private set; }
    public Money? ContractPrice { get; private set; }
    public DateTime? ContractDate { get; private set; }

    // Failed Contract History
    private readonly List<FailedContractAttempt> _failedContracts = new();
    public IReadOnlyList<FailedContractAttempt> FailedContracts => _failedContracts.AsReadOnly();

    // Closing
    public DateTime? ClosingDate { get; private set; }
    public DateTime? ActualClosingDate { get; private set; }

    // Move In
    public MovedInStatus? MovedInStatus { get; private set; }
    public DateTime? MovedInDate { get; private set; }

    // Housing Preferences (what they're looking for in this search)
    public Money? Budget { get; private set; }
    public int? MinBedrooms { get; private set; }
    public decimal? MinBathrooms { get; private set; }
    public List<string> RequiredFeatures { get; private set; } = new();
    public ShulProximityPreference? ShulProximity { get; private set; }
    public MoveTimeline? MoveTimeline { get; private set; }

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
    /// Factory method to create a new housing search
    /// </summary>
    public static HousingSearch Create(
        Guid applicantId,
        string searchNumber,
        Guid createdBy)
    {
        if (applicantId == Guid.Empty)
            throw new ArgumentException("Applicant ID is required", nameof(applicantId));

        if (string.IsNullOrWhiteSpace(searchNumber))
            throw new ArgumentException("Search number is required", nameof(searchNumber));

        var housingSearch = new HousingSearch
        {
            HousingSearchId = Guid.NewGuid(),
            ApplicantId = applicantId,
            SearchNumber = searchNumber,
            Stage = HousingSearchStage.Submitted,
            StageChangedDate = DateTime.UtcNow,
            RequiredFeatures = new List<string>(),
            ShulProximity = ShulProximityPreference.NoPreference(),
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
    /// Begin house hunting (called after board approves the Applicant)
    /// </summary>
    public void StartHouseHunting(Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.Submitted && Stage != HousingSearchStage.Paused)
            throw new InvalidOperationException($"Cannot start house hunting from {Stage} stage.");

        TransitionTo(HousingSearchStage.HouseHunting, modifiedBy);
    }

    /// <summary>
    /// Reject the housing search (board rejection or other disqualification)
    /// </summary>
    public void Reject(string? reason, Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.Submitted)
            throw new InvalidOperationException("Can only reject from Submitted stage.");

        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = string.IsNullOrEmpty(Notes)
                ? $"Rejected: {reason}"
                : $"{Notes}\n\nRejected: {reason}";
        }

        TransitionTo(HousingSearchStage.Rejected, modifiedBy);
    }

    /// <summary>
    /// Pause the housing search (family taking a break)
    /// </summary>
    public void Pause(string? reason, Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.HouseHunting)
            throw new InvalidOperationException("Can only pause when actively house hunting.");

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
        if (Stage != HousingSearchStage.Paused)
            throw new InvalidOperationException("Can only resume from Paused stage.");

        TransitionTo(HousingSearchStage.HouseHunting, modifiedBy);
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
        if (Stage != HousingSearchStage.HouseHunting)
            throw new InvalidOperationException($"Cannot go under contract from {Stage} stage. Must be in HouseHunting stage.");

        ContractPropertyId = propertyId;
        ContractPrice = contractPrice;
        ContractDate = DateTime.UtcNow;
        ClosingDate = expectedClosingDate;

        TransitionTo(HousingSearchStage.UnderContract, modifiedBy);
    }

    /// <summary>
    /// Contract fell through - preserve history and return to house hunting
    /// </summary>
    public void ContractFellThrough(string? reason, Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.UnderContract && Stage != HousingSearchStage.Closed)
            throw new InvalidOperationException("Can only mark contract as fallen through when UnderContract or Closed.");

        // Preserve the failed contract in history
        if (ContractPropertyId.HasValue && ContractPrice != null && ContractDate.HasValue)
        {
            var failedContract = new FailedContractAttempt(
                propertyId: ContractPropertyId.Value,
                contractPrice: ContractPrice,
                contractDate: ContractDate.Value,
                failedDate: DateTime.UtcNow,
                reason: reason);

            _failedContracts.Add(failedContract);
        }

        // Clear current contract info
        ContractPropertyId = null;
        ContractPrice = null;
        ContractDate = null;
        ClosingDate = null;

        TransitionTo(HousingSearchStage.HouseHunting, modifiedBy);
    }

    /// <summary>
    /// Record the closing (after closing has completed)
    /// </summary>
    public void RecordClosing(DateTime closingDate, Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.UnderContract)
            throw new InvalidOperationException("Can only record closing when UnderContract.");

        ActualClosingDate = closingDate;
        TransitionTo(HousingSearchStage.Closed, modifiedBy);
    }

    /// <summary>
    /// Record that family has moved in
    /// </summary>
    public void RecordMovedIn(DateTime movedInDate, Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.Closed)
            throw new InvalidOperationException("Can only record move-in when in Closed stage.");

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
    public void UpdateHousingPreferences(
        Money? budget,
        int? minBedrooms,
        decimal? minBathrooms,
        List<string>? features,
        ShulProximityPreference? shulProximity,
        MoveTimeline? moveTimeline,
        Guid modifiedBy)
    {
        Budget = budget;
        MinBedrooms = minBedrooms;
        MinBathrooms = minBathrooms;
        RequiredFeatures = features ?? new List<string>();
        ShulProximity = shulProximity ?? ShulProximityPreference.NoPreference();
        MoveTimeline = moveTimeline;
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
    public bool IsUnderContract => Stage == HousingSearchStage.UnderContract && ContractPropertyId.HasValue;

    /// <summary>
    /// Check if housing search is complete (moved in)
    /// </summary>
    public bool IsComplete => Stage == HousingSearchStage.MovedIn;

    /// <summary>
    /// Check if housing search was rejected
    /// </summary>
    public bool IsRejected => Stage == HousingSearchStage.Rejected;

    /// <summary>
    /// Number of failed contract attempts
    /// </summary>
    public int FailedContractCount => _failedContracts.Count;
}
