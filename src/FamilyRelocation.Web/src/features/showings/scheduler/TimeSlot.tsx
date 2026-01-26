import { useDroppable, useDraggable } from '@dnd-kit/core';
import { CSS } from '@dnd-kit/utilities';
import { Typography, Dropdown, Button } from 'antd';
import { MoreOutlined, EditOutlined, CloseCircleOutlined } from '@ant-design/icons';
import type { ShowingListDto } from '../../../api/types';
import type { MenuProps } from 'antd';

const { Text } = Typography;

interface TimeSlotProps {
  id: string; // ISO datetime string, e.g., "2026-01-27T10:00:00"
  time: string; // Display time, e.g., "10:00 AM"
  showing?: ShowingListDto;
  isCurrentContext?: boolean; // Is this showing for the current applicant/property?
  onReschedule?: (showingId: string) => void;
  onCancel?: (showingId: string) => void;
}

// Separate component for draggable showing content
const DraggableShowingContent = ({
  showing,
  isCurrentContext,
  onReschedule,
  onCancel,
}: {
  showing: ShowingListDto;
  isCurrentContext: boolean;
  onReschedule?: (showingId: string) => void;
  onCancel?: (showingId: string) => void;
}) => {
  const isScheduled = showing.status === 'Scheduled';

  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: `showing-${showing.id}`,
    data: {
      type: 'showing',
      showing,
    },
  });

  const style = {
    transform: CSS.Translate.toString(transform),
    opacity: isDragging ? 0.5 : 1,
    cursor: 'grab',
  };

  const menuItems: MenuProps['items'] = isScheduled
    ? [
        {
          key: 'reschedule',
          icon: <EditOutlined />,
          label: 'Reschedule',
          onClick: () => onReschedule?.(showing.id),
        },
        {
          key: 'cancel',
          icon: <CloseCircleOutlined />,
          label: 'Cancel Showing',
          danger: true,
          onClick: () => onCancel?.(showing.id),
        },
      ]
    : [];

  return (
    <div
      ref={setNodeRef}
      style={{
        ...style,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        flex: 1,
      }}
      {...listeners}
      {...attributes}
    >
      <div style={{ minWidth: 0, flex: 1 }}>
        <Text
          style={{
            fontSize: 12,
            fontWeight: 500,
            color: isCurrentContext ? '#1890ff' : '#666',
          }}
          ellipsis
        >
          {showing.propertyStreet}
        </Text>
        <Text
          type="secondary"
          style={{ fontSize: 11, display: 'block' }}
          ellipsis
        >
          {showing.applicantName}
        </Text>
      </div>
      {isScheduled && (onReschedule || onCancel) && (
        <Dropdown menu={{ items: menuItems }} trigger={['click']}>
          <Button
            type="text"
            size="small"
            icon={<MoreOutlined />}
            onClick={(e) => e.stopPropagation()}
            onPointerDown={(e) => e.stopPropagation()} // Prevent drag when clicking menu
            style={{ marginLeft: 4 }}
          />
        </Dropdown>
      )}
    </div>
  );
};

const TimeSlot = ({
  id,
  time,
  showing,
  isCurrentContext = false,
  onReschedule,
  onCancel,
}: TimeSlotProps) => {
  const { isOver, setNodeRef } = useDroppable({
    id,
    disabled: false, // Allow dropping even if there's a showing (for stacking)
  });

  const hasShowing = !!showing;

  return (
    <div
      ref={setNodeRef}
      style={{
        display: 'flex',
        alignItems: 'stretch',
        minHeight: 40,
        borderBottom: '1px solid #f0f0f0',
      }}
    >
      {/* Time label */}
      <div
        style={{
          width: 70,
          padding: '8px 8px 8px 12px',
          fontSize: 12,
          color: '#666',
          borderRight: '1px solid #f0f0f0',
          flexShrink: 0,
          display: 'flex',
          alignItems: 'center',
        }}
      >
        {time}
      </div>

      {/* Slot content */}
      <div
        style={{
          flex: 1,
          padding: '4px 8px',
          background: isOver
            ? '#e6f7ff'
            : hasShowing
            ? isCurrentContext
              ? '#f0f5ff'
              : '#fafafa'
            : 'transparent',
          borderLeft: isOver ? '3px solid #1890ff' : hasShowing && isCurrentContext ? '3px solid #1890ff' : '3px solid transparent',
          transition: 'all 0.2s',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
        }}
      >
        {hasShowing ? (
          <DraggableShowingContent
            showing={showing}
            isCurrentContext={isCurrentContext}
            onReschedule={onReschedule}
            onCancel={onCancel}
          />
        ) : isOver ? (
          <Text type="secondary" style={{ fontSize: 12 }}>
            Drop here to schedule
          </Text>
        ) : null}
      </div>
    </div>
  );
};

export default TimeSlot;
