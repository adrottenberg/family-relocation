using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Reminders.Commands.DismissReminder;

/// <summary>
/// Handler for DismissReminderCommand.
/// </summary>
public class DismissReminderCommandHandler : IRequestHandler<DismissReminderCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DismissReminderCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DismissReminderCommand command, CancellationToken cancellationToken)
    {
        var reminder = await _context.Set<FollowUpReminder>()
            .FirstOrDefaultAsync(r => r.Id == command.ReminderId, cancellationToken)
            ?? throw new NotFoundException("Reminder", command.ReminderId);

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to dismiss reminders.");
        reminder.Dismiss(userId);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
