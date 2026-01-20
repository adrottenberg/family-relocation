import { useState } from 'react';
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
  PrinterOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { applicantsApi, documentsApi } from '../../api';
import type { ApplicantDto, ChildDto, AuditLogDto, ApplicantDocumentDto } from '../../api/types';
import { colors, statusTagStyles, stageTagStyles } from '../../theme/antd-theme';
import { useAuthStore } from '../../store/authStore';
import BoardReviewSection from './BoardReviewSection';
import SetBoardDecisionModal from './SetBoardDecisionModal';
import EditApplicantDrawer from './EditApplicantDrawer';
import DocumentUploadModal from './DocumentUploadModal';
import './ApplicantDetailPage.css';

const { Title, Text } = Typography;

const ApplicantDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const canApproveBoardDecisions = useAuthStore((state) => state.canApproveBoardDecisions);
  const [boardDecisionModalOpen, setBoardDecisionModalOpen] = useState(false);
  const [editDrawerOpen, setEditDrawerOpen] = useState(false);
  const [documentUploadModalOpen, setDocumentUploadModalOpen] = useState(false);

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

  const handlePrint = () => {
    const printWindow = window.open('', '_blank');
    if (!printWindow) return;

    const { husband, wife, address, children } = applicant;
    const prefs = hs?.preferences;

    const childrenHtml = children && children.length > 0
      ? `
        <h3>Children</h3>
        <table>
          <thead>
            <tr><th>Name</th><th>Age</th><th>Gender</th><th>School</th></tr>
          </thead>
          <tbody>
            ${children.map(c => `
              <tr>
                <td>${c.name || 'N/A'}</td>
                <td>${c.age}</td>
                <td>${c.gender}</td>
                <td>${c.school || 'N/A'}</td>
              </tr>
            `).join('')}
          </tbody>
        </table>
      `
      : '';

    const preferencesHtml = prefs
      ? `
        <h3>Housing Preferences</h3>
        <table>
          <tr><td><strong>Budget</strong></td><td>${prefs.budgetAmount ? `$${prefs.budgetAmount.toLocaleString()}` : 'N/A'}</td></tr>
          <tr><td><strong>Min Bedrooms</strong></td><td>${prefs.minBedrooms || 'N/A'}</td></tr>
          <tr><td><strong>Min Bathrooms</strong></td><td>${prefs.minBathrooms || 'N/A'}</td></tr>
          <tr><td><strong>Move Timeline</strong></td><td>${prefs.moveTimeline || 'N/A'}</td></tr>
          ${prefs.requiredFeatures && prefs.requiredFeatures.length > 0
            ? `<tr><td><strong>Required Features</strong></td><td>${prefs.requiredFeatures.join(', ')}</td></tr>`
            : ''}
        </table>
      `
      : '';

    const html = `
      <!DOCTYPE html>
      <html>
      <head>
        <title>${husband.lastName} Family - Application Details</title>
        <style>
          body { font-family: Arial, sans-serif; padding: 20px; max-width: 800px; margin: 0 auto; }
          h1 { color: #2d7a3a; border-bottom: 2px solid #2d7a3a; padding-bottom: 10px; }
          h2 { color: #333; margin-top: 24px; }
          h3 { color: #555; margin-top: 20px; border-bottom: 1px solid #ddd; padding-bottom: 5px; }
          table { width: 100%; border-collapse: collapse; margin-top: 10px; }
          td, th { padding: 8px; text-align: left; border-bottom: 1px solid #eee; }
          th { background: #f5f5f5; }
          .status-row { display: flex; gap: 20px; margin: 15px 0; }
          .status-item { padding: 8px 16px; border-radius: 4px; font-weight: 500; }
          .status-pending { background: #fff7e6; color: #d48806; }
          .status-approved { background: #f6ffed; color: #389e0d; }
          .status-rejected { background: #fff1f0; color: #cf1322; }
          .two-column { display: grid; grid-template-columns: 1fr 1fr; gap: 30px; }
          .print-date { color: #888; font-size: 12px; margin-top: 30px; }
          @media print { body { padding: 0; } }
        </style>
      </head>
      <body>
        <h1>${husband.lastName} Family</h1>
        <div class="status-row">
          <span class="status-item status-${boardDecision.toLowerCase()}">${boardDecision}</span>
          <span class="status-item" style="background:#e6f7ff;color:#1890ff;">${formatStageName(stage)}</span>
        </div>

        <div class="two-column">
          <div>
            <h3>Husband</h3>
            <table>
              <tr><td><strong>Name</strong></td><td>${husband.firstName} ${husband.lastName}</td></tr>
              <tr><td><strong>Father's Name</strong></td><td>${husband.fatherName || 'N/A'}</td></tr>
              <tr><td><strong>Email</strong></td><td>${husband.email || 'N/A'}</td></tr>
              <tr><td><strong>Phone</strong></td><td>${getPrimaryPhone(husband.phoneNumbers)}</td></tr>
              <tr><td><strong>Occupation</strong></td><td>${husband.occupation || 'N/A'}</td></tr>
              <tr><td><strong>Employer</strong></td><td>${husband.employerName || 'N/A'}</td></tr>
            </table>
          </div>
          ${wife ? `
          <div>
            <h3>Wife</h3>
            <table>
              <tr><td><strong>Name</strong></td><td>${wife.firstName} ${wife.maidenName ? `(${wife.maidenName})` : ''}</td></tr>
              <tr><td><strong>Father's Name</strong></td><td>${wife.fatherName || 'N/A'}</td></tr>
              <tr><td><strong>Email</strong></td><td>${wife.email || 'N/A'}</td></tr>
              <tr><td><strong>Phone</strong></td><td>${getPrimaryPhone(wife.phoneNumbers)}</td></tr>
              <tr><td><strong>Occupation</strong></td><td>${wife.occupation || 'N/A'}</td></tr>
              <tr><td><strong>High School</strong></td><td>${wife.highSchool || 'N/A'}</td></tr>
            </table>
          </div>
          ` : ''}
        </div>

        ${address ? `
        <h3>Current Address</h3>
        <table>
          <tr><td><strong>Street</strong></td><td>${address.street}${address.street2 ? `, ${address.street2}` : ''}</td></tr>
          <tr><td><strong>City</strong></td><td>${address.city}, ${address.state} ${address.zipCode}</td></tr>
        </table>
        ` : ''}

        <h3>Community</h3>
        <table>
          <tr><td><strong>Current Kehila</strong></td><td>${applicant.currentKehila || 'N/A'}</td></tr>
          <tr><td><strong>Shabbos Shul</strong></td><td>${applicant.shabbosShul || 'N/A'}</td></tr>
        </table>

        ${childrenHtml}
        ${preferencesHtml}

        <p class="print-date">Printed on ${new Date().toLocaleDateString()} at ${new Date().toLocaleTimeString()}</p>
      </body>
      </html>
    `;

    printWindow.document.write(html);
    printWindow.document.close();
    printWindow.onload = () => {
      printWindow.print();
    };
  };

  const tabItems = [
    {
      key: 'overview',
      label: 'Overview',
      children: (
        <OverviewTab
          applicant={applicant}
          onRecordBoardDecision={() => setBoardDecisionModalOpen(true)}
          onUploadDocuments={() => setDocumentUploadModalOpen(true)}
          canApprove={canApproveBoardDecisions()}
        />
      ),
    },
    {
      key: 'housing',
      label: 'Housing Search',
      children: <HousingSearchTab applicant={applicant} />,
    },
    {
      key: 'documents',
      label: 'Documents',
      children: <DocumentsTab applicantId={applicant.id} onUploadDocuments={() => setDocumentUploadModalOpen(true)} />,
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
            <Button icon={<PrinterOutlined />} onClick={handlePrint}>
              Print
            </Button>
            <Button icon={<EditOutlined />} onClick={() => setEditDrawerOpen(true)}>
              Edit
            </Button>
          </div>
        </div>
      </Card>

      {/* Tabs */}
      <Card className="tabs-card">
        <Tabs items={tabItems} />
      </Card>

      {/* Board Decision Modal */}
      <SetBoardDecisionModal
        open={boardDecisionModalOpen}
        onClose={() => setBoardDecisionModalOpen(false)}
        applicant={applicant}
      />

      {/* Edit Applicant Drawer */}
      <EditApplicantDrawer
        open={editDrawerOpen}
        onClose={() => setEditDrawerOpen(false)}
        applicant={applicant}
      />

      {/* Document Upload Modal */}
      <DocumentUploadModal
        open={documentUploadModalOpen}
        onClose={() => setDocumentUploadModalOpen(false)}
        applicant={applicant}
      />
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
interface OverviewTabProps {
  applicant: ApplicantDto;
  onRecordBoardDecision: () => void;
  onUploadDocuments: () => void;
  canApprove: boolean;
}

const OverviewTab = ({ applicant, onRecordBoardDecision, onUploadDocuments, canApprove }: OverviewTabProps) => {
  const { husband, wife, address } = applicant;

  return (
    <div className="tab-content">
      <div className="info-grid">
        {/* Board Review - First card for visibility */}
        <BoardReviewSection
          applicant={applicant}
          onRecordDecision={onRecordBoardDecision}
          onUploadDocuments={onUploadDocuments}
          canApprove={canApprove}
        />

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

        {/* Children */}
        <Card title={`Children (${applicant.children?.length || 0})`} size="small" className="info-card">
          {applicant.children && applicant.children.length > 0 ? (
            <Table
              dataSource={applicant.children}
              columns={[
                { title: 'Name', dataIndex: 'name', key: 'name' },
                { title: 'Age', dataIndex: 'age', key: 'age', width: 60 },
                { title: 'Gender', dataIndex: 'gender', key: 'gender', width: 80 },
                { title: 'School', dataIndex: 'school', key: 'school', render: (v: string) => v || '-' },
              ]}
              rowKey="name"
              pagination={false}
              size="small"
            />
          ) : (
            <Empty description="No children listed" image={Empty.PRESENTED_IMAGE_SIMPLE} />
          )}
        </Card>
      </div>
    </div>
  );
};

// Housing Search Tab
interface HousingSearchTabProps {
  applicant: ApplicantDto;
}

const HousingSearchTab = ({ applicant }: HousingSearchTabProps) => {
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

// Documents Tab
interface DocumentsTabProps {
  applicantId: string;
  onUploadDocuments: () => void;
}

const DocumentsTab = ({ applicantId, onUploadDocuments }: DocumentsTabProps) => {
  const { data: documents, isLoading } = useQuery({
    queryKey: ['applicantDocuments', applicantId],
    queryFn: () => documentsApi.getApplicantDocuments(applicantId),
  });

  const columns = [
    {
      title: 'Document Type',
      dataIndex: 'documentTypeName',
      key: 'documentTypeName',
      render: (text: string) => <Text strong>{text}</Text>,
    },
    {
      title: 'File Name',
      dataIndex: 'fileName',
      key: 'fileName',
    },
    {
      title: 'Uploaded',
      dataIndex: 'uploadedAt',
      key: 'uploadedAt',
      render: (date: string) => new Date(date).toLocaleDateString(),
    },
    {
      title: 'Status',
      key: 'status',
      render: () => <Tag color="success">Uploaded</Tag>,
    },
  ];

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: 40 }}>
        <Spin />
      </div>
    );
  }

  return (
    <div className="tab-content">
      <div style={{ marginBottom: 16 }}>
        <Button type="primary" onClick={onUploadDocuments}>
          Upload Documents
        </Button>
      </div>
      {documents && documents.length > 0 ? (
        <Table
          dataSource={documents}
          columns={columns}
          rowKey="id"
          pagination={false}
          size="middle"
        />
      ) : (
        <Empty description="No documents uploaded yet" />
      )}
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
