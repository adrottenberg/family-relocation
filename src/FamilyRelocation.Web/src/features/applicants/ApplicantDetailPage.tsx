import { useParams, useNavigate, Link } from 'react-router-dom';
import {
  Card,
  Tabs,
  Typography,
  Tag,
  Button,
  Descriptions,
  Space,
  Spin,
  Empty,
  Timeline,
  Table,
} from 'antd';
import {
  ArrowLeftOutlined,
  EditOutlined,
  PhoneOutlined,
  MailOutlined,
  HomeOutlined,
  UserOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { applicantsApi } from '../../api';
import type { ApplicantDto, ChildDto, AuditLogDto, HusbandInfoDto, SpouseInfoDto } from '../../api/types';
import { colors, statusTagStyles, stageTagStyles } from '../../theme/antd-theme';
import './ApplicantDetailPage.css';

const { Title, Text } = Typography;

const ApplicantDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: applicant, isLoading, error } = useQuery({
    queryKey: ['applicant', id],
    queryFn: () => applicantsApi.getById(id!),
    enabled: !!id,
  });

  const { data: auditLogs } = useQuery({
    queryKey: ['applicant-audit', id],
    queryFn: () => applicantsApi.getAuditLogs(id!, { page: 1, pageSize: 20 }),
    enabled: !!id,
  });

  if (isLoading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
      </div>
    );
  }

  if (error || !applicant) {
    return (
      <Card>
        <Empty description="Applicant not found" />
        <div style={{ textAlign: 'center', marginTop: 16 }}>
          <Button onClick={() => navigate('/applicants')}>Back to Applicants</Button>
        </div>
      </Card>
    );
  }

  const getStatusTagStyle = (decision: string) => {
    const key = decision.toLowerCase() as keyof typeof statusTagStyles;
    return statusTagStyles[key] || { backgroundColor: colors.neutral[100], color: colors.neutral[600] };
  };

  const getStageTagStyle = (stageName: string) => {
    const stageMap: Record<string, keyof typeof stageTagStyles> = {
      'Submitted': 'submitted',
      'HouseHunting': 'houseHunting',
      'UnderContract': 'underContract',
      'Closed': 'closed',
    };
    const key = stageMap[stageName];
    return key ? stageTagStyles[key] : { backgroundColor: colors.neutral[100], color: colors.neutral[600] };
  };

  const formatStageName = (stage: string) => {
    const names: Record<string, string> = {
      'Submitted': 'Submitted',
      'BoardApproved': 'Board Approved',
      'HouseHunting': 'House Hunting',
      'UnderContract': 'Under Contract',
      'Closed': 'Closed',
      'Paused': 'Paused',
      'Rejected': 'Rejected',
      'MovedIn': 'Moved In',
    };
    return names[stage] || stage;
  };

  const hs = applicant.housingSearch;
  const boardDecision = applicant.boardReview?.decision || 'Pending';
  const stage = hs?.stage || 'N/A';

  const tabItems = [
    {
      key: 'overview',
      label: 'Overview',
      children: <OverviewTab applicant={applicant} />,
    },
    {
      key: 'housing',
      label: 'Housing Search',
      children: <HousingSearchTab applicant={applicant} />,
    },
    {
      key: 'children',
      label: `Children (${applicant.children?.length || 0})`,
      children: <ChildrenTab children={applicant.children} />,
    },
    {
      key: 'activity',
      label: 'Activity',
      children: <ActivityTab auditLogs={auditLogs?.items || []} />,
    },
  ];

  return (
    <div className="applicant-detail-page">
      {/* Back link */}
      <Link to="/applicants" className="back-link">
        <ArrowLeftOutlined /> Back to Applicants
      </Link>

      {/* Header Card */}
      <Card className="header-card">
        <div className="header-content">
          <div className="header-left">
            <div className="avatar">
              {applicant.husband.lastName.charAt(0)}
            </div>
            <div className="header-info">
              <Title level={3} style={{ margin: 0 }}>
                {applicant.husband.lastName} Family
              </Title>
              <Text type="secondary">
                {applicant.husband.firstName}
                {applicant.wife && ` & ${applicant.wife.firstName}`}
              </Text>
              <div className="header-tags">
                <Tag style={getStatusTagStyle(boardDecision)}>{boardDecision}</Tag>
                <Tag style={getStageTagStyle(stage)}>{formatStageName(stage)}</Tag>
              </div>
            </div>
          </div>
          <div className="header-actions">
            <Button icon={<EditOutlined />}>Edit</Button>
          </div>
        </div>
      </Card>

      {/* Tabs */}
      <Card className="tabs-card">
        <Tabs items={tabItems} />
      </Card>
    </div>
  );
};

