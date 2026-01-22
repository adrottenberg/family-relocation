using FamilyRelocation.Application.Common;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyRelocation.Application.Shuls.EventHandlers;

/// <summary>
/// Handles PropertyCreated events by calculating walking distances to all active shuls.
/// </summary>
public class PropertyCreatedDistanceHandler : INotificationHandler<DomainEventNotification<PropertyCreated>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWalkingDistanceService _walkingDistanceService;
    private readonly ILogger<PropertyCreatedDistanceHandler> _logger;

    public PropertyCreatedDistanceHandler(
        IApplicationDbContext context,
        IWalkingDistanceService walkingDistanceService,
        ILogger<PropertyCreatedDistanceHandler> logger)
    {
        _context = context;
        _walkingDistanceService = walkingDistanceService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<PropertyCreated> notification, CancellationToken cancellationToken)
    {
        var propertyId = notification.DomainEvent.PropertyId;
        _logger.LogInformation("Processing PropertyCreated event for shul distances, property {PropertyId}", propertyId);

        // Get the property
        var property = await _context.Set<Property>()
            .FirstOrDefaultAsync(p => p.Id == propertyId, cancellationToken);

        if (property == null)
        {
            _logger.LogWarning("Property {PropertyId} not found", propertyId);
            return;
        }

        // Only calculate for active properties
        if (property.Status != ListingStatus.Active)
        {
            _logger.LogInformation("Property {PropertyId} is not active, skipping distance calculation", propertyId);
            return;
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
            _logger.LogWarning("Could not geocode property {PropertyId} address", propertyId);
            return;
        }

        // Get all active shuls with coordinates
        var shuls = await _context.Set<Shul>()
            .Where(s => s.Location != null)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Calculating distances from property {PropertyId} to {Count} shuls", propertyId, shuls.Count);

        var distancesCreated = 0;

        foreach (var shul in shuls)
        {
            if (shul.Location == null) continue;

            try
            {
                // Check if distance already exists
                var existingDistance = await _context.Set<PropertyShulDistance>()
                    .AnyAsync(d => d.PropertyId == propertyId && d.ShulId == shul.Id, cancellationToken);

                if (existingDistance)
                {
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
                        propertyId,
                        shul.Id,
                        result.DistanceMiles,
                        result.WalkingTimeMinutes);

                    _context.Add(distance);
                    distancesCreated++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance from property {PropertyId} to shul {ShulId}",
                    propertyId, shul.Id);
            }
        }

        if (distancesCreated > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created {Count} distance calculations for property {PropertyId}",
                distancesCreated, propertyId);
        }
    }
}
