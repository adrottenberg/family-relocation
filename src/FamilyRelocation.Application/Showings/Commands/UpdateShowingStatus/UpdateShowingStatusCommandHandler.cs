using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Showings.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Showings.Commands.UpdateShowingStatus;

public class UpdateShowingStatusCommandHandler : IRequestHandler<UpdateShowingStatusCommand, ShowingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogger _activityLogger;

    public UpdateShowingStatusCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IActivityLogger activityLogger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _activityLogger = activityLogger;
    }

    public async Task<ShowingDto> Handle(UpdateShowingStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        // Parse status
        if (!Enum.TryParse<ShowingStatus>(request.Status, true, out var newStatus))
        {
            throw new ValidationException($"Invalid status: {request.Status}. Valid values are: {string.Join(", ", Enum.GetNames<ShowingStatus>())}");
        }

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

        var oldStatus = showing.Status;

        // Apply status change based on target status
        switch (newStatus)
        {
            case ShowingStatus.Completed:
                showing.MarkCompleted(userId, request.Notes);
                break;
            case ShowingStatus.Cancelled:
                showing.Cancel(userId, request.Notes);
                break;
            case ShowingStatus.NoShow:
                showing.MarkNoShow(userId, request.Notes);
                break;
            default:
                throw new ValidationException($"Cannot transition to status: {newStatus}");
        }

        await _context.SaveChangesAsync(cancellationToken);

        var familyName = showing.PropertyMatch.HousingSearch.Applicant?.Husband?.LastName ?? "Unknown";
        var propertyAddress = $"{showing.PropertyMatch.Property.Address.Street}, {showing.PropertyMatch.Property.Address.City}";

        await _activityLogger.LogAsync(
            "Showing",
            showing.Id,
            newStatus.ToString(),
            $"Showing for {familyName} family at {propertyAddress} marked as {newStatus}",
            cancellationToken);

        return showing.ToDto();
    }
}
