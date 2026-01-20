using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.Entities;

public class HousingSearchTests
{
    private readonly Guid _applicantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateHousingSearch()
    {
        // Arrange & Act
        var housingSearch = HousingSearch.Create(
            applicantId: _applicantId,
            createdBy: _userId);

        // Assert
        housingSearch.ApplicantId.Should().Be(_applicantId);
        housingSearch.Stage.Should().Be(HousingSearchStage.Submitted);
        housingSearch.IsActive.Should().BeTrue();
        housingSearch.CreatedBy.Should().Be(_userId);
        housingSearch.Id.Should().NotBeEmpty();
        housingSearch.Preferences.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldRaiseHousingSearchStartedEvent()
    {
        // Arrange & Act
        var housingSearch = HousingSearch.Create(
            applicantId: _applicantId,
            createdBy: _userId);

        // Assert
        housingSearch.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<HousingSearchStarted>()
            .Which.ApplicantId.Should().Be(_applicantId);
    }

    [Fact]
    public void Create_WithEmptyApplicantId_ShouldThrow()
    {
        // Arrange & Act
        var act = () => HousingSearch.Create(
            applicantId: Guid.Empty,
            createdBy: _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Applicant ID*");
    }

    [Fact]
    public void StartHouseHunting_FromBoardApproved_ShouldMoveToHouseHunting()
    {
        // Arrange
        var housingSearch = CreateBoardApprovedSearch();
        housingSearch.ClearDomainEvents();

        // Act
        housingSearch.StartHouseHunting(_userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.HouseHunting);
        housingSearch.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<HousingSearchStageChanged>()
            .Which.NewStage.Should().Be(HousingSearchStage.HouseHunting);
    }

    [Fact]
    public void ApproveBoardReview_FromSubmitted_ShouldMoveToBoardApproved()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();
        housingSearch.ClearDomainEvents();

        // Act
        housingSearch.ApproveBoardReview(_userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.BoardApproved);
        housingSearch.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<HousingSearchStageChanged>()
            .Which.NewStage.Should().Be(HousingSearchStage.BoardApproved);
    }

    [Fact]
    public void ApproveBoardReview_WithRequiredDocuments_ShouldAutoTransitionToHouseHunting()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();
        housingSearch.ClearDomainEvents();

        // Act - Approve with required documents already uploaded
        housingSearch.ApproveBoardReview(_userId, hasRequiredDocuments: true);

        // Assert - Should auto-transition to HouseHunting
        housingSearch.Stage.Should().Be(HousingSearchStage.HouseHunting);
        housingSearch.DomainEvents.Should().HaveCount(2);
    }

    [Fact]
    public void ApproveBoardReview_FromNonSubmitted_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();

        // Act
        var act = () => housingSearch.ApproveBoardReview(_userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Submitted*");
    }

