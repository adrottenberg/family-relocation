import { useEffect, useState } from 'react';
import { Modal, Descriptions, Tag, Space, Button, Spin, Typography, Divider } from 'antd';
import {
  PhoneOutlined,
  MailOutlined,
  MessageOutlined,
  FileTextOutlined,
  SettingOutlined,
  UserOutlined,
  ClockCircleOutlined,
  BellOutlined,
} from '@ant-design/icons';
import { Link } from 'react-router-dom';
import dayjs from 'dayjs';
import { activitiesApi, ActivityDto } from '../../api';

const { Text, Paragraph } = Typography;

interface ActivityDetailModalProps {
  open: boolean;
  activityId: string | null;
  onClose: () => void;
}

const getActivityIcon = (type: string) => {
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
      return <SettingOutlined />;
  }
};

const getActivityColor = (type: string) => {
  switch (type) {
    case 'PhoneCall':
      return 'green';
    case 'Email':
      return 'blue';
    case 'SMS':
      return 'purple';
    case 'Note':
      return 'orange';
    default:
      return 'default';
  }
};

const getOutcomeColor = (outcome: string) => {
  switch (outcome) {
    case 'Connected':
      return 'success';
    case 'Voicemail':
    case 'LeftMessage':
      return 'warning';
    case 'NoAnswer':
    case 'Busy':
      return 'default';
    default:
      return 'default';
  }
};

const ActivityDetailModal = ({ open, activityId, onClose }: ActivityDetailModalProps) => {
  const [activity, setActivity] = useState<ActivityDto | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (open && activityId) {
      setLoading(true);
      activitiesApi
        .getById(activityId)
        .then((data) => setActivity(data))
        .catch((err) => console.error('Failed to fetch activity:', err))
        .finally(() => setLoading(false));
    } else {
      setActivity(null);
    }
  }, [open, activityId]);

  return (
    <Modal
      title={
        <Space>
          {activity && getActivityIcon(activity.type)}
          Activity Details
        </Space>
      }
      open={open}
      onCancel={onClose}
      width={600}
      footer={<Button onClick={onClose}>Close</Button>}
    >
      {loading ? (
        <div style={{ textAlign: 'center', padding: 40 }}>
          <Spin />
        </div>
      ) : activity ? (
        <>
          <Descriptions column={1} bordered size="small">
            <Descriptions.Item label="Type">
              <Tag icon={getActivityIcon(activity.type)} color={getActivityColor(activity.type)}>
                {activity.type}
              </Tag>
              {activity.action && activity.action !== activity.type && (
                <Text type="secondary" style={{ marginLeft: 8 }}>
                  ({activity.action})
                </Text>
              )}
            </Descriptions.Item>

            <Descriptions.Item label="Date & Time">
              <Space>
                <ClockCircleOutlined />
                {dayjs(activity.timestamp).format('dddd, MMMM D, YYYY [at] h:mm A')}
              </Space>
            </Descriptions.Item>

            {activity.type === 'PhoneCall' && activity.durationMinutes && (
              <Descriptions.Item label="Duration">
                {activity.durationMinutes} minutes
              </Descriptions.Item>
            )}

            {activity.outcome && (
              <Descriptions.Item label="Outcome">
                <Tag color={getOutcomeColor(activity.outcome)}>{activity.outcome}</Tag>
              </Descriptions.Item>
            )}

            <Descriptions.Item label="Related To">
              <Space direction="vertical" size={0}>
                <Text type="secondary">{activity.entityType}</Text>
                {activity.entityType === 'Applicant' ? (
                  <Link to={`/applicants/${activity.entityId}`} onClick={onClose}>
                    <Space>
                      <UserOutlined />
                      {activity.entityDisplayName || 'View Applicant'}
                    </Space>
                  </Link>
                ) : (
                  <Text>{activity.entityId}</Text>
                )}
              </Space>
            </Descriptions.Item>

            {activity.userName && (
              <Descriptions.Item label="Logged By">
                {activity.userName}
              </Descriptions.Item>
            )}

            {activity.followUpReminderId && (
              <Descriptions.Item label="Follow-up Reminder">
                <Tag icon={<BellOutlined />} color="blue">
                  Reminder Created
                </Tag>
              </Descriptions.Item>
            )}
          </Descriptions>

          <Divider orientation="left">Description</Divider>
          <Paragraph
            style={{
              backgroundColor: '#fafafa',
              padding: 16,
              borderRadius: 4,
              whiteSpace: 'pre-wrap',
              minHeight: 80,
            }}
          >
            {activity.description}
          </Paragraph>
        </>
      ) : (
        <Text type="secondary">Activity not found</Text>
      )}
    </Modal>
  );
};

export default ActivityDetailModal;
