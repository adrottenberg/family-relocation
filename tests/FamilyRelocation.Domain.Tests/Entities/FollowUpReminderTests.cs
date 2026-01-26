using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.Entities;

public class FollowUpReminderTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateReminder()
    {
        // Arrange
        var dueDateTime = DateTime.UtcNow.AddDays(1);

        // Act
        var reminder = FollowUpReminder.Create(
            title: "Follow up with applicant",
            dueDateTime: dueDateTime,
            entityType: "Applicant",
            entityId: _entityId,
            createdBy: _userId,
            notes: "Need to confirm documents");

        // Assert
        reminder.Id.Should().NotBeEmpty();
        reminder.Title.Should().Be("Follow up with applicant");
        reminder.Notes.Should().Be("Need to confirm documents");
        reminder.DueDateTime.Should().BeCloseTo(dueDateTime, TimeSpan.FromSeconds(1));
        reminder.Priority.Should().Be(ReminderPriority.Normal);
        reminder.EntityType.Should().Be("Applicant");
        reminder.EntityId.Should().Be(_entityId);
        reminder.Status.Should().Be(ReminderStatus.Open);
        reminder.SendEmailNotification.Should().BeFalse();
        reminder.SnoozeCount.Should().Be(0);
        reminder.CreatedBy.Should().Be(_userId);
        reminder.CompletedAt.Should().BeNull();
        reminder.CompletedBy.Should().BeNull();
    }

    [Fact]
    public void Create_WithPriority_ShouldSetPriority()
    {
        // Arrange
        var dueDateTime = DateTime.UtcNow.AddDays(1);

        // Act
        var reminder = FollowUpReminder.Create(
            title: "Urgent follow-up",
            dueDateTime: dueDateTime,
            entityType: "Applicant",
            entityId: _entityId,
            createdBy: _userId,
            priority: ReminderPriority.Urgent);

        // Assert
        reminder.Priority.Should().Be(ReminderPriority.Urgent);
    }

    [Fact]
    public void Create_WithAssignedUser_ShouldSetAssignedUser()
    {
        // Arrange
        var dueDateTime = DateTime.UtcNow.AddDays(1);
        var assignedUserId = Guid.NewGuid();

        // Act
        var reminder = FollowUpReminder.Create(
            title: "Assigned task",
            dueDateTime: dueDateTime,
            entityType: "Applicant",
            entityId: _entityId,
            createdBy: _userId,
            assignedToUserId: assignedUserId);

        // Assert
        reminder.AssignedToUserId.Should().Be(assignedUserId);
    }

    [Fact]
    public void Create_WithEmailNotification_ShouldSetFlag()
    {
        // Arrange
        var dueDateTime = DateTime.UtcNow.AddDays(1);

        // Act
        var reminder = FollowUpReminder.Create(
            title: "Send email reminder",
            dueDateTime: dueDateTime,
            entityType: "Applicant",
            entityId: _entityId,
            createdBy: _userId,
            sendEmailNotification: true);

        // Assert
        reminder.SendEmailNotification.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrow()
    {
        // Arrange
        var dueDateTime = DateTime.UtcNow.AddDays(1);

        // Act
        var act = () => FollowUpReminder.Create(
            title: "",
            dueDateTime: dueDateTime,
            entityType: "Applicant",
            entityId: _entityId,
            createdBy: _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Fact]
    public void Create_WithTitleExceeding200Characters_ShouldThrow()
    {
        // Arrange
        var dueDateTime = DateTime.UtcNow.AddDays(1);
        var longTitle = new string('a', 201);

        // Act
        var act = () => FollowUpReminder.Create(
            title: longTitle,
            dueDateTime: dueDateTime,
            entityType: "Applicant",
            entityId: _entityId,
            createdBy: _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Fact]
    public void Create_WithEmptyEntityType_ShouldThrow()
    {
        // Arrange
        var dueDateTime = DateTime.UtcNow.AddDays(1);

        // Act
        var act = () => FollowUpReminder.Create(
            title: "Test",
            dueDateTime: dueDateTime,
            entityType: "",
            entityId: _entityId,
            createdBy: _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("entityType");
    }

    [Fact]
    public void Create_WithPastDueDateTime_ShouldThrow()
    {
        // Arrange
        var pastDateTime = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => FollowUpReminder.Create(
            title: "Test",
            dueDateTime: pastDateTime,
            entityType: "Applicant",
            entityId: _entityId,
            createdBy: _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("dueDateTime");
    }

    [Fact]
    public void Create_WithNowDateTime_ShouldSucceed()
    {
        // Arrange - allow slight flexibility for clock drift
        var now = DateTime.UtcNow;

        // Act
        var reminder = FollowUpReminder.Create(
            title: "Now task",
            dueDateTime: now,
            entityType: "Applicant",
            entityId: _entityId,
            createdBy: _userId);

        // Assert
        reminder.DueDateTime.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Complete_ShouldMarkAsCompleted()
    {
        // Arrange
        var reminder = CreateTestReminder();
        var completedBy = Guid.NewGuid();

        // Act
        reminder.Complete(completedBy);

        // Assert
        reminder.Status.Should().Be(ReminderStatus.Completed);
        reminder.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        reminder.CompletedBy.Should().Be(completedBy);
        reminder.SnoozedUntil.Should().BeNull();
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrow()
    {
        // Arrange
        var reminder = CreateTestReminder();
        reminder.Complete(_userId);

        // Act
        var act = () => reminder.Complete(_userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already completed*");
    }

    [Fact]
    public void Snooze_ShouldSetSnoozedStatus()
    {
        // Arrange
        var reminder = CreateTestReminder();
        var snoozeUntil = DateTime.UtcNow.AddDays(3);

        // Act
        reminder.Snooze(snoozeUntil, _userId);

        // Assert
        reminder.Status.Should().Be(ReminderStatus.Snoozed);
        reminder.SnoozedUntil.Should().BeCloseTo(snoozeUntil, TimeSpan.FromSeconds(1));
        reminder.SnoozeCount.Should().Be(1);
    }

    [Fact]
    public void Snooze_MultipleTimes_ShouldIncrementSnoozeCount()
    {
        // Arrange
        var reminder = CreateTestReminder();

        // Act
        reminder.Snooze(DateTime.UtcNow.AddDays(1), _userId);
        reminder.Snooze(DateTime.UtcNow.AddDays(2), _userId);
        reminder.Snooze(DateTime.UtcNow.AddDays(3), _userId);

        // Assert
        reminder.SnoozeCount.Should().Be(3);
    }

    [Fact]
    public void Snooze_WhenCompleted_ShouldThrow()
    {
        // Arrange
        var reminder = CreateTestReminder();
        reminder.Complete(_userId);

        // Act
        var act = () => reminder.Snooze(DateTime.UtcNow.AddDays(1), _userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public void Snooze_WithPastDateTime_ShouldThrow()
    {
        // Arrange
        var reminder = CreateTestReminder();
        var pastDateTime = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => reminder.Snooze(pastDateTime, _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("snoozeUntil");
    }

    [Fact]
    public void Dismiss_ShouldSetDismissedStatus()
    {
        // Arrange
        var reminder = CreateTestReminder();
        var dismissedBy = Guid.NewGuid();

        // Act
        reminder.Dismiss(dismissedBy);

        // Assert
        reminder.Status.Should().Be(ReminderStatus.Dismissed);
        reminder.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        reminder.CompletedBy.Should().Be(dismissedBy);
    }

    [Fact]
    public void Dismiss_WhenCompleted_ShouldThrow()
    {
        // Arrange
        var reminder = CreateTestReminder();
        reminder.Complete(_userId);

        // Act
        var act = () => reminder.Dismiss(_userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public void Reopen_FromCompleted_ShouldResetStatus()
    {
        // Arrange
        var reminder = CreateTestReminder();
        reminder.Complete(_userId);

        // Act
        reminder.Reopen();

        // Assert
        reminder.Status.Should().Be(ReminderStatus.Open);
        reminder.CompletedAt.Should().BeNull();
        reminder.CompletedBy.Should().BeNull();
        reminder.SnoozedUntil.Should().BeNull();
    }

    [Fact]
    public void Reopen_FromDismissed_ShouldResetStatus()
    {
        // Arrange
        var reminder = CreateTestReminder();
        reminder.Dismiss(_userId);

        // Act
        reminder.Reopen();

        // Assert
        reminder.Status.Should().Be(ReminderStatus.Open);
    }

    [Fact]
    public void Reopen_WhenAlreadyOpen_ShouldThrow()
    {
        // Arrange
        var reminder = CreateTestReminder();

        // Act
        var act = () => reminder.Reopen();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already open*");
    }

    [Fact]
    public void Reopen_WhenSnoozed_ShouldThrow()
    {
        // Arrange
        var reminder = CreateTestReminder();
        reminder.Snooze(DateTime.UtcNow.AddDays(1), _userId);

        // Act
        var act = () => reminder.Reopen();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already open*");
    }

    [Fact]
    public void Update_ShouldUpdateFields()
    {
        // Arrange
        var reminder = CreateTestReminder();
        var newDueDateTime = DateTime.UtcNow.AddDays(5);
        var newAssignee = Guid.NewGuid();

        // Act
        reminder.Update(
            title: "Updated title",
            dueDateTime: newDueDateTime,
            priority: ReminderPriority.High,
            notes: "Updated notes",
            assignedToUserId: newAssignee,
            sendEmailNotification: true);

        // Assert
        reminder.Title.Should().Be("Updated title");
        reminder.DueDateTime.Should().BeCloseTo(newDueDateTime, TimeSpan.FromSeconds(1));
        reminder.Priority.Should().Be(ReminderPriority.High);
        reminder.Notes.Should().Be("Updated notes");
        reminder.AssignedToUserId.Should().Be(newAssignee);
        reminder.SendEmailNotification.Should().BeTrue();
    }

    [Fact]
    public void Update_WithEmptyTitle_ShouldThrow()
    {
        // Arrange
        var reminder = CreateTestReminder();

        // Act
        var act = () => reminder.Update(title: "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Fact]
    public void Update_WithPastDueDateTime_ShouldThrow()
    {
        // Arrange
        var reminder = CreateTestReminder();
        var pastDateTime = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => reminder.Update(dueDateTime: pastDateTime);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("dueDateTime");
    }

    [Fact]
    public void Update_WithNullValues_ShouldNotChangeExistingValues()
    {
        // Arrange
        var reminder = CreateTestReminder();
        var originalTitle = reminder.Title;
        var originalDueDateTime = reminder.DueDateTime;

        // Act
        reminder.Update(notes: "Only updating notes");

        // Assert
        reminder.Title.Should().Be(originalTitle);
        reminder.DueDateTime.Should().Be(originalDueDateTime);
        reminder.Notes.Should().Be("Only updating notes");
    }

    [Fact]
    public void EffectiveDueDateTime_WhenNotSnoozed_ShouldReturnDueDateTime()
    {
        // Arrange
        var dueDateTime = DateTime.UtcNow.AddDays(1);
        var reminder = FollowUpReminder.Create(
            title: "Test",
            dueDateTime: dueDateTime,
            entityType: "Applicant",
            entityId: _entityId,
            createdBy: _userId);

        // Assert
        reminder.EffectiveDueDateTime.Should().BeCloseTo(dueDateTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void EffectiveDueDateTime_WhenSnoozed_ShouldReturnSnoozedUntil()
    {
        // Arrange
        var dueDateTime = DateTime.UtcNow.AddDays(1);
        var snoozeUntil = DateTime.UtcNow.AddDays(3);
        var reminder = FollowUpReminder.Create(
            title: "Test",
            dueDateTime: dueDateTime,
            entityType: "Applicant",
            entityId: _entityId,
            createdBy: _userId);

        // Act
        reminder.Snooze(snoozeUntil, _userId);

        // Assert
        reminder.EffectiveDueDateTime.Should().BeCloseTo(snoozeUntil, TimeSpan.FromSeconds(1));
    }

    private FollowUpReminder CreateTestReminder()
    {
        return FollowUpReminder.Create(
            title: "Test reminder",
            dueDateTime: DateTime.UtcNow.AddDays(1),
            entityType: "Applicant",
            entityId: _entityId,
            createdBy: _userId,
            notes: "Test notes");
    }
}
