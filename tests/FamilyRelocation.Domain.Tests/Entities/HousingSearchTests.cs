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
            searchNumber: "HS-2026-0001",
            createdBy: _userId);

        // Assert
        housingSearch.ApplicantId.Should().Be(_applicantId);
        housingSearch.SearchNumber.Should().Be("HS-2026-0001");
        housingSearch.Stage.Should().Be(HousingSearchStage.Submitted);
        housingSearch.IsActive.Should().BeTrue();
        housingSearch.CreatedBy.Should().Be(_userId);
        housingSearch.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ShouldRaiseHousingSearchStartedEvent()
    {
        // Arrange & Act
        var housingSearch = HousingSearch.Create(
            applicantId: _applicantId,
            searchNumber: "HS-2026-0001",
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
            searchNumber: "HS-2026-0001",
            createdBy: _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Applicant ID*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSearchNumber_ShouldThrow(string? searchNumber)
    {
        // Arrange & Act
        var act = () => HousingSearch.Create(
            applicantId: _applicantId,
            searchNumber: searchNumber!,
            createdBy: _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Search number*");
    }

    [Fact]
    public void StartHouseHunting_FromSubmitted_ShouldMoveToHouseHunting()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();
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
    public void StartHouseHunting_FromUnderContract_ShouldThrow()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();
        housingSearch.PutUnderContract(Guid.NewGuid(), new Money(450000), null, _userId);

        // Act
        var act = () => housingSearch.StartHouseHunting(_userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot start house hunting*");
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
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*only reject from Submitted*");
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
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot start house hunting*");
    }

    [Fact]
    public void PutUnderContract_ShouldSetContractDetails()
    {
        // Arrange
        var housingSearch = CreateHouseHuntingSearch();
        housingSearch.ClearDomainEvents();

        var propertyId = Guid.NewGuid();
        var contractPrice = new Money(450000);
        var closingDate = DateTime.UtcNow.AddDays(60);

        // Act
        housingSearch.PutUnderContract(propertyId, contractPrice, closingDate, _userId);

        // Assert
        housingSearch.Stage.Should().Be(HousingSearchStage.UnderContract);
        housingSearch.ContractPropertyId.Should().Be(propertyId);
        housingSearch.ContractPrice.Should().Be(contractPrice);
        housingSearch.ContractDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        housingSearch.ClosingDate.Should().Be(closingDate);
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
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot go under contract*");
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
        housingSearch.ContractPropertyId.Should().BeNull();
        housingSearch.ContractPrice.Should().BeNull();
        housingSearch.ContractDate.Should().BeNull();
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
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*only mark contract as fallen through when UnderContract or Closed*");
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
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*only pause when actively house hunting*");
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
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*only resume from Paused*");
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
        housingSearch.ActualClosingDate.Should().Be(closingDate);
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

        housingSearch.StartHouseHunting(_userId);
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
        housingSearch.ContractPropertyId.Should().Be(propertyId);
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
    public void UpdateHousingPreferences_ShouldUpdatePreferencesAndRaiseEvent()
    {
        // Arrange
        var housingSearch = CreateTestHousingSearch();
        housingSearch.ClearDomainEvents();
        var budget = new Money(500000);

        // Act
        housingSearch.UpdateHousingPreferences(
            budget: budget,
            minBedrooms: 4,
            minBathrooms: 2.5m,
            features: new List<string> { "Basement", "Garage" },
            shulProximity: ShulProximityPreference.WithMaxDistance(0.5),
            moveTimeline: MoveTimeline.ShortTerm,
            modifiedBy: _userId);

        // Assert
        housingSearch.Budget.Should().Be(budget);
        housingSearch.MinBedrooms.Should().Be(4);
        housingSearch.MinBathrooms.Should().Be(2.5m);
        housingSearch.RequiredFeatures.Should().Contain("Basement");
        housingSearch.RequiredFeatures.Should().Contain("Garage");
        housingSearch.MoveTimeline.Should().Be(MoveTimeline.ShortTerm);
        housingSearch.ShulProximity!.MaxWalkingDistanceMiles.Should().Be(0.5);

        housingSearch.DomainEvents.Should().Contain(e => e is HousingPreferencesUpdated);
    }

    [Fact]
    public void Create_ShouldInitializeWithDefaultPreferences()
    {
        // Arrange & Act
        var housingSearch = CreateTestHousingSearch();

        // Assert
        housingSearch.Budget.Should().BeNull();
        housingSearch.MinBedrooms.Should().BeNull();
        housingSearch.MinBathrooms.Should().BeNull();
        housingSearch.RequiredFeatures.Should().BeEmpty();
        housingSearch.ShulProximity.Should().NotBeNull();
        housingSearch.ShulProximity!.AnyShulAcceptable.Should().BeTrue();
        housingSearch.MoveTimeline.Should().BeNull();
    }

    private HousingSearch CreateTestHousingSearch()
    {
        return HousingSearch.Create(
            applicantId: _applicantId,
            searchNumber: "HS-2026-0001",
            createdBy: _userId);
    }

    private HousingSearch CreateHouseHuntingSearch()
    {
        var housingSearch = CreateTestHousingSearch();
        housingSearch.StartHouseHunting(_userId);
        return housingSearch;
    }
}
