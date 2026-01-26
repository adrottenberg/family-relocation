import { Card, Tag, Button, Space, Typography, Dropdown, MenuProps } from 'antd';
import {
  CalendarOutlined,
  ClockCircleOutlined,
  HomeOutlined,
  UserOutlined,
  MoreOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ExclamationCircleOutlined,
} from '@ant-design/icons';
import { Link } from 'react-router-dom';
import type { ShowingListDto } from '../../api/types';
import { formatDate, formatTime, isToday, isPast } from '../../utils/datetime';

const { Text } = Typography;

interface ShowingCardProps {
  showing: ShowingListDto;
  onReschedule?: (id: string) => void;
  onComplete?: (id: string) => void;
  onCancel?: (id: string) => void;
  onNoShow?: (id: string) => void;
}

const statusColors: Record<string, string> = {
  Scheduled: 'blue',
  Completed: 'green',
  Cancelled: 'default',
  NoShow: 'red',
};

const statusLabels: Record<string, string> = {
  Scheduled: 'Scheduled',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  NoShow: 'No Show',
};

const ShowingCard = ({
  showing,
  onReschedule,
  onComplete,
  onCancel,
  onNoShow,
}: ShowingCardProps) => {
  const showingDateTime = showing.scheduledDateTime;
  const showingIsToday = isToday(showingDateTime);
  const showingIsPast = isPast(showingDateTime);

  const menuItems: MenuProps['items'] = [];

  if (showing.status === 'Scheduled') {
    menuItems.push(
      {
        key: 'complete',
        icon: <CheckCircleOutlined />,
        label: 'Mark Completed',
        onClick: () => onComplete?.(showing.id),
      },
      {
        key: 'noshow',
        icon: <ExclamationCircleOutlined />,
        label: 'Mark No Show',
        onClick: () => onNoShow?.(showing.id),
      },
      { type: 'divider' },
      {
        key: 'cancel',
        icon: <CloseCircleOutlined />,
        label: 'Cancel',
        danger: true,
        onClick: () => onCancel?.(showing.id),
      }
    );
  }

  return (
    <Card
      size="small"
      style={{
        marginBottom: 12,
        borderLeft: showingIsToday ? '4px solid #1890ff' : showingIsPast ? '4px solid #d9d9d9' : undefined,
      }}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div style={{ flex: 1 }}>
          {/* Date and Time */}
          <div style={{ marginBottom: 8 }}>
            <Space size="large">
              <span>
                <CalendarOutlined style={{ marginRight: 6, color: '#1890ff' }} />
                <Text strong>
                  {showingIsToday ? 'Today' : formatDate(showingDateTime, 'ddd, MMM D, YYYY')}
                </Text>
              </span>
              <span>
                <ClockCircleOutlined style={{ marginRight: 6, color: '#1890ff' }} />
                <Text>{formatTime(showingDateTime)}</Text>
              </span>
            </Space>
          </div>

          {/* Property Info */}
          <div style={{ marginBottom: 4 }}>
            <HomeOutlined style={{ marginRight: 6, color: '#666' }} />
            <Link to={`/listings/${showing.propertyId}`}>
              {showing.propertyStreet}, {showing.propertyCity}
            </Link>
          </div>

          {/* Applicant Info */}
          <div style={{ marginBottom: 8 }}>
            <UserOutlined style={{ marginRight: 6, color: '#666' }} />
            <Link to={`/applicants/${showing.applicantId}`}>
              {showing.applicantName}
            </Link>
          </div>

                  </div>

        {/* Status and Actions */}
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 8 }}>
          <Tag color={statusColors[showing.status] || 'default'}>
            {statusLabels[showing.status] || showing.status}
          </Tag>

          <Space>
            {showing.status === 'Scheduled' && onReschedule && (
              <Button size="small" onClick={() => onReschedule(showing.id)}>
                Reschedule
              </Button>
            )}
            {menuItems.length > 0 && (
              <Dropdown menu={{ items: menuItems }} trigger={['click']}>
                <Button size="small" icon={<MoreOutlined />} />
              </Dropdown>
            )}
          </Space>
        </div>
      </div>
    </Card>
  );
};

export default ShowingCard;
