import { useEffect, useState } from 'react';
import { Typography, Card, Row, Col, Spin, Statistic, List, Space, Tag } from 'antd';
import {
  UserOutlined,
  HomeOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  SearchOutlined,
  FileTextOutlined,
} from '@ant-design/icons';
import { dashboardApi, activitiesApi, DashboardStatsDto, ActivityDto } from '../../api';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';

dayjs.extend(relativeTime);

const { Title, Text } = Typography;

/**
 * Dashboard page showing statistics and recent activity
 */
const DashboardPage = () => {
  const [stats, setStats] = useState<DashboardStatsDto | null>(null);
  const [activities, setActivities] = useState<ActivityDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [statsData, activitiesData] = await Promise.all([
          dashboardApi.getStats(),
          activitiesApi.getRecent(10),
        ]);
        setStats(statsData);
        setActivities(activitiesData);
      } catch (error) {
        console.error('Failed to fetch dashboard data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: '100px' }}>
        <Spin size="large" />
      </div>
    );
  }

  const applicantStats = stats?.applicants;
  const propertyStats = stats?.properties;

  return (
    <div>
      <Title level={2}>Dashboard</Title>

      {/* Applicant Stats */}
      <Title level={4} style={{ marginTop: 24 }}>
        <UserOutlined /> Applicants
      </Title>
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Applicants"
              value={applicantStats?.total ?? 0}
              prefix={<UserOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Pending Review"
              value={applicantStats?.byStage?.submitted ?? 0}
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Actively Searching"
              value={applicantStats?.byStage?.searching ?? 0}
              prefix={<SearchOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Closed / Moved In"
              value={applicantStats?.byStage?.closed ?? 0}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Property Stats */}
      <Title level={4} style={{ marginTop: 32 }}>
        <HomeOutlined /> Properties
      </Title>
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Properties"
              value={propertyStats?.total ?? 0}
              prefix={<HomeOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Active Listings"
              value={propertyStats?.byStatus?.active ?? 0}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Under Contract"
              value={propertyStats?.byStatus?.undercontract ?? 0}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Sold"
              value={propertyStats?.byStatus?.sold ?? 0}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Recent Activity */}
      <Title level={4} style={{ marginTop: 32 }}>
        <FileTextOutlined /> Recent Activity
      </Title>
      <Card>
        <List
          dataSource={activities}
          renderItem={(activity) => (
            <List.Item>
              <List.Item.Meta
                title={
                  <Space>
                    <Tag color={getEntityColor(activity.entityType)}>{activity.entityType}</Tag>
                    <Text strong>{activity.action}</Text>
                  </Space>
                }
                description={
                  <Space direction="vertical" size={0}>
                    <Text>{activity.description}</Text>
                    <Text type="secondary" style={{ fontSize: 12 }}>
                      {activity.userName && `by ${activity.userName} - `}
                      {dayjs(activity.timestamp).fromNow()}
                    </Text>
                  </Space>
                }
              />
            </List.Item>
          )}
          locale={{ emptyText: 'No recent activity' }}
        />
      </Card>
    </div>
  );
};

const getEntityColor = (entityType: string): string => {
  switch (entityType.toLowerCase()) {
    case 'applicant':
      return 'blue';
    case 'property':
      return 'green';
    case 'housingsearch':
      return 'purple';
    default:
      return 'default';
  }
};

export default DashboardPage;
