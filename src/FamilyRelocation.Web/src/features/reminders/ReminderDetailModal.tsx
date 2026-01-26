import { useEffect, useState } from 'react';
import { Modal, Descriptions, Tag, Space, Button, Spin, Typography, Divider } from 'antd';
import {
  CheckOutlined,
  ClockCircleOutlined,
  CloseOutlined,
  ReloadOutlined,
  UserOutlined,
  CalendarOutlined,
  BellOutlined,
  PhoneOutlined,
  MailOutlined,
  MessageOutlined,
  FileTextOutlined,
} from '@ant-design/icons';
import { Link } from 'react-router-dom';
import { remindersApi, ReminderDto, ReminderPriority, ReminderStatus } from '../../api';
import { formatDateTime } from '../../utils/datetime';
import { ActivityDetailModal } from '../activities';

const { Text, Paragraph } = Typography;

const getActivityIcon = (type?: string) => {
  switch (type) {
    case 'PhoneCall':
      return <PhoneOutlined />;
    case 'Email':
      return <MailOutlined />;
    case 'SMS':
      return <MessageOutlined />;
    case 'Note':
      return <FileTextOutlined />;
    default:
      return null;
  }
};

interface ReminderDetailModalProps {
  open: boolean;
  reminderId: string | null;
  onClose: () => void;
  onComplete: (id: string) => void;
  onSnooze: (id: string) => void;
  onDismiss: (id: string) => void;
  onReopen: (id: string) => void;
}

const priorityColors: Record<ReminderPriority, string> = {
  Urgent: 'red',
  High: 'orange',
  Normal: 'blue',
  Low: 'default',
};

const statusColors: Record<ReminderStatus, string> = {
  Open: 'processing',
  Completed: 'success',
  Snoozed: 'warning',
  Dismissed: 'default',
};

