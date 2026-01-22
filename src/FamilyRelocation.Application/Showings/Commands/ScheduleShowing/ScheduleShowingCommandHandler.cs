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

    public ScheduleShowingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
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

        // Create the showing
        var showing = Showing.Create(
            propertyMatchId: request.PropertyMatchId,
            scheduledDate: request.ScheduledDate,
            scheduledTime: request.ScheduledTime,
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

        return savedShowing.ToDto();
    }
}
