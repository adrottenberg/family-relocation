using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Shuls.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Shuls.Commands.UpdateShul;

public class UpdateShulCommandHandler : IRequestHandler<UpdateShulCommand, ShulDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWalkingDistanceService _walkingDistanceService;
    private readonly IActivityLogger _activityLogger;

    public UpdateShulCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IWalkingDistanceService walkingDistanceService,
        IActivityLogger activityLogger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _walkingDistanceService = walkingDistanceService;
        _activityLogger = activityLogger;
    }

    public async Task<ShulDto> Handle(UpdateShulCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to update a shul");

        var shul = await _context.Set<Shul>()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Shul with ID {request.Id} not found");

        var address = new Address(
            request.Street,
            request.City,
            request.State,
            request.ZipCode,
            request.Street2);

        // Try to get coordinates if not provided
        Coordinates? location = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            location = new Coordinates(request.Latitude.Value, request.Longitude.Value);
        }
        else
        {
            // Try to geocode the address
            var geocodeResult = await _walkingDistanceService.GeocodeAddressAsync(
                request.Street,
                request.City,
                request.State,
                request.ZipCode,
                cancellationToken);

            if (geocodeResult != null)
            {
                location = new Coordinates(geocodeResult.Latitude, geocodeResult.Longitude);
            }
        }

        shul.Update(
            name: request.Name,
            address: address,
            modifiedBy: currentUserId,
            location: location,
            rabbi: request.Rabbi,
            denomination: request.Denomination,
            website: request.Website,
            notes: request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(
            entityType: "Shul",
            entityId: shul.Id,
            action: "Updated",
            description: $"Updated shul: {shul.Name}",
            ct: cancellationToken);

        return ShulMapper.ToDto(shul);
    }
}
