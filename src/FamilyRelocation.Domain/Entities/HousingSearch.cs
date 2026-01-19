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

    // Required Agreements (must be signed before starting house hunting)
    public bool BrokerAgreementSigned { get; private set; }
    public string? BrokerAgreementDocumentUrl { get; private set; }
    public DateTime? BrokerAgreementSignedDate { get; private set; }

    public bool CommunityTakanosSigned { get; private set; }
    public string? CommunityTakanosDocumentUrl { get; private set; }
    public DateTime? CommunityTakanosSignedDate { get; private set; }

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
        Guid createdBy)
    {
        if (applicantId == Guid.Empty)
            throw new ArgumentException("Applicant ID is required", nameof(applicantId));

        var housingSearch = new HousingSearch
        {
            HousingSearchId = Guid.NewGuid(),
            ApplicantId = applicantId,
            Stage = HousingSearchStage.Submitted,
            StageChangedDate = DateTime.UtcNow,
            Preferences = HousingPreferences.Default(),
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
        // Business rule: StartHouseHunting is only for initial start or resuming
        // Use ContractFellThrough to return to house hunting after a failed contract
        if (Stage != HousingSearchStage.Submitted && Stage != HousingSearchStage.Paused)
            throw new InvalidOperationException(
                $"StartHouseHunting can only be called from Submitted or Paused stage. " +
                $"Use ContractFellThrough to return to house hunting from {Stage}.");

        // Business rule: Agreements must be signed before starting (only from Submitted)
        if (Stage == HousingSearchStage.Submitted && !AreAgreementsSigned)
            throw new InvalidOperationException(
                "Both broker agreement and community takanos must be signed with uploaded documents before starting house hunting.");

        TransitionTo(HousingSearchStage.HouseHunting, modifiedBy);
    }

    /// <summary>
    /// Record that the broker agreement has been signed and document uploaded.
    /// </summary>
    public void RecordBrokerAgreementSigned(string documentUrl, Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(documentUrl))
            throw new ArgumentException("Document URL is required", nameof(documentUrl));

        BrokerAgreementSigned = true;
        BrokerAgreementDocumentUrl = documentUrl;
        BrokerAgreementSignedDate = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Record that the community takanos agreement has been signed and document uploaded.
    /// </summary>
    public void RecordCommunityTakanosSigned(string documentUrl, Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(documentUrl))
            throw new ArgumentException("Document URL is required", nameof(documentUrl));

        CommunityTakanosSigned = true;
        CommunityTakanosDocumentUrl = documentUrl;
        CommunityTakanosSignedDate = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if all required agreements are signed with uploaded documents.
    /// </summary>
    public bool AreAgreementsSigned =>
        BrokerAgreementSigned && !string.IsNullOrEmpty(BrokerAgreementDocumentUrl) &&
        CommunityTakanosSigned && !string.IsNullOrEmpty(CommunityTakanosDocumentUrl);

    /// <summary>
    /// Reject the housing search (board rejection or other disqualification)
    /// </summary>
    public void Reject(string? reason, Guid modifiedBy)
    {
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
        CurrentContract = new Contract(
            propertyId,
            contractPrice,
            DateTime.UtcNow,
            expectedClosingDate);

        TransitionTo(HousingSearchStage.UnderContract, modifiedBy);
    }

    /// <summary>
    /// Contract fell through - preserve history and return to house hunting
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

        TransitionTo(HousingSearchStage.HouseHunting, modifiedBy);
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
    /// Check if housing search was rejected
    /// </summary>
    public bool IsRejected => Stage == HousingSearchStage.Rejected;

    /// <summary>
    /// Number of failed contract attempts
    /// </summary>
    public int FailedContractCount => _failedContracts.Count;
}
