using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.Entities;

/// <summary>
/// Tests for HousingSearch entity.
/// Note: HousingSearch is now created ONLY when an applicant is approved by the board.
/// It starts in Searching stage (not Submitted - that's an application-level concern).
/// </summary>
public class HousingSearchTests
{
    private readonly Guid _applicantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldCreateHousingSearchInSearchingStage()
    {
        // Arrange & Act
        var housingSearch = HousingSearch.Create(
            applicantId: _applicantId,
            createdBy: _userId);

        // Assert
        housingSearch.ApplicantId.Should().Be(_applicantId);
        housingSearch.Stage.Should().Be(HousingSearchStage.Searching);
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
    public void Create_ShouldInitializeWithDefaultPreferences()
    {
        // Arrange & Act
        var housingSearch = CreateSearchingSearch();

        // Assert
        housingSearch.Preferences.Should().NotBeNull();
        housingSearch.Preferences!.Budget.Should().BeNull();
        housingSearch.Preferences.MinBedrooms.Should().BeNull();
        housingSearch.Preferences.MinBathrooms.Should().BeNull();
        housingSearch.Preferences.RequiredFeatures.Should().BeEmpty();
        housingSearch.Preferences.MoveTimeline.Should().BeNull();
    }

    #endregion

    #region PutUnderContract Tests

    [Fact]
    public void PutUnderContract_FromSearching_ShouldSetContractDetails()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
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
    public void PutUnderContract_FromPaused_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        housingSearch.Pause("Taking a break", _userId);

        // Act
        var act = () => housingSearch.PutUnderContract(
            Guid.NewGuid(),
            new Money(450000),
            DateTime.UtcNow.AddDays(60),
            _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Searching*");
    }

    [Fact]
    public void PutUnderContract_FromClosed_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        housingSearch.PutUnderContract(Guid.NewGuid(), new Money(450000), null, _userId);
        housingSearch.RecordClosing(DateTime.UtcNow, _userId);

        // Act
        var act = () => housingSearch.PutUnderContract(
            Guid.NewGuid(),
            new Money(450000),
            DateTime.UtcNow.AddDays(60),
            _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region ContractFellThrough Tests

    [Fact]
    public void ContractFellThrough_FromUnderContract_ShouldReturnToSearchingAndPreserveHistory()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        var propertyId = Guid.NewGuid();
        var contractPrice = new Money(450000);
        housingSearch.PutUnderContract(propertyId, contractPrice, null, _userId);
        housingSearch.ClearDomainEvents();

        // Act
        housingSearch.ContractFellThrough("Inspection issues", _userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.Searching);
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
    public void ContractFellThrough_FromClosed_ShouldReturnToSearching()
    {
        // Arrange - deal fell through after closing was recorded but before move-in
        var housingSearch = CreateSearchingSearch();
        var propertyId = Guid.NewGuid();
        housingSearch.PutUnderContract(propertyId, new Money(450000), null, _userId);
        housingSearch.RecordClosing(DateTime.UtcNow, _userId);
        housingSearch.ClearDomainEvents();

        // Act
        housingSearch.ContractFellThrough("Post-closing issue", _userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.Searching);
        housingSearch.CurrentContract.Should().BeNull();
        housingSearch.FailedContracts.Should().HaveCount(1);
    }

    [Fact]
    public void ContractFellThrough_MultipleTimes_ShouldPreserveAllHistory()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();

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
    public void ContractFellThrough_FromSearching_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();

        // Act
        var act = () => housingSearch.ContractFellThrough("Reason", _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*UnderContract*Closed*");
    }

    #endregion

    #region Pause/Resume Tests

    [Fact]
    public void Pause_FromSearching_ShouldMoveToPaused()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        housingSearch.ClearDomainEvents();

        // Act
        housingSearch.Pause("Family circumstances", _userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.Paused);
        housingSearch.Notes.Should().Contain("Paused: Family circumstances");
    }

    [Fact]
    public void Pause_FromUnderContract_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        housingSearch.PutUnderContract(Guid.NewGuid(), new Money(450000), null, _userId);

        // Act
        var act = () => housingSearch.Pause("Reason", _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Searching*");
    }

    [Fact]
    public void Resume_FromPaused_ShouldMoveToSearching()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        housingSearch.Pause("Taking a break", _userId);
        housingSearch.ClearDomainEvents();

        // Act
        housingSearch.Resume(_userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.Searching);
    }

    [Fact]
    public void Resume_FromNonPaused_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();

        // Act
        var act = () => housingSearch.Resume(_userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Paused*");
    }

    #endregion

    #region RecordClosing Tests

    [Fact]
    public void RecordClosing_FromUnderContract_ShouldMoveToClosedStage()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
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
    public void RecordClosing_FromSearching_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();

        // Act - try to record closing without being under contract
        var act = () => housingSearch.RecordClosing(DateTime.UtcNow, _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*contract*");
    }

    #endregion