    [Fact]
    public void StartHouseHunting_FromSubmitted_ShouldThrow()
    {
        // Arrange - can't call StartHouseHunting from Submitted (must be BoardApproved first)
        var housingSearch = CreateTestHousingSearch();

        // Act
        var act = () => housingSearch.StartHouseHunting(_userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*BoardApproved*");
    }

    [Fact]
    public void StartHouseHunting_FromUnderContract_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();
        housingSearch.PutUnderContract(Guid.NewGuid(), new Money(450000), null, _userId);

        // Act
        var act = () => housingSearch.StartHouseHunting(_userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_FromSubmitted_ShouldMoveToRejected()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();
        housingSearch.ClearDomainEvents();

        // Act
        housingSearch.Reject("Does not meet criteria", _userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.Rejected);
        housingSearch.IsRejected.Should().BeTrue();
        housingSearch.Notes.Should().Contain("Rejected: Does not meet criteria");
        housingSearch.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<HousingSearchStageChanged>()
            .Which.NewStage.Should().Be(HousingSearchStage.Rejected);
    }

    [Fact]
    public void Reject_FromNonSubmitted_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();

        // Act
        var act = () => housingSearch.Reject("Reason", _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_IsTerminalState_CannotTransitionOut()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();
        housingSearch.Reject("Does not meet criteria", _userId);

        // Act
        var act = () => housingSearch.StartHouseHunting(_userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PutUnderContract_ShouldSetContractDetails()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();
        housingSearch.ClearDomainEvents();

        var propertyId = Guid.NewGuid();
        var contractPrice = new Money(450000);
        var expectedClosingDate = DateTime.UtcNow.AddDays(60);

        // Act
        housingSearch.PutUnderContract(propertyId, contractPrice, expectedClosingDate, _userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.UnderContract);
        housingSearch.CurrentContract.Should().NotBeNull();
        housingSearch.CurrentContract!.PropertyId.Should().Be(propertyId);
        housingSearch.CurrentContract.Price.Should().Be(contractPrice);
        housingSearch.CurrentContract.ContractDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        housingSearch.CurrentContract.ExpectedClosingDate.Should().Be(expectedClosingDate);
        housingSearch.IsUnderContract.Should().BeTrue();
    }

    [Fact]
    public void PutUnderContract_FromNonHouseHunting_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();

        // Act
        var act = () => housingSearch.PutUnderContract(
            Guid.NewGuid(),
            new Money(450000),
            DateTime.UtcNow.AddDays(60),
            _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ContractFellThrough_ShouldReturnToHouseHuntingAndPreserveHistory()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();
        var propertyId = Guid.NewGuid();
        var contractPrice = new Money(450000);
        housingSearch.PutUnderContract(propertyId, contractPrice, null, _userId);
        housingSearch.ClearDomainEvents();

        // Act
        housingSearch.ContractFellThrough("Inspection issues", _userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.HouseHunting);
        housingSearch.CurrentContract.Should().BeNull();
        housingSearch.IsUnderContract.Should().BeFalse();

        // Verify history was preserved
        housingSearch.FailedContracts.Should().HaveCount(1);
        housingSearch.FailedContractCount.Should().Be(1);
        var failedContract = housingSearch.FailedContracts.First();
        failedContract.PropertyId.Should().Be(propertyId);
        failedContract.ContractPrice.Should().Be(contractPrice);
        failedContract.Reason.Should().Be("Inspection issues");
    }

    [Fact]
    public void ContractFellThrough_MultipleTimes_ShouldPreserveAllHistory()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();

        var propertyId1 = Guid.NewGuid();
        var propertyId2 = Guid.NewGuid();

        // First contract
        housingSearch.PutUnderContract(propertyId1, new Money(450000), null, _userId);
        housingSearch.ContractFellThrough("Financing fell through", _userId);

        // Second contract
        housingSearch.PutUnderContract(propertyId2, new Money(475000), null, _userId);
        housingSearch.ContractFellThrough("Seller backed out", _userId);

        // Assert
        housingSearch.FailedContracts.Should().HaveCount(2);
        housingSearch.FailedContracts[0].PropertyId.Should().Be(propertyId1);
        housingSearch.FailedContracts[0].Reason.Should().Be("Financing fell through");
        housingSearch.FailedContracts[1].PropertyId.Should().Be(propertyId2);
        housingSearch.FailedContracts[1].Reason.Should().Be("Seller backed out");
    }

    [Fact]
    public void ContractFellThrough_FromNonUnderContract_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();

        // Act
        var act = () => housingSearch.ContractFellThrough("Reason", _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Pause_FromHouseHunting_ShouldMoveToPaused()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();
        housingSearch.ClearDomainEvents();

        // Act
        housingSearch.Pause("Family circumstances", _userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.Paused);
        housingSearch.Notes.Should().Contain("Paused: Family circumstances");
    }

    [Fact]
    public void Pause_FromNonHouseHunting_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();

        // Act
        var act = () => housingSearch.Pause("Reason", _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Resume_FromPaused_ShouldMoveToHouseHunting()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();
        housingSearch.Pause("Taking a break", _userId);
        housingSearch.ClearDomainEvents();

        // Act
        housingSearch.Resume(_userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.HouseHunting);
    }

    [Fact]
    public void Resume_FromNonPaused_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();

        // Act
        var act = () => housingSearch.Resume(_userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordClosing_ShouldMoveToClosedStage()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();
        housingSearch.PutUnderContract(Guid.NewGuid(), new Money(450000), null, _userId);
        housingSearch.ClearDomainEvents();

        var closingDate = DateTime.UtcNow;

        // Act
        housingSearch.RecordClosing(closingDate, _userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.Closed);
        housingSearch.CurrentContract.Should().NotBeNull();
        housingSearch.CurrentContract!.ActualClosingDate.Should().Be(closingDate);
        housingSearch.CurrentContract.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void RecordClosing_WithoutContract_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();
        // Manually transition to UnderContract without a contract (simulated edge case)
        // This should not happen in normal flow, but let's test the guard

        // Act - try to record closing from HouseHunting (no contract)
        var act = () => housingSearch.RecordClosing(DateTime.UtcNow, _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordMovedIn_ShouldMoveToMovedInStage()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();
        housingSearch.PutUnderContract(Guid.NewGuid(), new Money(450000), null, _userId);
        housingSearch.RecordClosing(DateTime.UtcNow, _userId);
        housingSearch.ClearDomainEvents();

        var movedInDate = DateTime.UtcNow;

        // Act
        housingSearch.RecordMovedIn(movedInDate, _userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.MovedIn);
        housingSearch.MovedInDate.Should().Be(movedInDate);
        housingSearch.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void SetMovedInStatus_ShouldUpdateStatus()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();
        housingSearch.PutUnderContract(Guid.NewGuid(), new Money(450000), null, _userId);
        housingSearch.RecordClosing(DateTime.UtcNow, _userId);
        housingSearch.RecordMovedIn(DateTime.UtcNow, _userId);

        // Act
        housingSearch.SetMovedInStatus(MovedInStatus.MovedIn, DateTime.UtcNow, _userId);

        // Assert
        housingSearch.MovedInStatus.Should().Be(MovedInStatus.MovedIn);
        housingSearch.MovedInDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();

        // Act
        housingSearch.Deactivate(_userId);

        // Assert
        housingSearch.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Reactivate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();
        housingSearch.Deactivate(_userId);

        // Act
        housingSearch.Reactivate(_userId);

        // Assert
        housingSearch.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateNotes_ShouldUpdateNotes()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();

        // Act
        housingSearch.UpdateNotes("Important notes here", _userId);

        // Assert
        housingSearch.Notes.Should().Be("Important notes here");
    }

    [Fact]
    public void FullWorkflow_ShouldProgressThroughAllStages()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();
        var propertyId = Guid.NewGuid();

        // Act - Progress through entire workflow
        housingSearch.Stage.Should().Be(HousingSearchStage.Submitted);

        // Board approval (with documents already uploaded)
        housingSearch.ApproveBoardReview(_userId, hasRequiredDocuments: true);
        housingSearch.Stage.Should().Be(HousingSearchStage.HouseHunting);

        housingSearch.PutUnderContract(propertyId, new Money(500000), DateTime.UtcNow.AddDays(60), _userId);
        housingSearch.Stage.Should().Be(HousingSearchStage.UnderContract);

        housingSearch.RecordClosing(DateTime.UtcNow, _userId);
        housingSearch.Stage.Should().Be(HousingSearchStage.Closed);

        housingSearch.RecordMovedIn(DateTime.UtcNow, _userId);
        housingSearch.Stage.Should().Be(HousingSearchStage.MovedIn);

        housingSearch.SetMovedInStatus(MovedInStatus.MovedIn, DateTime.UtcNow, _userId);

        // Assert final state
        housingSearch.IsComplete.Should().BeTrue();
        housingSearch.MovedInStatus.Should().Be(MovedInStatus.MovedIn);
        housingSearch.CurrentContract.Should().NotBeNull();
        housingSearch.CurrentContract!.PropertyId.Should().Be(propertyId);
    }

    [Fact]
    public void FullWorkflow_WithManualHouseHuntingStart_ShouldProgressThroughAllStages()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();
        var propertyId = Guid.NewGuid();

        // Act - Progress through entire workflow (documents uploaded after board approval)
        housingSearch.Stage.Should().Be(HousingSearchStage.Submitted);

        // Board approval without documents
        housingSearch.ApproveBoardReview(_userId, hasRequiredDocuments: false);
        housingSearch.Stage.Should().Be(HousingSearchStage.BoardApproved);

        // Manually start house hunting (caller verifies documents are uploaded)
        housingSearch.StartHouseHunting(_userId);
        housingSearch.Stage.Should().Be(HousingSearchStage.HouseHunting);

        housingSearch.PutUnderContract(propertyId, new Money(500000), DateTime.UtcNow.AddDays(60), _userId);
        housingSearch.Stage.Should().Be(HousingSearchStage.UnderContract);

        housingSearch.RecordClosing(DateTime.UtcNow, _userId);
        housingSearch.Stage.Should().Be(HousingSearchStage.Closed);

        housingSearch.RecordMovedIn(DateTime.UtcNow, _userId);
        housingSearch.Stage.Should().Be(HousingSearchStage.MovedIn);

        // Assert final state
        housingSearch.IsComplete.Should().BeTrue();
        housingSearch.CurrentContract.Should().NotBeNull();
        housingSearch.CurrentContract!.PropertyId.Should().Be(propertyId);
    }

