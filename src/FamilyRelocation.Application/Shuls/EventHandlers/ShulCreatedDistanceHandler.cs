using FamilyRelocation.Application.Common;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyRelocation.Application.Shuls.EventHandlers;

/// <summary>
/// Handles ShulCreated events by calculating walking distances from all active properties.
/// </summary>
public class ShulCreatedDistanceHandler : INotificationHandler<DomainEventNotification<ShulCreated>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWalkingDistanceService _walkingDistanceService;
    private readonly ILogger<ShulCreatedDistanceHandler> _logger;

    public ShulCreatedDistanceHandler(
        IApplicationDbContext context,
        IWalkingDistanceService walkingDistanceService,
        ILogger<ShulCreatedDistanceHandler> logger)
    {
        _context = context;
        _walkingDistanceService = walkingDistanceService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<ShulCreated> notification, CancellationToken cancellationToken)
    {
        var shulId = notification.DomainEvent.ShulId;
        _logger.LogInformation("Processing ShulCreated event for distance calculations, shul {ShulId}", shulId);

        // Get the shul
        var shul = await _context.Set<Shul>()
            .FirstOrDefaultAsync(s => s.Id == shulId, cancellationToken);

        if (shul == null)
        {
            _logger.LogWarning("Shul {ShulId} not found", shulId);
            return;
        }

        if (shul.Location == null)
        {
            _logger.LogInformation("Shul {ShulId} has no coordinates, skipping distance calculation", shulId);
            return;
        }

        // Get all active properties
        var properties = await _context.Set<Property>()
            .Where(p => p.Status == ListingStatus.Active)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Calculating distances from {Count} active properties to shul {ShulId}",
            properties.Count, shulId);

        var distancesCreated = 0;

        foreach (var property in properties)
        {
            try
            {
                // Check if distance already exists
                var existingDistance = await _context.Set<PropertyShulDistance>()
                    .AnyAsync(d => d.PropertyId == property.Id && d.ShulId == shulId, cancellationToken);

                if (existingDistance)
                {
                    continue;
                }

                // Get property coordinates via geocoding
                var propertyCoords = await _walkingDistanceService.GeocodeAddressAsync(
                    property.Address.Street,
                    property.Address.City,
                    property.Address.State,
                    property.Address.ZipCode,
                    cancellationToken);

                if (propertyCoords == null)
                {
                    _logger.LogWarning("Could not geocode property {PropertyId} address", property.Id);
                    continue;
                }

                // Calculate walking distance
                var result = await _walkingDistanceService.GetWalkingDistanceAsync(
                    propertyCoords.Latitude,
                    propertyCoords.Longitude,
                    shul.Location.Latitude,
                    shul.Location.Longitude,
                    cancellationToken);

                if (result != null)
                {
                    var distance = PropertyShulDistance.Create(
                        property.Id,
                        shulId,
                        result.DistanceMiles,
                        result.WalkingTimeMinutes);

                    _context.Add(distance);
                    distancesCreated++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance from property {PropertyId} to shul {ShulId}",
                    property.Id, shulId);
            }
        }

        if (distancesCreated > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created {Count} distance calculations for shul {ShulId}",
                distancesCreated, shulId);
        }
    }
}
