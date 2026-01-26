import { useState } from 'react';
import { Button, Typography, Badge } from 'antd';
import { LeftOutlined, RightOutlined } from '@ant-design/icons';
import dayjs, { Dayjs } from 'dayjs';

const { Text } = Typography;

interface SchedulerCalendarProps {
  selectedDate: Dayjs;
  onDateSelect: (date: Dayjs) => void;
  showingCounts: Record<string, number>; // date string (YYYY-MM-DD) → count
}

const WEEKDAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

const SchedulerCalendar = ({
  selectedDate,
  onDateSelect,
  showingCounts,
}: SchedulerCalendarProps) => {
  const [viewMonth, setViewMonth] = useState(selectedDate.startOf('month'));

  const handlePrevMonth = () => {
    setViewMonth(viewMonth.subtract(1, 'month'));
  };

  const handleNextMonth = () => {
    setViewMonth(viewMonth.add(1, 'month'));
  };

  // Generate calendar days
  const generateCalendarDays = () => {
    const startOfMonth = viewMonth.startOf('month');
    const endOfMonth = viewMonth.endOf('month');
    const startDay = startOfMonth.day(); // 0-6 (Sun-Sat)
    const daysInMonth = endOfMonth.date();

    const days: (Dayjs | null)[] = [];

    // Add empty slots for days before the first of the month
    for (let i = 0; i < startDay; i++) {
      days.push(null);
    }

    // Add days of the month
    for (let i = 1; i <= daysInMonth; i++) {
      days.push(viewMonth.date(i));
    }

    return days;
  };

  const days = generateCalendarDays();
  const today = dayjs().startOf('day');

  return (
    <div style={{ padding: '12px 16px' }}>
      {/* Month navigation */}
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: 12,
        }}
      >
        <Button
          type="text"
          icon={<LeftOutlined />}
          onClick={handlePrevMonth}
          size="small"
        />
        <Text strong>{viewMonth.format('MMMM YYYY')}</Text>
        <Button
          type="text"
          icon={<RightOutlined />}
          onClick={handleNextMonth}
          size="small"
        />
      </div>

      {/* Weekday headers */}
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(7, 1fr)',
          gap: 2,
          marginBottom: 4,
        }}
      >
        {WEEKDAYS.map((day) => (
          <div
            key={day}
            style={{
              textAlign: 'center',
              padding: '4px 0',
              fontSize: 11,
              color: '#999',
              fontWeight: 500,
            }}
          >
            {day}
          </div>
        ))}
      </div>

      {/* Calendar grid */}
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(7, 1fr)',
          gap: 2,
        }}
      >
        {days.map((day, index) => {
          if (!day) {
            return <div key={`empty-${index}`} style={{ padding: 4 }} />;
          }

          const dateStr = day.format('YYYY-MM-DD');
          const count = showingCounts[dateStr] || 0;
          const isSelected = day.isSame(selectedDate, 'day');
          const isToday = day.isSame(today, 'day');
          const isPast = day.isBefore(today, 'day');

          return (
            <div
              key={dateStr}
              onClick={() => !isPast && onDateSelect(day)}
              style={{
                padding: 4,
                textAlign: 'center',
                borderRadius: 4,
                cursor: isPast ? 'not-allowed' : 'pointer',
                background: isSelected ? '#1890ff' : isToday ? '#e6f7ff' : 'transparent',
                color: isPast ? '#ccc' : isSelected ? '#fff' : '#333',
                position: 'relative',
                transition: 'background 0.2s',
              }}
              onMouseEnter={(e) => {
                if (!isPast && !isSelected) {
                  e.currentTarget.style.background = '#f0f0f0';
                }
              }}
              onMouseLeave={(e) => {
                if (!isPast && !isSelected) {
                  e.currentTarget.style.background = isToday ? '#e6f7ff' : 'transparent';
                }
              }}
            >
              <div style={{ fontSize: 13, fontWeight: isToday ? 600 : 400 }}>
                {day.date()}
              </div>
              {count > 0 && (
                <Badge
                  count={count}
                  size="small"
                  style={{
                    position: 'absolute',
                    top: 2,
                    right: 2,
                    fontSize: 10,
                    minWidth: 14,
                    height: 14,
                    lineHeight: '14px',
                    backgroundColor: isSelected ? '#fff' : '#1890ff',
                    color: isSelected ? '#1890ff' : '#fff',
                  }}
                />
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default SchedulerCalendar;
