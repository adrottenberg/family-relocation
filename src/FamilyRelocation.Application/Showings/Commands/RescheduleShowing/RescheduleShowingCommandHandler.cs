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

    public RescheduleShowingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
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

        showing.Reschedule(request.NewDate, request.NewTime, userId);
        await _context.SaveChangesAsync(cancellationToken);

        return showing.ToDto();
    }
}