const ReminderDetailModal = ({
  open,
  reminderId,
  onClose,
  onComplete,
  onSnooze,
  onDismiss,
  onReopen,
}: ReminderDetailModalProps) => {
  const [reminder, setReminder] = useState<ReminderDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [activityModalOpen, setActivityModalOpen] = useState(false);

  useEffect(() => {
    if (open && reminderId) {
      setLoading(true);
      remindersApi
        .getById(reminderId)
        .then((data) => setReminder(data))
        .catch((err) => console.error('Failed to fetch reminder:', err))
        .finally(() => setLoading(false));
    } else {
      setReminder(null);
    }
  }, [open, reminderId]);

  const getEntityLink = () => {
    if (!reminder) return null;

    if (reminder.entityType === 'Applicant') {
      return (
        <Link to={`/applicants/${reminder.entityId}`}>
          <Space>
            <UserOutlined />
            {reminder.entityDisplayName || 'View Applicant'}
          </Space>
        </Link>
      );
    }

    // For other entity types, just show the name
    return reminder.entityDisplayName || `${reminder.entityType} ${reminder.entityId.substring(0, 8)}...`;
  };

  const isActive = reminder?.status === 'Open' || reminder?.status === 'Snoozed';

  return (
    <Modal
      title={
        <Space>
          <BellOutlined />
          Reminder Details
        </Space>
      }
      open={open}
      onCancel={onClose}
      width={600}
      footer={
        reminder ? (
          <Space>
            {isActive && (
              <>
                <Button
                  type="primary"
                  icon={<CheckOutlined />}
                  onClick={() => {
                    onComplete(reminder.id);
                    onClose();
                  }}
                >
                  Complete
                </Button>
                <Button
                  icon={<ClockCircleOutlined />}
                  onClick={() => {
                    onSnooze(reminder.id);
                    onClose();
                  }}
                >
                  Snooze
                </Button>
                <Button
                  danger
                  icon={<CloseOutlined />}
                  onClick={() => {
                    onDismiss(reminder.id);
                    onClose();
                  }}
                >
                  Dismiss
                </Button>
              </>
            )}
            {(reminder.status === 'Completed' || reminder.status === 'Dismissed') && (
              <Button
                icon={<ReloadOutlined />}
                onClick={() => {
                  onReopen(reminder.id);
                  onClose();
                }}
              >
                Reopen
              </Button>
            )}
            <Button onClick={onClose}>Close</Button>
          </Space>
        ) : null
      }
    >
      {loading ? (
        <div style={{ textAlign: 'center', padding: 40 }}>
          <Spin />
        </div>
      ) : reminder ? (
        <>
          <Descriptions column={1} bordered size="small">
            <Descriptions.Item label="Title">
              <Text strong style={{ fontSize: 16 }}>{reminder.title}</Text>
            </Descriptions.Item>

            <Descriptions.Item label="Status">
              <Tag color={statusColors[reminder.status]}>{reminder.status}</Tag>
              {reminder.snoozeCount > 0 && (
                <Tag style={{ marginLeft: 8 }}>Snoozed {reminder.snoozeCount}x</Tag>
              )}
            </Descriptions.Item>

            <Descriptions.Item label="Priority">
              <Tag color={priorityColors[reminder.priority]}>{reminder.priority}</Tag>
            </Descriptions.Item>

            <Descriptions.Item label="Due Date">
              <Space>
                <CalendarOutlined />
                {formatDateTime(reminder.dueDateTime, 'dddd, MMMM D, YYYY [at] h:mm A')}
              </Space>
            </Descriptions.Item>

            {reminder.snoozedUntil && (
              <Descriptions.Item label="Snoozed Until">
                {formatDateTime(reminder.snoozedUntil, 'dddd, MMMM D, YYYY [at] h:mm A')}
              </Descriptions.Item>
            )}

            <Descriptions.Item label="Related To">
              <Space direction="vertical" size={0}>
                <Text type="secondary">{reminder.entityType}</Text>
                {getEntityLink()}
              </Space>
            </Descriptions.Item>

            {reminder.assignedToUserName && (
              <Descriptions.Item label="Assigned To">
                {reminder.assignedToUserName}
              </Descriptions.Item>
            )}

            {reminder.sourceActivityId && (
              <Descriptions.Item label="Created From">
                <Button
                  type="link"
                  style={{ padding: 0, height: 'auto' }}
                  onClick={() => setActivityModalOpen(true)}
                >
                  <Space>
                    {getActivityIcon(reminder.sourceActivityType)}
                    <Tag color="purple">{reminder.sourceActivityType}</Tag>
                    {reminder.sourceActivityTimestamp && (
                      <Text type="secondary">
                        {formatDateTime(reminder.sourceActivityTimestamp)}
                      </Text>
                    )}
                  </Space>
                </Button>
              </Descriptions.Item>
            )}

            <Descriptions.Item label="Created">
              <Space direction="vertical" size={0}>
                <Text>{formatDateTime(reminder.createdAt)}</Text>
                {reminder.createdByName && (
                  <Text type="secondary">by {reminder.createdByName}</Text>
                )}
              </Space>
            </Descriptions.Item>

            {reminder.completedAt && (
              <Descriptions.Item label="Completed">
                <Text>{formatDateTime(reminder.completedAt)}</Text>
              </Descriptions.Item>
            )}
          </Descriptions>

          {reminder.notes && (
            <>
              <Divider orientation="left">Notes</Divider>
              <Paragraph
                style={{
                  backgroundColor: '#fafafa',
                  padding: 16,
                  borderRadius: 4,
                  whiteSpace: 'pre-wrap',
                }}
              >
                {reminder.notes}
              </Paragraph>
            </>
          )}
        </>
      ) : (
        <Text type="secondary">Reminder not found</Text>
      )}

      {/* Activity Detail Modal */}
      <ActivityDetailModal
        open={activityModalOpen}
        activityId={reminder?.sourceActivityId || null}
        onClose={() => setActivityModalOpen(false)}
      />
    </Modal>
  );
};

export default ReminderDetailModal;
