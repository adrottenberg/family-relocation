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
  Dropdown,
  message,
} from 'antd';
import type { MenuProps } from 'antd';
import {
  ArrowLeftOutlined,
  EditOutlined,
  PhoneOutlined,
  MailOutlined,
  HomeOutlined,
  UserOutlined,
  PrinterOutlined,
  SwapOutlined,
  BellOutlined,
  CheckOutlined,
  ClockCircleOutlined,
  MessageOutlined,
  FileTextOutlined,
  HistoryOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { applicantsApi, documentsApi, getStageRequirements, housingSearchesApi, remindersApi, activitiesApi } from '../../api';
import type { ApplicantDto, ReminderListDto, AuditLogDto } from '../../api/types';
import { colors, statusTagStyles, stageTagStyles } from '../../theme/antd-theme';
import { useAuthStore } from '../../store/authStore';
import BoardReviewSection from './BoardReviewSection';
import SetBoardDecisionModal from './SetBoardDecisionModal';
import EditApplicantDrawer from './EditApplicantDrawer';
import DocumentUploadModal from './DocumentUploadModal';
import {
  validateTransition,
  formatStage,
  getPipelineStage,
  type PipelineStage,
  type TransitionType,
} from '../pipeline/transitionRules';
import AgreementsRequiredModal from '../pipeline/modals/AgreementsRequiredModal';
import ContractInfoModal from '../pipeline/modals/ContractInfoModal';
import ContractFailedModal from '../pipeline/modals/ContractFailedModal';
import ClosingConfirmModal from '../pipeline/modals/ClosingConfirmModal';
import { CreateReminderModal } from '../reminders';
import { LogActivityModal } from '../activities';
import { PropertyMatchList, CreatePropertyMatchModal } from '../propertyMatches';
import { ScheduleShowingModal } from '../showings';
import './ApplicantDetailPage.css';

const { Title, Text } = Typography;

const ApplicantDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const canApproveBoardDecisions = useAuthStore((state) => state.canApproveBoardDecisions);
  const [boardDecisionModalOpen, setBoardDecisionModalOpen] = useState(false);
  const [editDrawerOpen, setEditDrawerOpen] = useState(false);
  const [documentUploadModalOpen, setDocumentUploadModalOpen] = useState(false);
  const [reminderModalOpen, setReminderModalOpen] = useState(false);
  const [activityModalOpen, setActivityModalOpen] = useState(false);
  const [createMatchModalOpen, setCreateMatchModalOpen] = useState(false);
  const [scheduleShowingModalData, setScheduleShowingModalData] = useState<{
    propertyMatchId: string;
    propertyInfo?: { street: string; city: string };
    applicantInfo?: { name: string };
  } | null>(null);

  // Stage transition modal state
  const [activeTransitionModal, setActiveTransitionModal] = useState<TransitionType | null>(null);

  const { data: applicant, isLoading, error } = useQuery({
    queryKey: ['applicant', id],
    queryFn: () => applicantsApi.getById(id!),
    enabled: !!id,
  });

  const { data: auditLogs } = useQuery({
    queryKey: ['applicant-audit', id],
    queryFn: () => applicantsApi.getAuditLogs(id!, { page: 1, pageSize: 50 }),
    enabled: !!id,
  });

  // Direct stage change mutation (for transitions that don't need a modal)
  const directStageMutation = useMutation({
    mutationFn: (newStage: string) =>
      housingSearchesApi.changeStage(applicant?.housingSearch?.id || '', { newStage }),
    onSuccess: () => {
      message.success('Stage updated successfully');
      queryClient.invalidateQueries({ queryKey: ['applicant', id] });
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
    },
    onError: () => {
      message.error('Failed to update stage');
    },
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
      'AwaitingAgreements': 'submitted', // Reuse submitted style for AwaitingAgreements
      'Searching': 'houseHunting', // Reuse houseHunting style for Searching
      'UnderContract': 'underContract',
      'Closed': 'closed',
    };
    const key = stageMap[stageName];
    return key ? stageTagStyles[key] : { backgroundColor: colors.neutral[100], color: colors.neutral[600] };
  };

  const formatStageName = (stage: string) => {
    const names: Record<string, string> = {
      'AwaitingAgreements': 'Awaiting Agreements',
      'Searching': 'Searching',
      'UnderContract': 'Under Contract',
      'Closed': 'Closed',
      'Paused': 'Paused',
      'MovedIn': 'Moved In',
    };
    return names[stage] || stage;
  };

  const hs = applicant.housingSearch;
  const boardDecision = applicant.boardReview?.decision || 'Pending';
  const stage = hs?.stage || 'N/A';

  // Get the current pipeline stage
  const currentPipelineStage = getPipelineStage(boardDecision, stage);

  // Build the stage change dropdown menu
  const getStageChangeMenuItems = (): MenuProps['items'] => {
    if (!currentPipelineStage || currentPipelineStage === 'Closed') {
      return [];
    }

    const allStages: PipelineStage[] = ['Submitted', 'AwaitingAgreements', 'Searching', 'UnderContract', 'Closed'];
    const items: MenuProps['items'] = [];

    for (const targetStage of allStages) {
      if (targetStage === currentPipelineStage) continue;

      const transition = validateTransition(currentPipelineStage, targetStage, {
        boardDecision,
      });

      if (transition.type !== 'blocked') {
        items.push({
          key: targetStage,
          label: formatStage(targetStage),
          onClick: () => handleStageChange(targetStage),
        });
      }
    }

    return items;
  };

  const handleStageChange = async (toStage: PipelineStage) => {
    if (!currentPipelineStage) return;

    const transition = validateTransition(currentPipelineStage, toStage, {
      boardDecision,
    });

    switch (transition.type) {
      case 'needsBoardApproval':
        setBoardDecisionModalOpen(true);
        break;
      case 'needsAgreements':
        // Check if all required documents are already uploaded
        try {
          const requirements = await getStageRequirements(currentPipelineStage, toStage, applicant.id);
          const allRequiredUploaded = requirements.requirements.every(
            (req) => !req.isRequired || req.isUploaded
          );
          if (allRequiredUploaded) {
            // All docs uploaded - transition directly
            directStageMutation.mutate(toStage);
          } else {
            // Show modal for missing documents
            setActiveTransitionModal('needsAgreements');
          }
        } catch {
          // On error, show modal anyway
          setActiveTransitionModal('needsAgreements');
        }
        break;
      case 'needsContractInfo':
        setActiveTransitionModal('needsContractInfo');
        break;
      case 'needsClosingInfo':
        setActiveTransitionModal('needsClosingInfo');
        break;
      case 'contractFailed':
        setActiveTransitionModal('contractFailed');
        break;
      default:
        break;
    }
  };

  const closeTransitionModal = () => {
    setActiveTransitionModal(null);
  };

  const stageChangeMenuItems = getStageChangeMenuItems();

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
    ...(hs ? [{
      key: 'matches',
      label: 'Property Matches',
      children: (
        <PropertyMatchesTab
          housingSearchId={hs.id}
          applicantName={`${applicant.husband.lastName} Family`}
          onCreateMatch={() => setCreateMatchModalOpen(true)}
          onScheduleShowing={(matchIds) => {
            // For now, schedule for first match (could extend to batch)
            const matchId = matchIds[0];
            setScheduleShowingModalData({
              propertyMatchId: matchId,
              applicantInfo: { name: `${applicant.husband.lastName} Family` },
            });
          }}
        />
      ),
    }] : []),
    {
      key: 'documents',
      label: 'Documents',
      children: <DocumentsTab applicantId={applicant.id} onUploadDocuments={() => setDocumentUploadModalOpen(true)} />,
    },
    {
      key: 'reminders',
      label: 'Reminders',
      children: <RemindersTab applicantId={applicant.id} onAddReminder={() => setReminderModalOpen(true)} />,
    },
    {
      key: 'activity',
      label: 'Activity',
      children: <ActivityTab applicantId={applicant.id} onLogActivity={() => setActivityModalOpen(true)} />,
    },
    {
      key: 'history',
      label: 'History',
      children: <AuditHistoryTab auditLogs={auditLogs?.items || []} isLoading={!auditLogs} />,
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
                {stageChangeMenuItems && stageChangeMenuItems.length > 0 && (
                  <Dropdown menu={{ items: stageChangeMenuItems }} trigger={['click']}>
                    <Button size="small" icon={<SwapOutlined />}>
                      Change Stage
                    </Button>
                  </Dropdown>
                )}
              </div>
            </div>
          </div>
          <div className="header-actions">
            <Button icon={<PhoneOutlined />} onClick={() => setActivityModalOpen(true)}>
              Log Activity
            </Button>
            <Button icon={<BellOutlined />} onClick={() => setReminderModalOpen(true)}>
              Add Reminder
            </Button>
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

      {/* Create Reminder Modal */}
      <CreateReminderModal
        open={reminderModalOpen}
        onClose={() => setReminderModalOpen(false)}
        onSuccess={() => {
          setReminderModalOpen(false);
          queryClient.invalidateQueries({ queryKey: ['applicantReminders', applicant.id] });
        }}
        entityType="Applicant"
        entityId={applicant.id}
        entityDisplayName={`${applicant.husband.lastName} Family`}
      />

      {/* Log Activity Modal */}
      <LogActivityModal
        open={activityModalOpen}
        onClose={() => setActivityModalOpen(false)}
        onSuccess={() => {
          setActivityModalOpen(false);
          queryClient.invalidateQueries({ queryKey: ['applicant-audit', applicant.id] });
        }}
        entityType="Applicant"
        entityId={applicant.id}
        entityName={`${applicant.husband.lastName} Family`}
      />

      {/* Create Property Match Modal */}
      {hs && (
        <CreatePropertyMatchModal
          open={createMatchModalOpen}
          onClose={() => setCreateMatchModalOpen(false)}
          housingSearchId={hs.id}
        />
      )}

      {/* Schedule Showing Modal */}
      {scheduleShowingModalData && (
        <ScheduleShowingModal
          open={true}
          onClose={() => setScheduleShowingModalData(null)}
          propertyMatchId={scheduleShowingModalData.propertyMatchId}
          propertyInfo={scheduleShowingModalData.propertyInfo}
          applicantInfo={scheduleShowingModalData.applicantInfo}
        />
      )}

      {/* Stage Transition Modals */}
      {hs && (
        <>
          <AgreementsRequiredModal
            open={activeTransitionModal === 'needsAgreements'}
            onClose={closeTransitionModal}
            applicantId={applicant.id}
            housingSearchId={hs.id}
            familyName={applicant.husband.lastName}
            fromStage={currentPipelineStage || 'AwaitingAgreements'}
            toStage="Searching"
          />

          <ContractInfoModal
            open={activeTransitionModal === 'needsContractInfo'}
            onClose={closeTransitionModal}
            housingSearchId={hs.id}
            familyName={applicant.husband.lastName}
          />

          <ContractFailedModal
            open={activeTransitionModal === 'contractFailed'}
            onClose={closeTransitionModal}
            housingSearchId={hs.id}
            familyName={applicant.husband.lastName}
          />

          <ClosingConfirmModal
            open={activeTransitionModal === 'needsClosingInfo'}
            onClose={closeTransitionModal}
            housingSearchId={hs.id}
            familyName={applicant.husband.lastName}
          />
        </>
      )}
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

  // Determine if board review section should be shown
  // Hide once they're past AwaitingAgreements (i.e., in Searching, UnderContract, Closed, etc.)
  const boardDecision = applicant.boardReview?.decision || 'Pending';
  const housingSearchStage = applicant.housingSearch?.stage;
  const showBoardReview =
    boardDecision === 'Pending' ||
    boardDecision === 'Deferred' ||
    boardDecision === 'Rejected' ||
    housingSearchStage === 'AwaitingAgreements' ||
    !housingSearchStage;

  return (
    <div className="tab-content">
      <div className="info-grid">
        {/* Board Review - Show only for pending/awaiting stages */}
        {showBoardReview && (
          <BoardReviewSection
            applicant={applicant}
            onRecordDecision={onRecordBoardDecision}
            onUploadDocuments={onUploadDocuments}
            canApprove={canApprove}
          />
        )}

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

// Property Matches Tab
interface PropertyMatchesTabProps {
  housingSearchId: string;
  applicantName: string;
  onCreateMatch: () => void;
  onScheduleShowing: (matchIds: string[]) => void;
}

const PropertyMatchesTab = ({ housingSearchId, onCreateMatch, onScheduleShowing }: PropertyMatchesTabProps) => {
  return (
    <div className="tab-content">
      <PropertyMatchList
        housingSearchId={housingSearchId}
        onCreateMatch={onCreateMatch}
        onScheduleShowings={onScheduleShowing}
        showApplicant={false}
        showProperty={true}
      />
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

// Reminders Tab
interface RemindersTabProps {
  applicantId: string;
  onAddReminder: () => void;
}

const RemindersTab = ({ applicantId, onAddReminder }: RemindersTabProps) => {
  const queryClient = useQueryClient();

  const { data: reminders, isLoading } = useQuery({
    queryKey: ['applicantReminders', applicantId],
    queryFn: () => remindersApi.getByEntity('Applicant', applicantId, 'Open'),
  });

  const completeMutation = useMutation({
    mutationFn: (reminderId: string) => remindersApi.complete(reminderId),
    onSuccess: () => {
      message.success('Reminder completed');
      queryClient.invalidateQueries({ queryKey: ['applicantReminders', applicantId] });
    },
    onError: () => {
      message.error('Failed to complete reminder');
    },
  });

  const getPriorityColor = (priority: string) => {
    const colors: Record<string, string> = {
      Urgent: 'red',
      High: 'orange',
      Normal: 'blue',
      Low: 'default',
    };
    return colors[priority] || 'default';
  };

  const isOverdue = (dueDate: string) => {
    return new Date(dueDate) < new Date();
  };

  const isDueToday = (dueDate: string) => {
    const today = new Date();
    const due = new Date(dueDate);
    return (
      due.getFullYear() === today.getFullYear() &&
      due.getMonth() === today.getMonth() &&
      due.getDate() === today.getDate()
    );
  };

  const columns = [
    {
      title: 'Title',
      dataIndex: 'title',
      key: 'title',
      render: (text: string, record: ReminderListDto) => (
        <Space>
          <Text strong>{text}</Text>
          {isOverdue(record.dueDate) && <Tag color="red">Overdue</Tag>}
          {!isOverdue(record.dueDate) && isDueToday(record.dueDate) && (
            <Tag color="orange">Due Today</Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Due Date',
      dataIndex: 'dueDate',
      key: 'dueDate',
      width: 120,
      render: (date: string) => new Date(date).toLocaleDateString(),
    },
    {
      title: 'Priority',
      dataIndex: 'priority',
      key: 'priority',
      width: 100,
      render: (priority: string) => <Tag color={getPriorityColor(priority)}>{priority}</Tag>,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      render: (_: unknown, record: ReminderListDto) => (
        <Button
          type="link"
          size="small"
          icon={<CheckOutlined />}
          onClick={() => completeMutation.mutate(record.id)}
          loading={completeMutation.isPending}
        >
          Complete
        </Button>
      ),
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
        <Button type="primary" icon={<BellOutlined />} onClick={onAddReminder}>
          Add Reminder
        </Button>
      </div>
      {reminders && reminders.length > 0 ? (
        <Table
          dataSource={reminders}
          columns={columns}
          rowKey="id"
          pagination={false}
          size="middle"
          rowClassName={(record) => {
            if (isOverdue(record.dueDate)) return 'reminder-overdue';
            if (isDueToday(record.dueDate)) return 'reminder-due-today';
            return '';
          }}
        />
      ) : (
        <Empty
          description="No outstanding reminders"
          image={<ClockCircleOutlined style={{ fontSize: 48, color: '#d9d9d9' }} />}
        />
      )}
    </div>
  );
};

// Activity Tab
// Activity Tab with support for different activity types
interface ActivityTabProps {
  applicantId: string;
  onLogActivity: () => void;
}

const ActivityTab = ({ applicantId, onLogActivity }: ActivityTabProps) => {
  const { data: activities, isLoading } = useQuery({
    queryKey: ['applicant-activities', applicantId],
    queryFn: () => activitiesApi.getByEntity('Applicant', applicantId, 1, 50),
  });

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: 40 }}>
        <Spin />
      </div>
    );
  }

  if (!activities?.items || activities.items.length === 0) {
    return (
      <div className="tab-content">
        <div style={{ marginBottom: 16 }}>
          <Button type="primary" icon={<PhoneOutlined />} onClick={onLogActivity}>
            Log Activity
          </Button>
        </div>
        <Empty description="No activity recorded" />
      </div>
    );
  }

  const getActivityIcon = (type: string, action: string) => {
    switch (type) {
      case 'PhoneCall':
        return <PhoneOutlined style={{ color: '#1890ff' }} />;
      case 'Email':
        return <MailOutlined style={{ color: '#722ed1' }} />;
      case 'SMS':
        return <MessageOutlined style={{ color: '#13c2c2' }} />;
      case 'Note':
        return <FileTextOutlined style={{ color: '#fa8c16' }} />;
      default:
        // System activity
        if (action === 'Added' || action === 'Created') {
          return <HistoryOutlined style={{ color: '#52c41a' }} />;
        }
        return <HistoryOutlined style={{ color: '#8c8c8c' }} />;
    }
  };

  const getActivityColor = (type: string, action: string) => {
    switch (type) {
      case 'PhoneCall':
        return 'blue';
      case 'Email':
        return 'purple';
      case 'SMS':
        return 'cyan';
      case 'Note':
        return 'orange';
      default:
        if (action === 'Added' || action === 'Created') return 'green';
        if (action === 'Modified' || action === 'Updated') return 'blue';
        if (action === 'Deleted') return 'red';
        return 'gray';
    }
  };

  const formatActivityTitle = (activity: { type: string; action: string; entityType: string; outcome?: string }) => {
    switch (activity.type) {
      case 'PhoneCall':
        return activity.outcome ? `Phone Call - ${activity.outcome}` : 'Phone Call';
      case 'Email':
        return 'Email';
      case 'SMS':
        return 'SMS';
      case 'Note':
        return 'Note';
      default:
        return `${activity.action} ${activity.entityType}`;
    }
  };

  return (
    <div className="tab-content">
      <div style={{ marginBottom: 16 }}>
        <Button type="primary" icon={<PhoneOutlined />} onClick={onLogActivity}>
          Log Activity
        </Button>
      </div>
      <Timeline
        items={activities.items.map((activity) => ({
          dot: getActivityIcon(activity.type, activity.action),
          color: getActivityColor(activity.type, activity.action),
          children: (
            <div className="timeline-item">
              <div className="timeline-header">
                <Space>
                  <Text strong>{formatActivityTitle(activity)}</Text>
                  {activity.type === 'PhoneCall' && activity.durationMinutes && (
                    <Tag>{activity.durationMinutes} min</Tag>
                  )}
                </Space>
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {new Date(activity.timestamp).toLocaleString()}
                </Text>
              </div>
              <Text style={{ display: 'block', marginTop: 4 }}>
                {activity.description}
              </Text>
              {activity.userName && (
                <Text type="secondary" style={{ fontSize: 13 }}>
                  by {activity.userName}
                </Text>
              )}
            </div>
          ),
        }))}
      />
    </div>
  );
};

// Audit History Tab
interface AuditHistoryTabProps {
  auditLogs: AuditLogDto[];
  isLoading: boolean;
}

const AuditHistoryTab = ({ auditLogs, isLoading }: AuditHistoryTabProps) => {
  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: 40 }}>
        <Spin />
      </div>
    );
  }

  if (!auditLogs || auditLogs.length === 0) {
    return (
      <div className="tab-content">
        <Empty description="No audit history recorded" />
      </div>
    );
  }

  const getActionColor = (action: string) => {
    switch (action.toLowerCase()) {
      case 'created':
      case 'added':
        return 'green';
      case 'updated':
      case 'modified':
        return 'blue';
      case 'deleted':
        return 'red';
      default:
        return 'default';
    }
  };

  const formatChanges = (oldValues?: Record<string, unknown>, newValues?: Record<string, unknown>) => {
    if (!newValues) return null;
    const changes: { field: string; oldValue?: string; newValue: string }[] = [];

    for (const [key, newVal] of Object.entries(newValues)) {
      const oldVal = oldValues?.[key];
      if (oldVal !== newVal) {
        changes.push({
          field: key,
          oldValue: oldVal !== undefined ? String(oldVal) : undefined,
          newValue: String(newVal),
        });
      }
    }
    return changes;
  };

  return (
    <div className="tab-content">
      <Timeline
        items={auditLogs.map((log) => {
          const changes = formatChanges(log.oldValues, log.newValues);
          return {
            dot: <HistoryOutlined style={{ color: '#8c8c8c' }} />,
            color: getActionColor(log.action),
            children: (
              <div className="timeline-item">
                <div className="timeline-header">
                  <Space>
                    <Tag color={getActionColor(log.action)}>{log.action}</Tag>
                    <Text strong>{log.entityType}</Text>
                  </Space>
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    {new Date(log.timestamp).toLocaleString()}
                  </Text>
                </div>
                {changes && changes.length > 0 && (
                  <div style={{ marginTop: 8, fontSize: 13 }}>
                    {changes.map((change, idx) => (
                      <div key={idx} style={{ color: '#666' }}>
                        <Text code>{change.field}</Text>:{' '}
                        {change.oldValue && <Text delete type="secondary">{change.oldValue}</Text>}
                        {change.oldValue && ' → '}
                        <Text>{change.newValue}</Text>
                      </div>
                    ))}
                  </div>
                )}
                {!changes?.length && (
                  <Text style={{ display: 'block', marginTop: 4 }}>
                    {log.action} {log.entityType}
                  </Text>
                )}
                {log.userName && (
                  <Text type="secondary" style={{ fontSize: 13, display: 'block', marginTop: 4 }}>
                    by {log.userName}
                  </Text>
                )}
              </div>
            ),
          };
        })}
      />
    </div>
  );
};

export default ApplicantDetailPage;
