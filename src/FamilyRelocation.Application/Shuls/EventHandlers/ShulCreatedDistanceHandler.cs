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
/// Handles ShulCreated events by calculating walking distances from all active properties.
/// The actual calculation work is queued to run in the background.
/// </summary>
public class ShulCreatedDistanceHandler : INotificationHandler<DomainEventNotification<ShulCreated>>
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<ShulCreatedDistanceHandler> _logger;

    public ShulCreatedDistanceHandler(
        IBackgroundTaskQueue taskQueue,
        ILogger<ShulCreatedDistanceHandler> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<ShulCreated> notification, CancellationToken cancellationToken)
    {
        var shulId = notification.DomainEvent.ShulId;
        _logger.LogInformation("Queueing distance calculations for shul {ShulId}", shulId);

        // Queue the work to run in the background
        _taskQueue.QueueBackgroundWorkItem(async (serviceProvider, ct) =>
        {
            var context = serviceProvider.GetRequiredService<IApplicationDbContext>();
            var walkingDistanceService = serviceProvider.GetRequiredService<IWalkingDistanceService>();
            var logger = serviceProvider.GetRequiredService<ILogger<ShulCreatedDistanceHandler>>();

            await CalculateDistancesAsync(context, walkingDistanceService, logger, shulId, ct);
        });

        return Task.CompletedTask;
    }

    private static async Task CalculateDistancesAsync(
        IApplicationDbContext context,
        IWalkingDistanceService walkingDistanceService,
        ILogger logger,
        Guid shulId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing ShulCreated event for distance calculations, shul {ShulId}", shulId);

        // Get the shul
        var shul = await context.Set<Shul>()
            .FirstOrDefaultAsync(s => s.Id == shulId, cancellationToken);

        if (shul == null)
        {
            logger.LogWarning("Shul {ShulId} not found", shulId);
            return;
        }

        if (shul.Location == null)
        {
            logger.LogInformation("Shul {ShulId} has no coordinates, skipping distance calculation", shulId);
            return;
        }

        // Get all active properties
        var properties = await context.Set<Property>()
            .Where(p => p.Status == ListingStatus.Active)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Calculating distances from {Count} active properties to shul {ShulId}",
            properties.Count, shulId);

        var distancesCreated = 0;

        foreach (var property in properties)
        {
            try
            {
                // Check if distance already exists
                var existingDistance = await context.Set<PropertyShulDistance>()
                    .AnyAsync(d => d.PropertyId == property.Id && d.ShulId == shulId, cancellationToken);

                if (existingDistance)
                {
                    continue;
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
                    logger.LogWarning("Could not geocode property {PropertyId} address", property.Id);
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
                        property.Id,
                        shulId,
                        result.DistanceMiles,
                        result.WalkingTimeMinutes);

                    context.Add(distance);
                    distancesCreated++;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calculating distance from property {PropertyId} to shul {ShulId}",
                    property.Id, shulId);
            }
        }

        if (distancesCreated > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Created {Count} distance calculations for shul {ShulId}",
                distancesCreated, shulId);
        }
    }
}
