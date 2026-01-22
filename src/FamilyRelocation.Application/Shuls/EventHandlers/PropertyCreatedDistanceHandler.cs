using FamilyRelocation.Application.Common;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FamilyRelocation.Application.Shuls.EventHandlers;

/// <summary>
/// Handles PropertyCreated events by calculating walking distances to all active shuls.
/// The actual calculation work is queued to run in the background.
/// </summary>
public class PropertyCreatedDistanceHandler : INotificationHandler<DomainEventNotification<PropertyCreated>>
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<PropertyCreatedDistanceHandler> _logger;

    public PropertyCreatedDistanceHandler(
        IBackgroundTaskQueue taskQueue,
        ILogger<PropertyCreatedDistanceHandler> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<PropertyCreated> notification, CancellationToken cancellationToken)
    {
        var propertyId = notification.DomainEvent.PropertyId;
        _logger.LogInformation("Queueing shul distance calculations for property {PropertyId}", propertyId);

        // Queue the work to run in the background
        _taskQueue.QueueBackgroundWorkItem(async (serviceProvider, ct) =>
        {
            var context = serviceProvider.GetRequiredService<IApplicationDbContext>();
            var walkingDistanceService = serviceProvider.GetRequiredService<IWalkingDistanceService>();
            var logger = serviceProvider.GetRequiredService<ILogger<PropertyCreatedDistanceHandler>>();

            await CalculateDistancesAsync(context, walkingDistanceService, logger, propertyId, ct);
        });

        return Task.CompletedTask;
    }

    private static async Task CalculateDistancesAsync(
        IApplicationDbContext context,
        IWalkingDistanceService walkingDistanceService,
        ILogger logger,
        Guid propertyId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing PropertyCreated event for shul distances, property {PropertyId}", propertyId);

        // Get the property
        var property = await context.Set<Property>()
            .FirstOrDefaultAsync(p => p.Id == propertyId, cancellationToken);

        if (property == null)
        {
            logger.LogWarning("Property {PropertyId} not found", propertyId);
            return;
        }

        // Only calculate for active properties
        if (property.Status != ListingStatus.Active)
        {
            logger.LogInformation("Property {PropertyId} is not active, skipping distance calculation", propertyId);
            return;
        }

        // Get property coordinates via geocoding
        var propertyCoords = await walkingDistanceService.GeocodeAddressAsync(
            property.Address.Street,
            property.Address.City,
            property.Address.State,
            property.Address.ZipCode,
            cancellationToken);

        if (propertyCoords == null)
        {
            logger.LogWarning("Could not geocode property {PropertyId} address", propertyId);
            return;
        }

        // Get all active shuls with coordinates
        var shuls = await context.Set<Shul>()
            .Where(s => s.Location != null)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Calculating distances from property {PropertyId} to {Count} shuls", propertyId, shuls.Count);

        var distancesCreated = 0;

        foreach (var shul in shuls)
        {
            if (shul.Location == null) continue;

            try
            {
                // Check if distance already exists
                var existingDistance = await context.Set<PropertyShulDistance>()
                    .AnyAsync(d => d.PropertyId == propertyId && d.ShulId == shul.Id, cancellationToken);

                if (existingDistance)
                {
                    continue;
                }

                // Calculate walking distance
                var result = await walkingDistanceService.GetWalkingDistanceAsync(
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

                    context.Add(distance);
                    distancesCreated++;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calculating distance from property {PropertyId} to shul {ShulId}",
                    propertyId, shul.Id);
            }
        }

        if (distancesCreated > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Created {Count} distance calculations for property {PropertyId}",
                distancesCreated, propertyId);
        }
    }
}
