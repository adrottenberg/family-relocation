namespace FamilyRelocation.Application.Reminders.DTOs;

/// <summary>
/// DTO for a paginated list of reminders.
/// </summary>
public class RemindersListDto
{
    public IReadOnlyList<ReminderDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int OverdueCount { get; init; }
    public int DueTodayCount { get; init; }
}
