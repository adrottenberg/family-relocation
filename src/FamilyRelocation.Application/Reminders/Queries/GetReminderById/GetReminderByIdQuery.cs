using FamilyRelocation.Application.Reminders.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Reminders.Queries.GetReminderById;

/// <summary>
/// Query to get a specific reminder by ID.
/// </summary>
public record GetReminderByIdQuery(Guid ReminderId) : IRequest<ReminderDto?>;
