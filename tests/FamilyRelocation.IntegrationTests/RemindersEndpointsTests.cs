using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyRelocation.IntegrationTests;

public class RemindersEndpointsTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public RemindersEndpointsTests()
    {
        var dbName = "TestDb_Reminders_" + Guid.NewGuid().ToString();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove all EF Core related services
                    var descriptorsToRemove = services
                        .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                            || d.ServiceType == typeof(DbContextOptions)
                            || d.ServiceType == typeof(IApplicationDbContext)
                            || d.ServiceType == typeof(ApplicationDbContext)
                            || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                        .ToList();

                    foreach (var descriptor in descriptorsToRemove)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                    });

                    // Re-add IApplicationDbContext pointing to the in-memory context
                    services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

                    // Add test authentication scheme
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestScheme";
                        options.DefaultChallengeScheme = "TestScheme";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

                    // Add IUserTimezoneService (required by reminder handlers)
                    // Use a simple test implementation that uses UTC
                    services.AddScoped<IUserTimezoneService, TestTimezoneService>();
                });
            });

        // Seed the database
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        }

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    #region Create Reminder Tests

    [Fact]
    public async Task CreateReminder_WithValidData_ReturnsCreated()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var request = new
        {
            title = "Follow up with applicant",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14), // 2 PM
            entityType = "Applicant",
            entityId = entityId,
            notes = "Need to confirm documents",
            priority = "Normal"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/reminders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        var reminder = JsonSerializer.Deserialize<ReminderDto>(content, _jsonOptions);
        reminder.Should().NotBeNull();
        reminder!.Id.Should().NotBeEmpty();
        reminder.Title.Should().Be("Follow up with applicant");
        reminder.EntityType.Should().Be("Applicant");
        reminder.EntityId.Should().Be(entityId);
        reminder.Status.Should().Be(ReminderStatus.Open);
    }

    [Fact]
    public async Task CreateReminder_WithPriority_SetsPriority()
    {
        // Arrange
        var request = new
        {
            title = "Urgent task",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = Guid.NewGuid(),
            priority = "Urgent"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/reminders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        var reminder = JsonSerializer.Deserialize<ReminderDto>(content, _jsonOptions);
        reminder!.Priority.Should().Be(ReminderPriority.Urgent);
    }

    [Fact]
    public async Task CreateReminder_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            title = "",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/reminders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Reminders Tests

    [Fact]
    public async Task GetReminders_ReturnsOkWithList()
    {
        // Arrange - Create a reminder first
        var createRequest = new
        {
            title = "Test reminder",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = Guid.NewGuid()
        };
        var createResponse = await _client.PostAsJsonAsync("/api/reminders", createRequest);

        // Verify the create was successful
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            because: $"POST should create reminder. Response: {await createResponse.Content.ReadAsStringAsync()}");

        // Act
        var response = await _client.GetAsync("/api/reminders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RemindersListDto>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(1);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetReminders_WithStatusFilter_FiltersCorrectly()
    {
        // Arrange - Create reminders
        var entityId = Guid.NewGuid();
        var createRequest = new
        {
            title = "Open reminder",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = entityId
        };
        await _client.PostAsJsonAsync("/api/reminders", createRequest);

        // Act
        var response = await _client.GetAsync("/api/reminders?status=Open");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RemindersListDto>(content, _jsonOptions);
        result!.Items.Should().OnlyContain(r => r.Status == ReminderStatus.Open);
    }

    [Fact]
    public async Task GetReminders_ByEntity_ReturnsOnlyMatchingReminders()
    {
        // Arrange - Create reminders for different entities
        var entityId1 = Guid.NewGuid();
        var entityId2 = Guid.NewGuid();

        await _client.PostAsJsonAsync("/api/reminders", new
        {
            title = "Reminder 1",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = entityId1
        });

        await _client.PostAsJsonAsync("/api/reminders", new
        {
            title = "Reminder 2",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = entityId2
        });

        // Act
        var response = await _client.GetAsync($"/api/reminders/entity/Applicant/{entityId1}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RemindersListDto>(content, _jsonOptions);
        result!.Items.Should().OnlyContain(r => r.EntityId == entityId1);
    }

    #endregion

    #region Get Reminder By Id Tests

    [Fact]
    public async Task GetReminderById_WithValidId_ReturnsReminder()
    {
        // Arrange - Create a reminder first
        var createRequest = new
        {
            title = "Get by ID test",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = Guid.NewGuid()
        };
        var createResponse = await _client.PostAsJsonAsync("/api/reminders", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();

        // Verify the create was successful
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            because: $"POST should create reminder. Response: {createContent}");

        var created = JsonSerializer.Deserialize<ReminderDto>(createContent, _jsonOptions);
        created.Should().NotBeNull("created reminder should deserialize");

        // Act
        var response = await _client.GetAsync($"/api/reminders/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var reminder = JsonSerializer.Deserialize<ReminderDto>(content, _jsonOptions);
        reminder!.Id.Should().Be(created.Id);
        reminder.Title.Should().Be("Get by ID test");
    }

    [Fact]
    public async Task GetReminderById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/reminders/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Due Report Tests

    [Fact]
    public async Task GetDueReport_ReturnsValidReport()
    {
        // Arrange - Create a reminder due later today (use end of day to ensure it's always in the future)
        // Use 23:59 to ensure the reminder is "due today" regardless of current time
        var dueLaterToday = DateTime.UtcNow.Date.AddHours(23).AddMinutes(59);
        await _client.PostAsJsonAsync("/api/reminders", new
        {
            title = "Due today",
            dueDateTime = dueLaterToday,
            entityType = "Applicant",
            entityId = Guid.NewGuid()
        });

        // Act
        var response = await _client.GetAsync("/api/reminders/due-report");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var report = JsonSerializer.Deserialize<DueRemindersReportDto>(content, _jsonOptions);
        report.Should().NotBeNull();
        // The reminder should appear in either DueToday or Overdue depending on timing
        var totalUrgent = report!.DueTodayCount + report.OverdueCount;
        totalUrgent.Should().BeGreaterThanOrEqualTo(1, "reminder should be in due today or overdue");
    }

    [Fact]
    public async Task GetDueReport_WithUpcomingDays_ReturnsUpcoming()
    {
        // Arrange - Create a reminder due in 3 days
        await _client.PostAsJsonAsync("/api/reminders", new
        {
            title = "Upcoming reminder",
            dueDateTime = DateTime.UtcNow.Date.AddDays(3).AddHours(14),
            entityType = "Applicant",
            entityId = Guid.NewGuid()
        });

        // Act
        var response = await _client.GetAsync("/api/reminders/due-report?upcomingDays=7");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var report = JsonSerializer.Deserialize<DueRemindersReportDto>(content, _jsonOptions);
        report!.UpcomingCount.Should().BeGreaterThanOrEqualTo(1);
    }

    #endregion

    #region Complete Reminder Tests

    [Fact]
    public async Task CompleteReminder_WithValidId_ReturnsNoContent()
    {
        // Arrange - Create a reminder
        var createResponse = await _client.PostAsJsonAsync("/api/reminders", new
        {
            title = "To complete",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = Guid.NewGuid()
        });
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ReminderDto>(createContent, _jsonOptions);

        // Act
        var response = await _client.PostAsync($"/api/reminders/{created!.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify status changed
        var getResponse = await _client.GetAsync($"/api/reminders/{created.Id}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var reminder = JsonSerializer.Deserialize<ReminderDto>(getContent, _jsonOptions);
        reminder!.Status.Should().Be(ReminderStatus.Completed);
    }

    [Fact]
    public async Task CompleteReminder_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PostAsync($"/api/reminders/{Guid.NewGuid()}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Snooze Reminder Tests

    [Fact]
    public async Task SnoozeReminder_WithValidData_ReturnsNoContent()
    {
        // Arrange - Create a reminder
        var createResponse = await _client.PostAsJsonAsync("/api/reminders", new
        {
            title = "To snooze",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = Guid.NewGuid()
        });
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ReminderDto>(createContent, _jsonOptions);

        var snoozeRequest = new
        {
            snoozeUntil = DateTime.UtcNow.Date.AddDays(3)
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/reminders/{created!.Id}/snooze", snoozeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify status changed
        var getResponse = await _client.GetAsync($"/api/reminders/{created.Id}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var reminder = JsonSerializer.Deserialize<ReminderDto>(getContent, _jsonOptions);
        reminder!.Status.Should().Be(ReminderStatus.Snoozed);
        reminder.SnoozeCount.Should().Be(1);
    }

    #endregion

    #region Dismiss Reminder Tests

    [Fact]
    public async Task DismissReminder_WithValidId_ReturnsNoContent()
    {
        // Arrange - Create a reminder
        var createResponse = await _client.PostAsJsonAsync("/api/reminders", new
        {
            title = "To dismiss",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = Guid.NewGuid()
        });
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ReminderDto>(createContent, _jsonOptions);

        // Act
        var response = await _client.PostAsync($"/api/reminders/{created!.Id}/dismiss", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify status changed
        var getResponse = await _client.GetAsync($"/api/reminders/{created.Id}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var reminder = JsonSerializer.Deserialize<ReminderDto>(getContent, _jsonOptions);
        reminder!.Status.Should().Be(ReminderStatus.Dismissed);
    }

    #endregion

    #region Reopen Reminder Tests

    [Fact]
    public async Task ReopenReminder_FromCompleted_ReturnsNoContent()
    {
        // Arrange - Create and complete a reminder
        var createResponse = await _client.PostAsJsonAsync("/api/reminders", new
        {
            title = "To reopen",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = Guid.NewGuid()
        });
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ReminderDto>(createContent, _jsonOptions);

        await _client.PostAsync($"/api/reminders/{created!.Id}/complete", null);

        // Act
        var response = await _client.PostAsync($"/api/reminders/{created.Id}/reopen", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify status changed
        var getResponse = await _client.GetAsync($"/api/reminders/{created.Id}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var reminder = JsonSerializer.Deserialize<ReminderDto>(getContent, _jsonOptions);
        reminder!.Status.Should().Be(ReminderStatus.Open);
    }

    #endregion

    #region Update Reminder Tests

    [Fact]
    public async Task UpdateReminder_WithValidData_ReturnsOk()
    {
        // Arrange - Create a reminder
        var createResponse = await _client.PostAsJsonAsync("/api/reminders", new
        {
            title = "Original title",
            dueDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            entityType = "Applicant",
            entityId = Guid.NewGuid()
        });
        var createContent = await createResponse.Content.ReadAsStringAsync();

        // Verify the create was successful
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            because: $"POST should create reminder. Response: {createContent}");

        var created = JsonSerializer.Deserialize<ReminderDto>(createContent, _jsonOptions);
        created.Should().NotBeNull("created reminder should deserialize");

        var updateRequest = new
        {
            title = "Updated title",
            priority = "High",
            notes = "Updated notes"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/reminders/{created!.Id}", updateRequest);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: $"PUT should update reminder. Response: {responseContent}");
        var updated = JsonSerializer.Deserialize<ReminderDto>(responseContent, _jsonOptions);
        updated!.Title.Should().Be("Updated title");
        updated.Priority.Should().Be(ReminderPriority.High);
        updated.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateReminder_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PutAsJsonAsync($"/api/reminders/{Guid.NewGuid()}", new
        {
            title = "Test"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DTOs for deserialization

    private record ReminderDto(
        Guid Id,
        string Title,
        string? Notes,
        DateTime DueDateTime,
        ReminderPriority Priority,
        string EntityType,
        Guid EntityId,
        string? EntityDisplayName,
        Guid? AssignedToUserId,
        string? AssignedToUserName,
        ReminderStatus Status,
        bool SendEmailNotification,
        DateTime? SnoozedUntil,
        int SnoozeCount,
        DateTime CreatedAt,
        Guid CreatedBy,
        string? CreatedByName,
        DateTime? CompletedAt,
        Guid? CompletedBy,
        string? CompletedByName,
        bool IsOverdue,
        bool IsDueToday);

    private record RemindersListDto(
        IReadOnlyList<ReminderDto> Items,
        int TotalCount,
        int OverdueCount,
        int DueTodayCount);

    private record DueRemindersReportDto(
        IReadOnlyList<ReminderDto> Overdue,
        IReadOnlyList<ReminderDto> DueToday,
        IReadOnlyList<ReminderDto> Upcoming,
        int OverdueCount,
        int DueTodayCount,
        int UpcomingCount,
        int TotalOpenCount);

    #endregion
}

/// <summary>
/// Simple test implementation of IUserTimezoneService that uses UTC.
/// </summary>
public class TestTimezoneService : IUserTimezoneService
{
    public Task<string> GetCurrentUserTimezoneIdAsync() => Task.FromResult("UTC");

    public Task<TimeZoneInfo> GetCurrentUserTimezoneAsync() => Task.FromResult(TimeZoneInfo.Utc);

    public Task<DateTime> GetTodayStartUtcAsync() => Task.FromResult(DateTime.UtcNow.Date);

    public Task<DateTime> GetTodayEndUtcAsync() => Task.FromResult(DateTime.UtcNow.Date.AddDays(1).AddTicks(-1));

    public Task<DateTime> ConvertToUserLocalAsync(DateTime utcDateTime) => Task.FromResult(utcDateTime);

    public Task<DateTime> ConvertToUtcAsync(DateTime localDateTime) => Task.FromResult(localDateTime);

    public Task<bool> IsTodayAsync(DateTime utcDateTime)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);
        return Task.FromResult(utcDateTime >= todayStart && utcDateTime <= todayEnd);
    }

    public Task<bool> IsOverdueAsync(DateTime utcDateTime) => Task.FromResult(utcDateTime < DateTime.UtcNow);
}
