using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.Entities;

public class ApplicantTests
{
    private readonly Email _testEmail = new("test@example.com");
    private readonly Address _testAddress = new("123 Main St", "Union", "NJ", "07083");
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void CreateFromApplication_WithValidData_ShouldCreateApplicant()
    {
        // Arrange & Act
        var applicant = Applicant.CreateFromApplication(
            firstName: "Moshe",
            lastName: "Cohen",
            fatherName: "Yaakov",
            email: _testEmail,
            address: _testAddress,
            currentKehila: "Brooklyn",
            shabbosShul: "Bobov",
            createdBy: _userId);

        // Assert
        applicant.FirstName.Should().Be("Moshe");
        applicant.LastName.Should().Be("Cohen");
        applicant.FatherName.Should().Be("Yaakov");
        applicant.FullName.Should().Be("Moshe Cohen");
        applicant.Email.Should().Be(_testEmail);
        applicant.Address.Should().Be(_testAddress);
        applicant.CurrentKehila.Should().Be("Brooklyn");
        applicant.ShabbosShul.Should().Be("Bobov");
        applicant.CreatedBy.Should().Be(_userId);
        applicant.IsDeleted.Should().BeFalse();
        applicant.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateFromApplication_ShouldRaiseApplicantCreatedEvent()
    {
        // Arrange & Act
        var applicant = Applicant.CreateFromApplication(
            firstName: "Moshe",
            lastName: "Cohen",
            fatherName: null,
            email: _testEmail,
            address: null,
            currentKehila: null,
            shabbosShul: null,
            createdBy: _userId);

        // Assert
        applicant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ApplicantCreated>()
            .Which.ApplicantId.Should().Be(applicant.Id);
    }

    [Fact]
    public void CreateFromApplication_WithProspectId_ShouldSetProspectId()
    {
        // Arrange
        var prospectId = Guid.NewGuid();

        // Act
        var applicant = Applicant.CreateFromApplication(
            firstName: "Moshe",
            lastName: "Cohen",
            fatherName: null,
            email: _testEmail,
            address: null,
            currentKehila: null,
            shabbosShul: null,
            createdBy: _userId,
            prospectId: prospectId);

        // Assert
        applicant.ProspectId.Should().Be(prospectId);
        var domainEvent = applicant.DomainEvents.OfType<ApplicantCreated>().Single();
        domainEvent.ProspectId.Should().Be(prospectId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFromApplication_WithNullOrEmptyFirstName_ShouldThrow(string? firstName)
    {
        // Arrange & Act
        var act = () => Applicant.CreateFromApplication(
            firstName: firstName!,
            lastName: "Cohen",
            fatherName: null,
            email: _testEmail,
            address: null,
            currentKehila: null,
            shabbosShul: null,
            createdBy: _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*First name*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFromApplication_WithNullOrEmptyLastName_ShouldThrow(string? lastName)
    {
        // Arrange & Act
        var act = () => Applicant.CreateFromApplication(
            firstName: "Moshe",
            lastName: lastName!,
            fatherName: null,
            email: _testEmail,
            address: null,
            currentKehila: null,
            shabbosShul: null,
            createdBy: _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Last name*");
    }

    [Fact]
    public void CreateFromApplication_WithNullEmail_ShouldThrow()
    {
        // Arrange & Act
        var act = () => Applicant.CreateFromApplication(
            firstName: "Moshe",
            lastName: "Cohen",
            fatherName: null,
            email: null!,
            address: null,
            currentKehila: null,
            shabbosShul: null,
            createdBy: _userId);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateBasicInfo_ShouldUpdateFields()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var modifiedBy = Guid.NewGuid();

        // Act
        applicant.UpdateBasicInfo(
            firstName: "David",
            lastName: "Levy",
            fatherName: "Avraham",
            wifeFirstName: "Sarah",
            wifeMaidenName: "Goldstein",
            wifeFatherName: "Yitzchak",
            wifeHighSchool: "Bais Yaakov",
            currentKehila: "Monsey",
            shabbosShul: "Nassad",
            modifiedBy: modifiedBy);

        // Assert
        applicant.FirstName.Should().Be("David");
        applicant.LastName.Should().Be("Levy");
        applicant.WifeFirstName.Should().Be("Sarah");
        applicant.WifeMaidenName.Should().Be("Goldstein");
        applicant.ShabbosShul.Should().Be("Nassad");
        applicant.ModifiedBy.Should().Be(modifiedBy);
    }

    [Fact]
    public void UpdateHousingPreferences_ShouldUpdatePreferencesAndRaiseEvent()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var modifiedBy = Guid.NewGuid();
        var budget = new Money(500000);

        // Act
        applicant.UpdateHousingPreferences(
            budget: budget,
            minBedrooms: 4,
            minBathrooms: 2.5m,
            features: new List<string> { "Basement", "Garage" },
            shulProximity: ShulProximityPreference.WithMaxDistance(0.5),
            moveTimeline: MoveTimeline.ShortTerm,
            notes: "Looking for quiet neighborhood",
            modifiedBy: modifiedBy);

        // Assert
        applicant.Budget.Should().Be(budget);
        applicant.MinBedrooms.Should().Be(4);
        applicant.MinBathrooms.Should().Be(2.5m);
        applicant.RequiredFeatures.Should().Contain("Basement");
        applicant.MoveTimeline.Should().Be(MoveTimeline.ShortTerm);

        applicant.DomainEvents.Should().Contain(e => e is HousingPreferencesUpdated);
    }

    [Fact]
    public void SetBoardDecision_WithApproved_ShouldUpdateAndRaiseEvent()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var reviewerId = Guid.NewGuid();

        // Act
        applicant.SetBoardDecision(
            decision: BoardDecision.Approved,
            notes: "Good candidate",
            reviewedByUserId: reviewerId);

        // Assert
        applicant.BoardDecision.Should().Be(BoardDecision.Approved);
        applicant.BoardDecisionNotes.Should().Be("Good candidate");
        applicant.BoardReviewedByUserId.Should().Be(reviewerId);
        applicant.BoardReviewDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        applicant.IsApproved.Should().BeTrue();
        applicant.IsPendingBoardReview.Should().BeFalse();

        var approvedEvent = applicant.DomainEvents
            .OfType<ApplicantApprovedByBoard>()
            .SingleOrDefault();
        approvedEvent.Should().NotBeNull();
        approvedEvent!.Decision.Should().Be(BoardDecision.Approved);
    }

    [Fact]
    public void SetBoardDecision_WithRejected_ShouldNotBeApproved()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var reviewerId = Guid.NewGuid();

        // Act
        applicant.SetBoardDecision(
            decision: BoardDecision.Rejected,
            notes: "Does not meet criteria",
            reviewedByUserId: reviewerId);

        // Assert
        applicant.BoardDecision.Should().Be(BoardDecision.Rejected);
        applicant.IsApproved.Should().BeFalse();
        applicant.IsPendingBoardReview.Should().BeFalse();
    }

    [Fact]
    public void IsPendingBoardReview_WhenNoDecision_ShouldReturnTrue()
    {
        // Arrange
        var applicant = CreateTestApplicant();

        // Act & Assert
        applicant.IsPendingBoardReview.Should().BeTrue();
        applicant.BoardDecision.Should().BeNull();
    }

    [Fact]
    public void Delete_ShouldSoftDelete()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var deletedBy = Guid.NewGuid();

        // Act
        applicant.Delete(deletedBy);

        // Assert
        applicant.IsDeleted.Should().BeTrue();
        applicant.ModifiedBy.Should().Be(deletedBy);
    }

    [Fact]
    public void Restore_ShouldUndelete()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var userId = Guid.NewGuid();
        applicant.Delete(userId);

        // Act
        applicant.Restore(userId);

        // Assert
        applicant.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        applicant.DomainEvents.Should().NotBeEmpty();

        // Act
        applicant.ClearDomainEvents();

        // Assert
        applicant.DomainEvents.Should().BeEmpty();
    }

    private Applicant CreateTestApplicant()
    {
        return Applicant.CreateFromApplication(
            firstName: "Moshe",
            lastName: "Cohen",
            fatherName: "Yaakov",
            email: _testEmail,
            address: _testAddress,
            currentKehila: "Brooklyn",
            shabbosShul: "Bobov",
            createdBy: _userId);
    }
}
