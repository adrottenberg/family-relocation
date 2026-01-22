using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Showings.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Showings.Commands.RescheduleShowing;

public class RescheduleShowingCommandHandler : IRequestHandler<RescheduleShowingCommand, ShowingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogger _activityLogger;

    public RescheduleShowingCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IActivityLogger activityLogger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _activityLogger = activityLogger;
    }

    public async Task<ShowingDto> Handle(RescheduleShowingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var showing = await _context.Set<Showing>()
            .Include(s => s.PropertyMatch)
                .ThenInclude(m => m.Property)
                    .ThenInclude(p => p.Photos)
            .Include(s => s.PropertyMatch)
                .ThenInclude(m => m.HousingSearch)
                    .ThenInclude(h => h.Applicant)
            .FirstOrDefaultAsync(s => s.Id == request.ShowingId, cancellationToken);

        if (showing == null)
        {
            throw new NotFoundException(nameof(Showing), request.ShowingId);
        }

        var oldDate = showing.ScheduledDate;
        var oldTime = showing.ScheduledTime;

        showing.Reschedule(request.NewDate, request.NewTime, userId);
        await _context.SaveChangesAsync(cancellationToken);

        var familyName = showing.PropertyMatch.HousingSearch.Applicant?.Husband?.LastName ?? "Unknown";
        var propertyAddress = $"{showing.PropertyMatch.Property.Address.Street}, {showing.PropertyMatch.Property.Address.City}";

        await _activityLogger.LogAsync(
            "Showing",
            showing.Id,
            "Rescheduled",
            $"Showing for {familyName} family at {propertyAddress} rescheduled from {oldDate:MMM d} at {oldTime} to {request.NewDate:MMM d, yyyy} at {request.NewTime}",
            cancellationToken);

        return showing.ToDto();
    }
}