// Helper to get primary phone
const getPrimaryPhone = (phoneNumbers?: { number: string; isPrimary: boolean }[]) => {
  if (!phoneNumbers || phoneNumbers.length === 0) return 'N/A';
  const primary = phoneNumbers.find(p => p.isPrimary);
  return primary?.number || phoneNumbers[0]?.number || 'N/A';
};

// Overview Tab
const OverviewTab = ({ applicant }: { applicant: ApplicantDto }) => {
  const { husband, wife, address } = applicant;

  return (
    <div className="tab-content">
      <div className="info-grid">
        {/* Husband Info */}
        <Card title="Husband" size="small" className="info-card">
          <Descriptions column={1} size="small">
            <Descriptions.Item label={<><UserOutlined /> Name</>}>
              {husband.firstName} {husband.lastName}
            </Descriptions.Item>
            <Descriptions.Item label={<><MailOutlined /> Email</>}>
              {husband.email || 'N/A'}
            </Descriptions.Item>
            <Descriptions.Item label={<><PhoneOutlined /> Phone</>}>
              {getPrimaryPhone(husband.phoneNumbers)}
            </Descriptions.Item>
            <Descriptions.Item label="Occupation">
              {husband.occupation || 'N/A'}
            </Descriptions.Item>
          </Descriptions>
        </Card>

        {/* Wife Info */}
        {wife && (
          <Card title="Wife" size="small" className="info-card">
            <Descriptions column={1} size="small">
              <Descriptions.Item label={<><UserOutlined /> Name</>}>
                {wife.firstName} {wife.maidenName ? `(${wife.maidenName})` : ''}
              </Descriptions.Item>
              <Descriptions.Item label={<><MailOutlined /> Email</>}>
                {wife.email || 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label={<><PhoneOutlined /> Phone</>}>
                {getPrimaryPhone(wife.phoneNumbers)}
              </Descriptions.Item>
              <Descriptions.Item label="Occupation">
                {wife.occupation || 'N/A'}
              </Descriptions.Item>
            </Descriptions>
          </Card>
        )}

        {/* Address */}
        {address && (
          <Card title="Current Address" size="small" className="info-card">
            <Descriptions column={1} size="small">
              <Descriptions.Item label={<><HomeOutlined /> Street</>}>
                {address.street}
              </Descriptions.Item>
              <Descriptions.Item label="City">
                {address.city}, {address.state} {address.zipCode}
              </Descriptions.Item>
            </Descriptions>
          </Card>
        )}

        {/* Community */}
        <Card title="Community" size="small" className="info-card">
          <Descriptions column={1} size="small">
            <Descriptions.Item label="Current Kehila">
              {applicant.currentKehila || 'N/A'}
            </Descriptions.Item>
            <Descriptions.Item label="Shabbos Shul">
              {applicant.shabbosShul || 'N/A'}
            </Descriptions.Item>
          </Descriptions>
        </Card>
      </div>
    </div>
  );
};

// Housing Search Tab
const HousingSearchTab = ({ applicant }: { applicant: ApplicantDto }) => {
  const hs = applicant.housingSearch;

  if (!hs) {
    return <Empty description="No housing search data" />;
  }

  const prefs = hs.preferences;

  return (
    <div className="tab-content">
      <div className="info-grid">
        {/* Status */}
        <Card title="Status" size="small" className="info-card">
          <Descriptions column={1} size="small">
            <Descriptions.Item label="Stage">
              {hs.stage}
            </Descriptions.Item>
            <Descriptions.Item label="Stage Changed">
              {new Date(hs.stageChangedDate).toLocaleDateString()}
            </Descriptions.Item>
            <Descriptions.Item label="Failed Contracts">
              {hs.failedContractCount}
            </Descriptions.Item>
          </Descriptions>
        </Card>

        {/* Agreements */}
        <Card title="Agreements" size="small" className="info-card">
          <Descriptions column={1} size="small">
            <Descriptions.Item label="Broker Agreement">
              {hs.brokerAgreementSigned ? (
                <Tag color="success">Signed</Tag>
              ) : (
                <Tag color="warning">Not Signed</Tag>
              )}
            </Descriptions.Item>
            <Descriptions.Item label="Community Takanos">
              {hs.communityTakanosSigned ? (
                <Tag color="success">Signed</Tag>
              ) : (
                <Tag color="warning">Not Signed</Tag>
              )}
            </Descriptions.Item>
          </Descriptions>
        </Card>

        {/* Preferences */}
        {prefs && (
          <Card title="Preferences" size="small" className="info-card">
            <Descriptions column={1} size="small">
              <Descriptions.Item label="Budget">
                {prefs.budgetAmount
                  ? `$${prefs.budgetAmount.toLocaleString()}`
                  : 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label="Bedrooms">
                {prefs.minBedrooms ? `${prefs.minBedrooms}+` : 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label="Bathrooms">
                {prefs.minBathrooms ? `${prefs.minBathrooms}+` : 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label="Move Timeline">
                {prefs.moveTimeline || 'N/A'}
              </Descriptions.Item>
            </Descriptions>
          </Card>
        )}

        {/* Required Features */}
        {prefs?.requiredFeatures && prefs.requiredFeatures.length > 0 && (
          <Card title="Required Features" size="small" className="info-card">
            <Space wrap>
              {prefs.requiredFeatures.map((feature) => (
                <Tag key={feature}>{feature}</Tag>
              ))}
            </Space>
          </Card>
        )}

        {/* Current Contract */}
        {hs.currentContract && (
          <Card title="Current Contract" size="small" className="info-card">
            <Descriptions column={1} size="small">
              <Descriptions.Item label="Contract Price">
                ${hs.currentContract.price.toLocaleString()}
              </Descriptions.Item>
              <Descriptions.Item label="Contract Date">
                {new Date(hs.currentContract.contractDate).toLocaleDateString()}
              </Descriptions.Item>
              {hs.currentContract.expectedClosingDate && (
                <Descriptions.Item label="Expected Closing">
                  {new Date(hs.currentContract.expectedClosingDate).toLocaleDateString()}
                </Descriptions.Item>
              )}
            </Descriptions>
          </Card>
        )}
      </div>
    </div>
  );
};

// Children Tab
const ChildrenTab = ({ children }: { children?: ChildDto[] }) => {
  if (!children || children.length === 0) {
    return <Empty description="No children listed" />;
  }

  const columns = [
    { title: 'Name', dataIndex: 'name', key: 'name' },
    { title: 'Age', dataIndex: 'age', key: 'age', width: 80 },
    { title: 'Gender', dataIndex: 'gender', key: 'gender', width: 100 },
    { title: 'School', dataIndex: 'school', key: 'school', render: (v: string) => v || 'N/A' },
  ];

  return (
    <div className="tab-content">
      <Table
        columns={columns}
        dataSource={children}
        rowKey="name"
        pagination={false}
        size="small"
      />
    </div>
  );
};

// Activity Tab
const ActivityTab = ({ auditLogs }: { auditLogs: AuditLogDto[] }) => {
  if (!auditLogs || auditLogs.length === 0) {
    return <Empty description="No activity recorded" />;
  }

  const formatAction = (action: string) => {
    const actions: Record<string, string> = {
      'Added': 'Created',
      'Modified': 'Updated',
      'Deleted': 'Deleted',
    };
    return actions[action] || action;
  };

  const getActionColor = (action: string) => {
    const colorMap: Record<string, string> = {
      'Added': 'green',
      'Modified': 'blue',
      'Deleted': 'red',
    };
    return colorMap[action] || 'gray';
  };

  return (
    <div className="tab-content">
      <Timeline
        items={auditLogs.map((log) => ({
          color: getActionColor(log.action),
          children: (
            <div className="timeline-item">
              <div className="timeline-header">
                <Text strong>{formatAction(log.action)} {log.entityType}</Text>
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {new Date(log.timestamp).toLocaleString()}
                </Text>
              </div>
              {log.userName && (
                <Text type="secondary" style={{ fontSize: 13 }}>
                  by {log.userName}
                </Text>
              )}
            </div>
          ),
        }))}
      />
    </div>
  );
};

export default ApplicantDetailPage;
