import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';

// Extend dayjs with timezone support
dayjs.extend(utc);
dayjs.extend(timezone);

// Default timezone
const DEFAULT_TIMEZONE = 'America/New_York';

// Get timezone from localStorage or use default
// This will be updated by the settings store when user preferences are loaded
let currentTimezone = DEFAULT_TIMEZONE;

/**
 * Set the current user's timezone
 * Called by the settings store when user preferences are loaded
 */
export const setUserTimezone = (timezone: string) => {
  currentTimezone = timezone;
};

/**
 * Get the current user's timezone
 */
export const getUserTimezone = (): string => {
  return currentTimezone;
};

/**
 * Format a UTC datetime string to user's local time
 * @param utcString - ISO datetime string in UTC
 * @param format - dayjs format string (default: 'MMM D, YYYY [at] h:mm A')
 * @returns Formatted datetime string in user's timezone
 */
export const formatDateTime = (utcString: string | undefined | null, format = 'MMM D, YYYY [at] h:mm A'): string => {
  if (!utcString) return '';
  return dayjs.utc(utcString).tz(currentTimezone).format(format);
};

/**
 * Format a UTC datetime string to just the date portion
 * @param utcString - ISO datetime string in UTC
 * @param format - dayjs format string (default: 'MMM D, YYYY')
 */
export const formatDate = (utcString: string | undefined | null, format = 'MMM D, YYYY'): string => {
  if (!utcString) return '';
  return dayjs.utc(utcString).tz(currentTimezone).format(format);
};

/**
 * Format a UTC datetime string to just the time portion
 * @param utcString - ISO datetime string in UTC
 * @param format - dayjs format string (default: 'h:mm A')
 */
export const formatTime = (utcString: string | undefined | null, format = 'h:mm A'): string => {
  if (!utcString) return '';
  return dayjs.utc(utcString).tz(currentTimezone).format(format);
};

/**
 * Format for calendar/scheduler display (day and time)
 * @param utcString - ISO datetime string in UTC
 */
export const formatScheduleDateTime = (utcString: string | undefined | null): string => {
  if (!utcString) return '';
  return dayjs.utc(utcString).tz(currentTimezone).format('ddd, MMM D [at] h:mm A');
};

/**
 * Format for short display (time only for today, date+time for other days)
 * @param utcString - ISO datetime string in UTC
 */
export const formatSmartDateTime = (utcString: string | undefined | null): string => {
  if (!utcString) return '';
  const dt = dayjs.utc(utcString).tz(currentTimezone);
  const now = dayjs().tz(currentTimezone);

  if (dt.isSame(now, 'day')) {
    return `Today at ${dt.format('h:mm A')}`;
  } else if (dt.isSame(now.add(1, 'day'), 'day')) {
    return `Tomorrow at ${dt.format('h:mm A')}`;
  } else if (dt.isSame(now.subtract(1, 'day'), 'day')) {
    return `Yesterday at ${dt.format('h:mm A')}`;
  } else if (dt.isSame(now, 'year')) {
    return dt.format('MMM D [at] h:mm A');
  }
  return dt.format('MMM D, YYYY [at] h:mm A');
};

/**
 * Format relative time (e.g., "2 hours ago", "in 3 days")
 * @param utcString - ISO datetime string in UTC
 */
export const formatRelativeTime = (utcString: string | undefined | null): string => {
  if (!utcString) return '';
  return dayjs.utc(utcString).tz(currentTimezone).fromNow();
};

/**
 * Convert local datetime to UTC for sending to API
 * @param localDateTime - dayjs object or Date in user's local timezone
 * @returns ISO string in UTC
 */
export const toUtcString = (localDateTime: dayjs.Dayjs | Date): string => {
  if (localDateTime instanceof Date) {
    return dayjs(localDateTime).tz(currentTimezone, true).utc().toISOString();
  }
  return localDateTime.tz(currentTimezone, true).utc().toISOString();
};

/**
 * Parse UTC string and return dayjs in user's timezone
 * @param utcString - ISO datetime string in UTC
 * @returns dayjs object in user's timezone
 */
export const parseUtcToLocal = (utcString: string): dayjs.Dayjs => {
  return dayjs.utc(utcString).tz(currentTimezone);
};

/**
 * Check if a UTC datetime is today in user's timezone
 * @param utcString - ISO datetime string in UTC
 */
export const isToday = (utcString: string | undefined | null): boolean => {
  if (!utcString) return false;
  const dt = dayjs.utc(utcString).tz(currentTimezone);
  const now = dayjs().tz(currentTimezone);
  return dt.isSame(now, 'day');
};

/**
 * Check if a UTC datetime is in the past in user's timezone
 * @param utcString - ISO datetime string in UTC
 */
export const isPast = (utcString: string | undefined | null): boolean => {
  if (!utcString) return false;
  return dayjs.utc(utcString).isBefore(dayjs());
};

/**
 * Check if a UTC datetime is overdue (past and before today)
 * @param utcString - ISO datetime string in UTC
 */
export const isOverdue = (utcString: string | undefined | null): boolean => {
  if (!utcString) return false;
  const dt = dayjs.utc(utcString).tz(currentTimezone);
  const now = dayjs().tz(currentTimezone);
  return dt.isBefore(now.startOf('day'));
};

/**
 * Get a dayjs object for "now" in user's timezone
 */
export const now = (): dayjs.Dayjs => {
  return dayjs().tz(currentTimezone);
};

/**
 * Get today's date at start of day in user's timezone
 */
export const today = (): dayjs.Dayjs => {
  return dayjs().tz(currentTimezone).startOf('day');
};
