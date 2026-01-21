namespace FamilyRelocation.Application.Reminders.DTOs;

/// <summary>
/// DTO for the due reminders dashboard report.
/// </summary>
public class DueRemindersReportDto
{
    public IReadOnlyList<ReminderDto> Overdue { get; init; } = [];
    public IReadOnlyList<ReminderDto> DueToday { get; init; } = [];
    public IReadOnlyList<ReminderDto> Upcoming { get; init; } = [];
    public int OverdueCount { get; init; }
    public int DueTodayCount { get; init; }
    public int UpcomingCount { get; init; }
    public int TotalOpenCount { get; init; }
}
