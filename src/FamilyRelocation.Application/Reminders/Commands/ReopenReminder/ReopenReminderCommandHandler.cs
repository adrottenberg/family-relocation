using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Reminders.Commands.ReopenReminder;

/// <summary>
/// Handler for ReopenReminderCommand.
/// </summary>
public class ReopenReminderCommandHandler : IRequestHandler<ReopenReminderCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ReopenReminderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ReopenReminderCommand command, CancellationToken cancellationToken)
    {
        var reminder = await _context.Set<FollowUpReminder>()
            .FirstOrDefaultAsync(r => r.Id == command.ReminderId, cancellationToken)
            ?? throw new NotFoundException("Reminder", command.ReminderId);

        reminder.Reopen();

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
