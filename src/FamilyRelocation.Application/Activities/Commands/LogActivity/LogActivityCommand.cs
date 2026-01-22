using MediatR;

namespace FamilyRelocation.Application.Activities.Commands.LogActivity;

/// <summary>
/// Command to log a manual activity (phone call, note, etc.) for an entity.
/// </summary>
public record LogActivityCommand(
    string EntityType,
    Guid EntityId,
    string Type,
    string Description,
    int? DurationMinutes = null,
    string? Outcome = null,
    bool CreateFollowUp = false,
    DateTime? FollowUpDate = null,
    string? FollowUpTitle = null
) : IRequest<LogActivityResult>;

/// <summary>
/// Result of logging an activity.
/// </summary>
public record LogActivityResult(
    Guid ActivityId,
    Guid? FollowUpReminderId = null
);