    [Fact]
    public void InvalidTransition_SubmittedToClosed_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();

        // Act - try to skip stages
        var act = () => housingSearch.RecordClosing(DateTime.UtcNow, _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void InvalidTransition_HouseHuntingToMovedIn_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();

        // Act - try to skip stages
        var act = () => housingSearch.RecordMovedIn(DateTime.UtcNow, _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdatePreferences_ShouldUpdatePreferencesAndRaiseEvent()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();
        housingSearch.ClearDomainEvents();
        var budget = new Money(500000);

        var preferences = new HousingPreferences(
            budget: budget,
            minBedrooms: 4,
            minBathrooms: 2.5m,
            requiredFeatures: new List<string> { "Basement", "Garage" },
            shulProximity: ShulProximityPreference.WithMaxDistance(0.5),
            moveTimeline: MoveTimeline.ShortTerm);

        // Act
        housingSearch.UpdatePreferences(preferences, _userId);

        // Assert
        housingSearch.Preferences.Should().NotBeNull();
        housingSearch.Preferences!.Budget.Should().Be(budget);
        housingSearch.Preferences.MinBedrooms.Should().Be(4);
        housingSearch.Preferences.MinBathrooms.Should().Be(2.5m);
        housingSearch.Preferences.RequiredFeatures.Should().Contain("Basement");
        housingSearch.Preferences.RequiredFeatures.Should().Contain("Garage");
        housingSearch.Preferences.MoveTimeline.Should().Be(MoveTimeline.ShortTerm);
        housingSearch.Preferences.ShulProximity!.MaxWalkingDistanceMiles.Should().Be(0.5);

        housingSearch.DomainEvents.Should().Contain(e => e is HousingPreferencesUpdated);
    }

    [Fact]
    public void Create_ShouldInitializeWithDefaultPreferences()
    {
        // Arrange & Act
        var housingSearch = CreateTestHousingSearch();

        // Assert
        housingSearch.Preferences.Should().NotBeNull();
        housingSearch.Preferences!.Budget.Should().BeNull();
        housingSearch.Preferences.MinBedrooms.Should().BeNull();
        housingSearch.Preferences.MinBathrooms.Should().BeNull();
        housingSearch.Preferences.RequiredFeatures.Should().BeEmpty();
        housingSearch.Preferences.MoveTimeline.Should().BeNull();
    }

    [Fact]
    public void Contract_WithActualClosingDate_ShouldBeMarkedAsClosed()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var price = new Money(450000);
        var contractDate = DateTime.UtcNow.AddDays(-30);
        var contract = new Contract(propertyId, price, contractDate);

        // Act
        var closedContract = contract.WithActualClosingDate(DateTime.UtcNow);

        // Assert
        closedContract.IsClosed.Should().BeTrue();
        closedContract.ActualClosingDate.Should().NotBeNull();
        contract.IsClosed.Should().BeFalse(); // Original should be unchanged (immutable)
    }

    private HousingSearch CreateTestHousingSearch()
    {
        return HousingSearch.Create(
            applicantId: _applicantId,
            createdBy: _userId);
    }

    private HousingSearch CreateHouseHuntingSearch()
    {
        var housingSearch = CreateTestHousingSearch();
        // Board approval with required documents (auto-transitions to HouseHunting)
        housingSearch.ApproveBoardReview(_userId, hasRequiredDocuments: true);
        return housingSearch;
    }

    private HousingSearch CreateBoardApprovedSearch()
    {
        var housingSearch = CreateTestHousingSearch();
        housingSearch.ApproveBoardReview(_userId, hasRequiredDocuments: false);
        return housingSearch;
    }
}