    #region RecordMovedIn Tests

    [Fact]
    public void RecordMovedIn_FromClosed_ShouldMoveToMovedInStage()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
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
    public void RecordMovedIn_FromSearching_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();

        // Act - try to skip stages
        var act = () => housingSearch.RecordMovedIn(DateTime.UtcNow, _userId);

        // Assert - domain throws because it's not Closed stage
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordMovedIn_FromUnderContract_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        housingSearch.PutUnderContract(Guid.NewGuid(), new Money(450000), null, _userId);

        // Act - try to skip closing
        var act = () => housingSearch.RecordMovedIn(DateTime.UtcNow, _userId);

        // Assert - domain throws because it's not Closed stage
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetMovedInStatus_ShouldUpdateStatus()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        housingSearch.PutUnderContract(Guid.NewGuid(), new Money(450000), null, _userId);
        housingSearch.RecordClosing(DateTime.UtcNow, _userId);
        housingSearch.RecordMovedIn(DateTime.UtcNow, _userId);

        // Act
        housingSearch.SetMovedInStatus(MovedInStatus.MovedIn, DateTime.UtcNow, _userId);

        // Assert
        housingSearch.MovedInStatus.Should().Be(MovedInStatus.MovedIn);
        housingSearch.MovedInDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Deactivate/Reactivate Tests

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();

        // Act
        housingSearch.Deactivate(_userId);

        // Assert
        housingSearch.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Reactivate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        housingSearch.Deactivate(_userId);

        // Act
        housingSearch.Reactivate(_userId);

        // Assert
        housingSearch.IsActive.Should().BeTrue();
    }

    #endregion

    #region Notes Tests

    [Fact]
    public void UpdateNotes_ShouldUpdateNotes()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();

        // Act
        housingSearch.UpdateNotes("Important notes here", _userId);

        // Assert
        housingSearch.Notes.Should().Be("Important notes here");
    }

    #endregion

    #region Preferences Tests

    [Fact]
    public void UpdatePreferences_ShouldUpdatePreferencesAndRaiseEvent()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
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

    #endregion

    #region Full Workflow Tests

    [Fact]
    public void FullWorkflow_ShouldProgressThroughAllStages()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        var propertyId = Guid.NewGuid();

        // Act & Assert - Progress through entire workflow
        housingSearch.Stage.Should().Be(HousingSearchStage.Searching);

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
    public void FullWorkflow_WithContractFallthrough_ShouldRecoverAndComplete()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        var propertyId1 = Guid.NewGuid();
        var propertyId2 = Guid.NewGuid();

        // First contract falls through
        housingSearch.PutUnderContract(propertyId1, new Money(450000), null, _userId);
        housingSearch.Stage.Should().Be(HousingSearchStage.UnderContract);

        housingSearch.ContractFellThrough("Inspection issues", _userId);
        housingSearch.Stage.Should().Be(HousingSearchStage.Searching);

        // Second contract succeeds
        housingSearch.PutUnderContract(propertyId2, new Money(475000), null, _userId);
        housingSearch.RecordClosing(DateTime.UtcNow, _userId);
        housingSearch.RecordMovedIn(DateTime.UtcNow, _userId);

        // Assert
        housingSearch.IsComplete.Should().BeTrue();
        housingSearch.FailedContractCount.Should().Be(1);
        housingSearch.CurrentContract!.PropertyId.Should().Be(propertyId2);
    }

    [Fact]
    public void FullWorkflow_WithPause_ShouldResumeAndComplete()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        var propertyId = Guid.NewGuid();

        // Pause and resume
        housingSearch.Pause("Family matter", _userId);
        housingSearch.Stage.Should().Be(HousingSearchStage.Paused);

        housingSearch.Resume(_userId);
        housingSearch.Stage.Should().Be(HousingSearchStage.Searching);

        // Continue to completion
        housingSearch.PutUnderContract(propertyId, new Money(500000), null, _userId);
        housingSearch.RecordClosing(DateTime.UtcNow, _userId);
        housingSearch.RecordMovedIn(DateTime.UtcNow, _userId);

        // Assert
        housingSearch.IsComplete.Should().BeTrue();
    }

    #endregion

    #region Contract Value Object Tests

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

    #endregion

    #region Invalid State Transitions

    [Fact]
    public void MovedIn_IsTerminalState_CannotTransitionOut()
    {
        // Arrange
        var housingSearch = CreateSearchingSearch();
        housingSearch.PutUnderContract(Guid.NewGuid(), new Money(450000), null, _userId);
        housingSearch.RecordClosing(DateTime.UtcNow, _userId);
        housingSearch.RecordMovedIn(DateTime.UtcNow, _userId);

        // Act
        var act = () => housingSearch.Pause("Want to pause", _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Helper Methods

    private HousingSearch CreateSearchingSearch()
    {
        return HousingSearch.Create(
            applicantId: _applicantId,
            createdBy: _userId);
    }

    #endregion
}
