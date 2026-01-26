using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Showings.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Showings.Commands.ScheduleShowing;

public class ScheduleShowingCommandHandler : IRequestHandler<ScheduleShowingCommand, ShowingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogger _activityLogger;

    public ScheduleShowingCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IActivityLogger activityLogger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _activityLogger = activityLogger;
    }

    public async Task<ShowingDto> Handle(ScheduleShowingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        // Verify property match exists and load navigation properties
        var propertyMatch = await _context.Set<PropertyMatch>()
            .Include(m => m.Property)
                .ThenInclude(p => p.Photos)
            .Include(m => m.HousingSearch)
                .ThenInclude(h => h.Applicant)
            .FirstOrDefaultAsync(m => m.Id == request.PropertyMatchId, cancellationToken);

        if (propertyMatch == null)
        {
            throw new NotFoundException(nameof(PropertyMatch), request.PropertyMatchId);
        }

        // Check for existing future scheduled showing for this property match
        // Rule: Only one future showing can be scheduled per property match at a time
        var now = DateTime.UtcNow;
        var existingFutureShowing = await _context.Set<Showing>()
            .Where(s => s.PropertyMatchId == request.PropertyMatchId &&
                       s.Status == Domain.Enums.ShowingStatus.Scheduled &&
                       s.ScheduledDateTime >= now)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingFutureShowing != null)
        {
            throw new ArgumentException(
                $"A showing is already scheduled for this property match on {existingFutureShowing.ScheduledDateTime:MMM d, yyyy 'at' h:mm tt}. " +
                "Please reschedule the existing showing instead of creating a new one.");
        }

        // Create the showing
        var showing = Showing.Create(
            propertyMatchId: request.PropertyMatchId,
            scheduledDateTime: request.ScheduledDateTime,
            createdBy: userId,
            notes: request.Notes,
            brokerUserId: request.BrokerUserId);

        _context.Add(showing);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties for DTO
        var savedShowing = await _context.Set<Showing>()
            .Include(s => s.PropertyMatch)
                .ThenInclude(m => m.Property)
                    .ThenInclude(p => p.Photos)
            .Include(s => s.PropertyMatch)
                .ThenInclude(m => m.HousingSearch)
                    .ThenInclude(h => h.Applicant)
            .FirstAsync(s => s.Id == showing.Id, cancellationToken);

        var familyName = savedShowing.PropertyMatch.HousingSearch.Applicant?.Husband?.LastName ?? "Unknown";
        var propertyAddress = $"{savedShowing.PropertyMatch.Property.Address.Street}, {savedShowing.PropertyMatch.Property.Address.City}";

        await _activityLogger.LogAsync(
            "Showing",
            showing.Id,
            "Scheduled",
            $"Showing scheduled for {familyName} family at {propertyAddress} on {request.ScheduledDateTime:MMM d, yyyy 'at' h:mm tt}",
            cancellationToken);

        return savedShowing.ToDto();
    }
}
