using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.Entities;

public class ApplicantTests
{
    private readonly Address _testAddress = new("123 Main St", "Union", "NJ", "07083");
    private readonly Guid _userId = Guid.NewGuid();

    private HusbandInfo CreateTestHusband(string firstName = "Moshe", string lastName = "Cohen") =>
        new(firstName, lastName, "Yaakov", new Email("moshe@example.com"));

    private SpouseInfo CreateTestWife() =>
        new("Sarah", "Goldstein", "Yitzchak", new Email("sarah@example.com"),
            highSchool: "Bais Yaakov");

    [Fact]
    public void Create_WithValidData_ShouldCreateApplicant()
    {
        // Arrange
        var husband = CreateTestHusband();
        var wife = CreateTestWife();

        // Act
        var applicant = Applicant.Create(
            husband: husband,
            wife: wife,
            address: _testAddress,
            children: null,
            currentKehila: "Brooklyn",
            shabbosShul: "Bobov",
            createdBy: _userId);

        // Assert
        applicant.Husband.Should().Be(husband);
        applicant.Husband.FirstName.Should().Be("Moshe");
        applicant.Husband.LastName.Should().Be("Cohen");
        applicant.Husband.FullName.Should().Be("Moshe Cohen");
        applicant.Wife.Should().Be(wife);
        applicant.Address.Should().Be(_testAddress);
        applicant.CurrentKehila.Should().Be("Brooklyn");
        applicant.ShabbosShul.Should().Be("Bobov");
        applicant.CreatedBy.Should().Be(_userId);
        applicant.IsDeleted.Should().BeFalse();
        applicant.Id.Should().NotBeEmpty();
        applicant.FamilyName.Should().Be("Moshe Cohen");
    }

    [Fact]
    public void Create_ShouldRaiseApplicantCreatedEvent()
    {
        // Arrange & Act
        var applicant = Applicant.Create(
            husband: CreateTestHusband(),
            wife: null,
            address: null,
            children: null,
            currentKehila: null,
            shabbosShul: null,
            createdBy: _userId);

        // Assert
        applicant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ApplicantCreated>()
            .Which.ApplicantId.Should().Be(applicant.Id);
    }

    [Fact]
    public void Create_WithNullHusband_ShouldThrow()
    {
        // Arrange & Act
        var act = () => Applicant.Create(
            husband: null!,
            wife: null,
            address: null,
            children: null,
            currentKehila: null,
            shabbosShul: null,
            createdBy: _userId);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithChildren_ShouldSetChildren()
    {
        // Arrange
        var children = new List<Child>
        {
            new(8, Gender.Male, "Yosef", "Torah Academy"),
            new(5, Gender.Female, "Rivka", "Bais Yaakov")
        };

        // Act
        var applicant = Applicant.Create(
            husband: CreateTestHusband(),
            wife: CreateTestWife(),
            address: _testAddress,
            children: children,
            currentKehila: "Brooklyn",
            shabbosShul: "Bobov",
            createdBy: _userId);

        // Assert
        applicant.Children.Should().HaveCount(2);
        applicant.NumberOfChildren.Should().Be(2);
        applicant.Children[0].Name.Should().Be("Yosef");
    }

    [Fact]
    public void UpdateHusband_ShouldUpdateHusbandInfo()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var modifiedBy = Guid.NewGuid();
        var newHusband = new HusbandInfo("David", "Levy", "Avraham", new Email("david@example.com"));

        // Act
        applicant.UpdateHusband(newHusband, modifiedBy);

        // Assert
        applicant.Husband.FirstName.Should().Be("David");
        applicant.Husband.LastName.Should().Be("Levy");
        applicant.Husband.FatherName.Should().Be("Avraham");
        applicant.FamilyName.Should().Be("David Levy");
        applicant.ModifiedBy.Should().Be(modifiedBy);
    }

    [Fact]
    public void UpdateWife_ShouldUpdateWifeInfo()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var modifiedBy = Guid.NewGuid();
        var newWife = new SpouseInfo("Rachel", "Schwartz", "Shlomo",
            new Email("rachel@example.com"), highSchool: "BJJ");

        // Act
        applicant.UpdateWife(newWife, modifiedBy);

        // Assert
        applicant.Wife.Should().NotBeNull();
        applicant.Wife!.FirstName.Should().Be("Rachel");
        applicant.Wife.MaidenName.Should().Be("Schwartz");
        applicant.Wife.FatherName.Should().Be("Shlomo");
        applicant.Wife.HighSchool.Should().Be("BJJ");
        applicant.Wife.FullName.Should().Be("Rachel (Schwartz)");
        applicant.ModifiedBy.Should().Be(modifiedBy);
    }

    [Fact]
    public void UpdateCommunityInfo_ShouldUpdateFields()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var modifiedBy = Guid.NewGuid();

        // Act
        applicant.UpdateCommunityInfo(
            currentKehila: "Monsey",
            shabbosShul: "Nassad",
            modifiedBy: modifiedBy);

        // Assert
        applicant.CurrentKehila.Should().Be("Monsey");
        applicant.ShabbosShul.Should().Be("Nassad");
        applicant.ModifiedBy.Should().Be(modifiedBy);
    }

    [Fact]
    public void UpdateAddress_ShouldUpdateAddress()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var modifiedBy = Guid.NewGuid();
        var newAddress = new Address("456 Oak Ave", "Roselle Park", "NJ", "07204");

        // Act
        applicant.UpdateAddress(newAddress, modifiedBy);

        // Assert
        applicant.Address.Should().Be(newAddress);
        applicant.ModifiedBy.Should().Be(modifiedBy);
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
        applicant.BoardReview.Should().NotBeNull();
        applicant.BoardReview!.Decision.Should().Be(BoardDecision.Approved);
        applicant.BoardReview.Notes.Should().Be("Good candidate");
        applicant.BoardReview.ReviewedByUserId.Should().Be(reviewerId);
        applicant.BoardReview.ReviewDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        applicant.IsApproved.Should().BeTrue();
        applicant.IsPendingBoardReview.Should().BeFalse();

        var approvedEvent = applicant.DomainEvents
            .OfType<ApplicantBoardDecisionMade>()
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
        applicant.BoardReview.Should().NotBeNull();
        applicant.BoardReview!.Decision.Should().Be(BoardDecision.Rejected);
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
        applicant.BoardReview.Should().BeNull();
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

    [Fact]
    public void HusbandInfo_FullNameWithFather_ShouldFormatCorrectly()
    {
        // Arrange
        var husband = new HusbandInfo("Moshe", "Cohen", "Yaakov");

        // Assert
        husband.FullNameWithFather.Should().Be("Moshe Cohen (ben Yaakov)");
    }

    [Fact]
    public void SpouseInfo_FullName_ShouldFormatCorrectly()
    {
        // Arrange
        var wife = new SpouseInfo("Sarah", "Goldstein", "Yitzchak");

        // Assert
        wife.FullName.Should().Be("Sarah (Goldstein)");
    }

    private Applicant CreateTestApplicant()
    {
        return Applicant.Create(
            husband: CreateTestHusband(),
            wife: CreateTestWife(),
            address: _testAddress,
            children: null,
            currentKehila: "Brooklyn",
            shabbosShul: "Bobov",
            createdBy: _userId);
    }
}
