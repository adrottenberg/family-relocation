using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Housing Search aggregate root
/// Represents a family's house-hunting journey from start to completion
/// An applicant typically has one active housing search, but may restart if they pause
/// </summary>
public class HousingSearch : Entity<Guid>
{
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
    /// Advance to the next stage
    /// </summary>
    public void AdvanceStage(HousingSearchStage newStage, Guid modifiedBy)
    {
        if (newStage <= Stage && newStage != HousingSearchStage.HouseHunting)
            throw new InvalidOperationException($"Cannot move from {Stage} to {newStage}. Stage must advance forward.");

        var oldStage = Stage;
        Stage = newStage;
        StageChangedDate = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new HousingSearchStageChanged(HousingSearchId, oldStage, newStage));
    }

    /// <summary>
    /// Set to Approved stage (after board approval)
    /// </summary>
    public void Approve(Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.Submitted)
            throw new InvalidOperationException($"Cannot approve housing search in {Stage} stage. Must be in Submitted stage.");

        AdvanceStage(HousingSearchStage.Approved, modifiedBy);
    }

    /// <summary>
    /// Begin house hunting
    /// </summary>
    public void StartHouseHunting(Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.Approved)
            throw new InvalidOperationException($"Cannot start house hunting from {Stage} stage. Must be Approved first.");

        AdvanceStage(HousingSearchStage.HouseHunting, modifiedBy);
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

        AdvanceStage(HousingSearchStage.UnderContract, modifiedBy);
    }

    /// <summary>
    /// Contract fell through - preserve history and return to house hunting
    /// </summary>
    public void ContractFellThrough(string? reason, Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.UnderContract)
            throw new InvalidOperationException("Can only mark contract as fallen through when UnderContract.");

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

        // Go back to house hunting
        var oldStage = Stage;
        Stage = HousingSearchStage.HouseHunting;
        StageChangedDate = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new HousingSearchStageChanged(HousingSearchId, oldStage, HousingSearchStage.HouseHunting));
    }

    /// <summary>
    /// Move to closing stage
    /// </summary>
    public void StartClosing(DateTime expectedClosingDate, Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.UnderContract)
            throw new InvalidOperationException("Can only start closing when UnderContract.");

        ClosingDate = expectedClosingDate;
        AdvanceStage(HousingSearchStage.Closing, modifiedBy);
    }

    /// <summary>
    /// Complete the closing
    /// </summary>
    public void CompleteClosing(DateTime actualClosingDate, Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.Closing)
            throw new InvalidOperationException("Can only complete closing when in Closing stage.");

        ActualClosingDate = actualClosingDate;
        AdvanceStage(HousingSearchStage.MovedIn, modifiedBy);
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
    /// Number of failed contract attempts
    /// </summary>
    public int FailedContractCount => _failedContracts.Count;
}
