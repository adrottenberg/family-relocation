import { Typography } from 'antd';
import dayjs, { Dayjs } from 'dayjs';
import type { ShowingListDto } from '../../../api/types';
import TimeSlot from './TimeSlot';

const { Text } = Typography;

interface DayScheduleViewProps {
  date: Dayjs;
  showings: ShowingListDto[];
  contextApplicantId?: string; // Highlight showings for this applicant
  contextPropertyId?: string; // Highlight showings for this property
  onReschedule?: (showingId: string) => void;
  onCancel?: (showingId: string) => void;
}

// Generate time slots from 8 AM to 8 PM in 30-minute intervals
const generateTimeSlots = (date: Dayjs): { id: string; time: string; timeKey: string }[] => {
  const slots: { id: string; time: string; timeKey: string }[] = [];
  const dateStr = date.format('YYYY-MM-DD');

  for (let hour = 8; hour <= 20; hour++) {
    for (const minute of [0, 30]) {
      // Skip 8:30 PM slot
      if (hour === 20 && minute === 30) continue;

      const timeStr = `${hour.toString().padStart(2, '0')}:${minute.toString().padStart(2, '0')}:00`;
      const displayHour = hour > 12 ? hour - 12 : hour === 0 ? 12 : hour;
      const ampm = hour >= 12 ? 'PM' : 'AM';
      const displayTime = `${displayHour}:${minute.toString().padStart(2, '0')} ${ampm}`;

      slots.push({
        id: `${dateStr}T${timeStr}`,
        time: displayTime,
        timeKey: timeStr, // Just the time portion for matching
      });
    }
  }

  return slots;
};

// Normalize time string to HH:MM:SS format, rounding to nearest 30-minute slot
const normalizeTimeToSlot = (time: string): string => {
  // Handle formats like "10:00:00", "10:00", "10:00:00.0000000"
  const parts = time.split(':');
  const hours = parseInt(parts[0] || '0', 10);
  const minutes = parseInt(parts[1] || '0', 10);

  // Round to nearest 30-minute slot
  const roundedMinutes = minutes < 15 ? 0 : minutes < 45 ? 30 : 0;
  const roundedHours = minutes >= 45 ? hours + 1 : hours;

  return `${roundedHours.toString().padStart(2, '0')}:${roundedMinutes.toString().padStart(2, '0')}:00`;
};

const DayScheduleView = ({
  date,
  showings,
  contextApplicantId,
  contextPropertyId,
  onReschedule,
  onCancel,
}: DayScheduleViewProps) => {
  const timeSlots = generateTimeSlots(date);
  const today = dayjs().startOf('day');
  const isToday = date.isSame(today, 'day');

  // Create a map of time slot to showings (multiple showings can map to same slot)
  const showingsByTime: Record<string, ShowingListDto[]> = {};
  showings.forEach((showing) => {
    const slotTime = normalizeTimeToSlot(showing.scheduledTime);
    if (!showingsByTime[slotTime]) {
      showingsByTime[slotTime] = [];
    }
    showingsByTime[slotTime].push(showing);
  });

  // Check if a showing is for the current context
  const isCurrentContext = (showing: ShowingListDto): boolean => {
    if (contextApplicantId && showing.applicantId === contextApplicantId) return true;
    if (contextPropertyId && showing.propertyId === contextPropertyId) return true;
    return false;
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      {/* Day header */}
      <div
        style={{
          padding: '12px 16px',
          borderBottom: '1px solid #f0f0f0',
          background: '#fafafa',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}
      >
        <Text strong>
          {isToday ? 'Today, ' : ''}
          {date.format('dddd, MMMM D')}
        </Text>
        <Text type="secondary" style={{ fontSize: 12 }}>
          {showings.length} showing{showings.length !== 1 ? 's' : ''}
        </Text>
      </div>

      {/* Time slots */}
      <div
        style={{
          flex: 1,
          overflow: 'auto',
        }}
      >
        {timeSlots.map((slot) => {
          const slotShowings = showingsByTime[slot.timeKey] || [];
          // If no showings, render empty slot
          if (slotShowings.length === 0) {
            return (
              <TimeSlot
                key={slot.id}
                id={slot.id}
                time={slot.time}
                onReschedule={onReschedule}
                onCancel={onCancel}
              />
            );
          }
          // Render a slot for each showing in this time range
          return slotShowings.map((showing, index) => (
            <TimeSlot
              key={`${slot.id}-${showing.id}`}
              id={slot.id}
              time={index === 0 ? slot.time : ''} // Only show time on first
              showing={showing}
              isCurrentContext={isCurrentContext(showing)}
              onReschedule={onReschedule}
              onCancel={onCancel}
            />
          ));
        })}
      </div>
    </div>
  );
};

export default DayScheduleView;
