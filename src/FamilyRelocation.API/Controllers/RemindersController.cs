using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Reminders.Commands.CompleteReminder;
using FamilyRelocation.Application.Reminders.Commands.CreateReminder;
using FamilyRelocation.Application.Reminders.Commands.DismissReminder;
using FamilyRelocation.Application.Reminders.Commands.ReopenReminder;
using FamilyRelocation.Application.Reminders.Commands.SnoozeReminder;
using FamilyRelocation.Application.Reminders.Commands.UpdateReminder;
using FamilyRelocation.Application.Reminders.DTOs;
using FamilyRelocation.Application.Reminders.Queries.GetDueReminders;
using FamilyRelocation.Application.Reminders.Queries.GetReminderById;
using FamilyRelocation.Application.Reminders.Queries.GetReminders;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for follow-up reminder operations.
/// </summary>
[ApiController]
[Route("api/reminders")]
[Authorize]
public class RemindersController : ControllerBase
{
    private readonly IMediator _mediator;

    public RemindersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets reminders with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(RemindersListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReminders(
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] ReminderStatus? status = null,
        [FromQuery] ReminderPriority? priority = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] DateTime? dueDateFrom = null,
        [FromQuery] DateTime? dueDateTo = null,
        [FromQuery] bool? overdueOnly = null,
        [FromQuery] bool? dueTodayOnly = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRemindersQuery(
            entityType,
            entityId,
            status,
            priority,
            assignedToUserId,
            dueDateFrom,
            dueDateTo,
            overdueOnly,
            dueTodayOnly,
            skip,
            take);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a reminder by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReminderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReminderByIdQuery(id), cancellationToken);
        if (result == null)
            return NotFound(new { message = "Reminder not found" });

        return Ok(result);
    }

    /// <summary>
    /// Gets a due reminders report for the dashboard.
    /// </summary>
    /// <remarks>
    /// Returns overdue reminders, reminders due today, and upcoming reminders
    /// for the next N days (default 7).
    /// </remarks>
    [HttpGet("due-report")]
    [ProducesResponseType(typeof(DueRemindersReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDueReport(
        [FromQuery] int upcomingDays = 7,
        [FromQuery] Guid? assignedToUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDueRemindersQuery(upcomingDays, assignedToUserId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets reminders for a specific entity (applicant, housing search, etc.).
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(RemindersListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        Guid entityId,
        [FromQuery] ReminderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRemindersQuery(
            EntityType: entityType,
            EntityId: entityId,
            Status: status);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new reminder.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReminderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateReminderRequest request,
        CancellationToken cancellationToken)
    {
        // Ensure the DueDate is treated as a calendar date (no timezone shift)
        // Parse the date and create it as UTC midnight to avoid timezone conversion issues
        var dueDate = DateTime.SpecifyKind(request.DueDate.Date, DateTimeKind.Utc);

        var command = new CreateReminderCommand(
            request.Title,
            dueDate,
            request.EntityType,
            request.EntityId,
            request.Notes,
            request.DueTime,
            request.Priority,
            request.AssignedToUserId,
            request.SendEmailNotification);

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing reminder.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ReminderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateReminderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Ensure the DueDate is treated as a calendar date (no timezone shift)
            DateTime? dueDate = request.DueDate.HasValue
                ? DateTime.SpecifyKind(request.DueDate.Value.Date, DateTimeKind.Utc)
                : null;

            var command = new UpdateReminderCommand(
                id,
                request.Title,
                dueDate,
                request.DueTime,
                request.Priority,
                request.Notes,
                request.AssignedToUserId,
                request.SendEmailNotification);

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Marks a reminder as completed.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new CompleteReminderCommand(id), cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Snoozes a reminder until a specified date.
    /// </summary>
    [HttpPost("{id:guid}/snooze")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Snooze(
        Guid id,
        [FromBody] SnoozeReminderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Ensure the SnoozeUntil date is treated as a calendar date (no timezone shift)
            var snoozeUntil = DateTime.SpecifyKind(request.SnoozeUntil.Date, DateTimeKind.Utc);

            await _mediator.Send(new SnoozeReminderCommand(id, snoozeUntil), cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Dismisses a reminder (soft delete).
    /// </summary>
    [HttpPost("{id:guid}/dismiss")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new DismissReminderCommand(id), cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reopens a completed or dismissed reminder.
    /// </summary>
    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new ReopenReminderCommand(id), cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

/// <summary>
/// Request model for creating a reminder.
/// </summary>
public record CreateReminderRequest(
    string Title,
    DateTime DueDate,
    string EntityType,
    Guid EntityId,
    string? Notes = null,
    TimeOnly? DueTime = null,
    ReminderPriority Priority = ReminderPriority.Normal,
    Guid? AssignedToUserId = null,
    bool SendEmailNotification = false);

/// <summary>
/// Request model for updating a reminder.
/// </summary>
public record UpdateReminderRequest(
    string? Title = null,
    DateTime? DueDate = null,
    TimeOnly? DueTime = null,
    ReminderPriority? Priority = null,
    string? Notes = null,
    Guid? AssignedToUserId = null,
    bool? SendEmailNotification = null);

/// <summary>
/// Request model for snoozing a reminder.
/// </summary>
public record SnoozeReminderRequest(DateTime SnoozeUntil);
