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
    public void UpdateHusbandInfo_ShouldUpdateFields()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var modifiedBy = Guid.NewGuid();

        // Act
        applicant.UpdateHusbandInfo(
            firstName: "David",
            lastName: "Levy",
            fatherName: "Avraham",
            modifiedBy: modifiedBy);

        // Assert
        applicant.FirstName.Should().Be("David");
        applicant.LastName.Should().Be("Levy");
        applicant.FatherName.Should().Be("Avraham");
        applicant.ModifiedBy.Should().Be(modifiedBy);
    }

    [Fact]
    public void UpdateWifeInfo_ShouldUpdateWife()
    {
        // Arrange
        var applicant = CreateTestApplicant();
        var modifiedBy = Guid.NewGuid();
        var wifeInfo = new SpouseInfo("Sarah", "Goldstein", "Yitzchak", "Bais Yaakov");

        // Act
        applicant.UpdateWifeInfo(wifeInfo, modifiedBy);

        // Assert
        applicant.Wife.Should().NotBeNull();
        applicant.Wife!.FirstName.Should().Be("Sarah");
        applicant.Wife.MaidenName.Should().Be("Goldstein");
        applicant.Wife.FatherName.Should().Be("Yitzchak");
        applicant.Wife.HighSchool.Should().Be("Bais Yaakov");
        applicant.Wife.FullName.Should().Be("Sarah Goldstein");
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
