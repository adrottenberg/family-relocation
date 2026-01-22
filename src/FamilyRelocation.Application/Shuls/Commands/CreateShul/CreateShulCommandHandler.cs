using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Shuls.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;

namespace FamilyRelocation.Application.Shuls.Commands.CreateShul;

public class CreateShulCommandHandler : IRequestHandler<CreateShulCommand, ShulDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWalkingDistanceService _walkingDistanceService;
    private readonly IActivityLogger _activityLogger;

    public CreateShulCommandHandler(
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

    public async Task<ShulDto> Handle(CreateShulCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to create a shul");

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

        var shul = Shul.Create(
            name: request.Name,
            address: address,
            createdBy: currentUserId,
            location: location,
            rabbi: request.Rabbi,
            denomination: request.Denomination,
            website: request.Website,
            notes: request.Notes);

        _context.Add(shul);
        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(
            entityType: "Shul",
            entityId: shul.Id,
            action: "Created",
            description: $"Created shul: {shul.Name}",
            ct: cancellationToken);

        return ShulMapper.ToDto(shul);
    }
}
