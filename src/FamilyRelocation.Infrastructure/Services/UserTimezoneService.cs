using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Infrastructure.Services;

/// <summary>
/// Service for handling user timezone conversions.
/// </summary>
public class UserTimezoneService : IUserTimezoneService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private const string DefaultTimezoneId = "America/New_York";

    // Cache the timezone for the current request to avoid repeated DB queries
    private string? _cachedTimezoneId;
    private TimeZoneInfo? _cachedTimezoneInfo;

    public UserTimezoneService(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<string> GetCurrentUserTimezoneIdAsync()
    {
        if (_cachedTimezoneId != null)
            return _cachedTimezoneId;

        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            _cachedTimezoneId = DefaultTimezoneId;
            return _cachedTimezoneId;
        }

        var settings = await _dbContext.Set<UserSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);

        _cachedTimezoneId = settings?.TimeZoneId ?? DefaultTimezoneId;
        return _cachedTimezoneId;
    }

    public async Task<TimeZoneInfo> GetCurrentUserTimezoneAsync()
    {
        if (_cachedTimezoneInfo != null)
            return _cachedTimezoneInfo;

        var timezoneId = await GetCurrentUserTimezoneIdAsync();

        try
        {
            _cachedTimezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback to default if stored timezone is invalid
            _cachedTimezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(DefaultTimezoneId);
        }

        return _cachedTimezoneInfo;
    }

    public async Task<DateTime> GetTodayStartUtcAsync()
    {
        var timezone = await GetCurrentUserTimezoneAsync();
        var nowUtc = DateTime.UtcNow;
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone);
        var todayStartLocal = nowLocal.Date;
        return TimeZoneInfo.ConvertTimeToUtc(todayStartLocal, timezone);
    }

    public async Task<DateTime> GetTodayEndUtcAsync()
    {
        var timezone = await GetCurrentUserTimezoneAsync();
        var nowUtc = DateTime.UtcNow;
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone);
        var todayEndLocal = nowLocal.Date.AddDays(1).AddTicks(-1);
        return TimeZoneInfo.ConvertTimeToUtc(todayEndLocal, timezone);
    }

    public async Task<DateTime> ConvertToUserLocalAsync(DateTime utcDateTime)
    {
        var timezone = await GetCurrentUserTimezoneAsync();
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, timezone);
    }

    public async Task<DateTime> ConvertToUtcAsync(DateTime localDateTime)
    {
        var timezone = await GetCurrentUserTimezoneAsync();
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified), timezone);
    }

    public async Task<bool> IsTodayAsync(DateTime utcDateTime)
    {
        var todayStart = await GetTodayStartUtcAsync();
        var todayEnd = await GetTodayEndUtcAsync();
        return utcDateTime >= todayStart && utcDateTime <= todayEnd;
    }

    public async Task<bool> IsOverdueAsync(DateTime utcDateTime)
    {
        return utcDateTime < DateTime.UtcNow;
    }
}
