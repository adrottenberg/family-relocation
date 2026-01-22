using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;

namespace FamilyRelocation.Application.Activities.Commands.LogActivity;

public class LogActivityCommandHandler : IRequestHandler<LogActivityCommand, LogActivityResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public LogActivityCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<LogActivityResult> Handle(LogActivityCommand command, CancellationToken cancellationToken)
    {
        Guid? followUpReminderId = null;

        // Create follow-up reminder if requested
        if (command.CreateFollowUp && command.FollowUpDate.HasValue)
        {
            var reminderTitle = command.FollowUpTitle ?? $"Follow up: {command.Description.Substring(0, Math.Min(50, command.Description.Length))}...";

            var reminder = FollowUpReminder.Create(
                title: reminderTitle,
                dueDate: command.FollowUpDate.Value,
                entityType: command.EntityType,
                entityId: command.EntityId,
                createdBy: _currentUser.UserId ?? Guid.Empty,
                notes: $"Created from {command.Type} activity"
            );

            _context.Add(reminder);
            followUpReminderId = reminder.Id;
        }

        // Create activity log based on type
        var activityLog = command.Type.ToUpperInvariant() switch
        {
            "PHONECALL" => ActivityLog.CreatePhoneCall(
                entityType: command.EntityType,
                entityId: command.EntityId,
                description: command.Description,
                durationMinutes: command.DurationMinutes,
                outcome: command.Outcome,
                userId: _currentUser.UserId,
                userName: _currentUser.UserName,
                followUpReminderId: followUpReminderId),

            "NOTE" => ActivityLog.CreateNote(
                entityType: command.EntityType,
                entityId: command.EntityId,
                description: command.Description,
                userId: _currentUser.UserId,
                userName: _currentUser.UserName,
                followUpReminderId: followUpReminderId),

            "EMAIL" => ActivityLog.CreateEmail(
                entityType: command.EntityType,
                entityId: command.EntityId,
                description: command.Description,
                userId: _currentUser.UserId,
                userName: _currentUser.UserName,
                followUpReminderId: followUpReminderId),

            "SMS" => ActivityLog.CreateSMS(
                entityType: command.EntityType,
                entityId: command.EntityId,
                description: command.Description,
                userId: _currentUser.UserId,
                userName: _currentUser.UserName,
                followUpReminderId: followUpReminderId),

            _ => throw new ArgumentException($"Invalid activity type: {command.Type}")
        };

        _context.Add(activityLog);
        await _context.SaveChangesAsync(cancellationToken);

        return new LogActivityResult(activityLog.Id, followUpReminderId);
    }
}
