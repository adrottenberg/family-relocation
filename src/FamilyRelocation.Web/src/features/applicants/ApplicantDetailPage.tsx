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
  message,
  Tooltip,
  Alert,
} from 'antd';
import {
  ArrowLeftOutlined,
  EditOutlined,
  PhoneOutlined,
  MailOutlined,
  PrinterOutlined,
  MessageOutlined,
  FileTextOutlined,
  HistoryOutlined,
  EyeOutlined,
  DownloadOutlined,
  FormOutlined,
  MobileOutlined,
  HomeOutlined,
  StarFilled,
  BellOutlined,
  ExclamationCircleOutlined,
} from '@ant-design/icons';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { applicantsApi, documentsApi, activitiesApi, propertyMatchesApi, remindersApi } from '../../api';
import type { ApplicantDto, AuditLogDto, ReminderListDto } from '../../api/types';
import { colors, statusTagStyles, stageTagStyles } from '../../theme/antd-theme';
import SetBoardDecisionModal from './SetBoardDecisionModal';
import EditApplicantModal from './EditApplicantModal';
import DocumentUploadModal from './DocumentUploadModal';
import EditPreferencesModal from './EditPreferencesModal';
import StageTimeline from './StageTimeline';
import { getPipelineStage, type TransitionType } from '../pipeline/transitionRules';
import AgreementsRequiredModal from '../pipeline/modals/AgreementsRequiredModal';
import ContractInfoModal from '../pipeline/modals/ContractInfoModal';
import ContractFailedModal from '../pipeline/modals/ContractFailedModal';
import ClosingConfirmModal from '../pipeline/modals/ClosingConfirmModal';
import { LogActivityModal } from '../activities';
import { ReminderDetailModal, SnoozeModal } from '../reminders';
import { PropertyMatchList, CreatePropertyMatchModal, type MatchScheduleData } from '../propertyMatches';
import { ScheduleShowingModal } from '../showings';
import { ShowingSchedulerModal } from '../showings/scheduler';
import './ApplicantDetailPage.css';

const { Title, Text } = Typography;

const ApplicantDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [boardDecisionModalOpen, setBoardDecisionModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [documentUploadModalOpen, setDocumentUploadModalOpen] = useState(false);
  const [activityModalOpen, setActivityModalOpen] = useState(false);
  const [createMatchModalOpen, setCreateMatchModalOpen] = useState(false);
  // Queue of showings to schedule - each modal close advances to the next
  const [showingsToSchedule, setShowingsToSchedule] = useState<MatchScheduleData[]>([]);
  // Drag-and-drop scheduler modal
  const [schedulerModalOpen, setSchedulerModalOpen] = useState(false);
  // Reminder modals
  const [reminderDetailId, setReminderDetailId] = useState<string | null>(null);
  const [snoozeReminderId, setSnoozeReminderId] = useState<string | null>(null);

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

  // Fetch property matches for print
  const housingSearchId = applicant?.housingSearch?.id;
  const { data: propertyMatches } = useQuery({
    queryKey: ['propertyMatches', 'housingSearch', housingSearchId],
    queryFn: () => propertyMatchesApi.getForHousingSearch(housingSearchId!),
    enabled: !!housingSearchId,
  });

  // Fetch urgent reminders for this applicant (overdue + due today)
  const { data: applicantReminders } = useQuery({
    queryKey: ['applicantReminders', id],
    queryFn: () => remindersApi.getByEntity('Applicant', id!, 'Open'),
    enabled: !!id,
  });

  // Filter to only urgent reminders (overdue or due today) using backend-computed flags
  // This avoids timezone issues from client-side date comparison
  const urgentReminders = (applicantReminders || []).filter((r: ReminderListDto) => {
    return r.isOverdue || r.isDueToday;
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
  const stage = hs?.stage || 'Submitted';
  const currentPipelineStage = getPipelineStage(boardDecision, stage);

  const closeTransitionModal = () => {
    setActiveTransitionModal(null);
  };

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
                <td>${c.name || '-'}</td>
                <td>${c.age}</td>
                <td>${c.gender}</td>
                <td>${c.school || '-'}</td>
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
          <tr><td><strong>Budget</strong></td><td>${prefs.budgetAmount ? `$${prefs.budgetAmount.toLocaleString()}` : '-'}</td></tr>
          <tr><td><strong>Min Bedrooms</strong></td><td>${prefs.minBedrooms || '-'}</td></tr>
          <tr><td><strong>Min Bathrooms</strong></td><td>${prefs.minBathrooms || '-'}</td></tr>
          <tr><td><strong>Move Timeline</strong></td><td>${prefs.moveTimeline || '-'}</td></tr>
          ${prefs.requiredFeatures && prefs.requiredFeatures.length > 0
            ? `<tr><td><strong>Required Features</strong></td><td>${prefs.requiredFeatures.join(', ')}</td></tr>`
            : ''}
        </table>
      `
      : '';

    const statusLabels: Record<string, string> = {
      MatchIdentified: 'Match Identified',
      ShowingRequested: 'Showing Requested',
      ApplicantInterested: 'Interested',
      OfferMade: 'Offer Made',
      ApplicantRejected: 'Rejected',
    };

    const propertyMatchesHtml = propertyMatches && propertyMatches.length > 0
      ? `
        <h3>Suggested Listings (${propertyMatches.length})</h3>
        <table>
          <thead>
            <tr><th>Property</th><th>Price</th><th>Beds/Baths</th><th>Score</th><th>Status</th></tr>
          </thead>
          <tbody>
            ${propertyMatches.map(m => `
              <tr>
                <td>${m.propertyStreet}, ${m.propertyCity}</td>
                <td>$${m.propertyPrice.toLocaleString()}</td>
                <td>${m.propertyBedrooms} bed / ${m.propertyBathrooms} bath</td>
                <td>${m.matchScore}%</td>
                <td>${statusLabels[m.status] || m.status}</td>
              </tr>
            `).join('')}
          </tbody>
        </table>
      `
      : '';

    const html = `
      <!DOCTYPE html>
      <html>
      <head>
        <title>${husband.firstName} ${husband.lastName} - Application Details</title>
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
        <h1>${husband.firstName} ${husband.lastName}</h1>
        <div class="status-row">
          <span class="status-item status-${boardDecision.toLowerCase()}">${boardDecision}</span>
          <span class="status-item" style="background:#e6f7ff;color:#1890ff;">${formatStageName(stage)}</span>
        </div>

        <div class="two-column">
          <div>
            <h3>Husband</h3>
            <table>
              <tr><td><strong>Name</strong></td><td>${husband.firstName} ${husband.lastName}</td></tr>
              <tr><td><strong>Father's Name</strong></td><td>${husband.fatherName || '-'}</td></tr>
              <tr><td><strong>Email</strong></td><td>${husband.email || '-'}</td></tr>
              <tr><td><strong>Phone</strong></td><td>${getPrimaryPhone(husband.phoneNumbers)}</td></tr>
              <tr><td><strong>Occupation</strong></td><td>${husband.occupation || '-'}</td></tr>
              <tr><td><strong>Employer</strong></td><td>${husband.employerName || '-'}</td></tr>
            </table>
          </div>
          ${wife ? `
          <div>
            <h3>Wife</h3>
            <table>
              <tr><td><strong>Name</strong></td><td>${wife.firstName} ${wife.maidenName ? `(${wife.maidenName})` : ''}</td></tr>
              <tr><td><strong>Father's Name</strong></td><td>${wife.fatherName || '-'}</td></tr>
              <tr><td><strong>Email</strong></td><td>${wife.email || '-'}</td></tr>
              <tr><td><strong>Phone</strong></td><td>${getPrimaryPhone(wife.phoneNumbers)}</td></tr>
              <tr><td><strong>Occupation</strong></td><td>${wife.occupation || '-'}</td></tr>
              <tr><td><strong>High School</strong></td><td>${wife.highSchool || '-'}</td></tr>
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
          <tr><td><strong>Current Kehila</strong></td><td>${applicant.currentKehila || '-'}</td></tr>
          <tr><td><strong>Shabbos Shul</strong></td><td>${applicant.shabbosShul || '-'}</td></tr>
        </table>

        ${childrenHtml}
        ${preferencesHtml}
        ${propertyMatchesHtml}

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
      children: <OverviewTab applicant={applicant} />,
    },
    ...(hs ? [{
      key: 'matches',
      label: 'Suggested Listings',
      children: (
        <PropertyMatchesTab
          housingSearchId={hs.id}
          applicantId={applicant.id}
          applicantName={`${applicant.husband.firstName} ${applicant.husband.lastName}`}
          onCreateMatch={() => setCreateMatchModalOpen(true)}
          onScheduleShowing={(matches) => {
            // Queue all matches for sequential scheduling
            setShowingsToSchedule(matches);
          }}
          onOpenScheduler={() => setSchedulerModalOpen(true)}
        />
      ),
    }] : []),
    {
      key: 'documents',
      label: 'Documents',
      children: <DocumentsTab applicantId={applicant.id} onUploadDocuments={() => setDocumentUploadModalOpen(true)} />,
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
                {applicant.husband.firstName} {applicant.husband.lastName}
              </Title>
              <div className="header-tags">
                <Tag style={getStatusTagStyle(boardDecision)}>{boardDecision}</Tag>
                <Tag style={getStageTagStyle(stage)}>{formatStageName(stage)}</Tag>
              </div>
            </div>
          </div>
          <div className="header-actions">
            <Button icon={<FormOutlined />} onClick={() => setActivityModalOpen(true)}>
              Log Activity
            </Button>
            <Button icon={<PrinterOutlined />} onClick={handlePrint}>
              Print
            </Button>
            <Button icon={<EditOutlined />} onClick={() => setEditModalOpen(true)}>
              Edit
            </Button>
          </div>
        </div>
      </Card>

      {/* Stage Timeline */}
      <StageTimeline
        applicantId={applicant.id}
        housingSearchId={hs?.id}
        boardDecision={boardDecision}
        currentStage={stage}
        onTransitionModalOpen={(type) => setActiveTransitionModal(type)}
        onBoardDecisionClick={() => setBoardDecisionModalOpen(true)}
      />

      {/* Urgent Reminders Banner - only shows when there are overdue or due today reminders */}
      {urgentReminders.length > 0 && (
        <Alert
          type="warning"
          icon={<BellOutlined />}
          showIcon
          style={{ marginBottom: 16 }}
          message={
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 8 }}>
              <span>
                <strong>{urgentReminders.length} reminder{urgentReminders.length > 1 ? 's' : ''}</strong> need attention
              </span>
              <Space size="small" wrap>
                {urgentReminders.slice(0, 3).map((reminder: ReminderListDto) => (
                  <Tag
                    key={reminder.id}
                    color={reminder.isOverdue ? 'red' : 'orange'}
                    style={{ cursor: 'pointer', margin: 0 }}
                    onClick={() => setReminderDetailId(reminder.id)}
                  >
                    {reminder.isOverdue && <ExclamationCircleOutlined style={{ marginRight: 4 }} />}
                    {reminder.title}
                  </Tag>
                ))}
                {urgentReminders.length > 3 && (
                  <Tag color="default" style={{ margin: 0 }}>+{urgentReminders.length - 3} more</Tag>
                )}
              </Space>
            </div>
          }
        />
      )}

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

      {/* Edit Applicant Modal */}
      <EditApplicantModal
        open={editModalOpen}
        onClose={() => setEditModalOpen(false)}
        applicant={applicant}
      />

      {/* Document Upload Modal */}
      <DocumentUploadModal
        open={documentUploadModalOpen}
        onClose={() => setDocumentUploadModalOpen(false)}
        applicant={applicant}
      />

      {/* Log Activity Modal */}
      <LogActivityModal
        open={activityModalOpen}
        onClose={() => setActivityModalOpen(false)}
        onSuccess={() => {
          setActivityModalOpen(false);
          queryClient.invalidateQueries({ queryKey: ['applicant-audit', applicant.id] });
          // Also invalidate reminders in case a follow-up was created
          queryClient.invalidateQueries({ queryKey: ['applicantReminders', applicant.id] });
          queryClient.invalidateQueries({ queryKey: ['reminderCounts'] });
        }}
        entityType="Applicant"
        entityId={applicant.id}
        entityName={`${applicant.husband.firstName} ${applicant.husband.lastName}`}
      />

      {/* Create Suggested Listing Modal */}
      {hs && (
        <CreatePropertyMatchModal
          open={createMatchModalOpen}
          onClose={() => setCreateMatchModalOpen(false)}
          housingSearchId={hs.id}
        />
      )}

      {/* Schedule Showing Modal - processes queue of showings sequentially */}
      {showingsToSchedule.length > 0 && (
        <ScheduleShowingModal
          open={true}
          onClose={() => {
            // Advance to next showing in queue, or clear if done
            setShowingsToSchedule((prev) => prev.slice(1));
          }}
          propertyMatchId={showingsToSchedule[0].id}
          propertyInfo={{
            street: showingsToSchedule[0].propertyStreet,
            city: showingsToSchedule[0].propertyCity,
          }}
          applicantInfo={{ name: showingsToSchedule[0].applicantName }}
          queueInfo={showingsToSchedule.length > 1 ? {
            current: 1,
            total: showingsToSchedule.length,
          } : undefined}
        />
      )}


      {/* Showing Scheduler Modal (Drag-and-Drop) */}
      {hs && (
        <ShowingSchedulerModal
          open={schedulerModalOpen}
          onClose={() => setSchedulerModalOpen(false)}
          mode="applicant"
          applicantId={applicant.id}
          housingSearchId={hs.id}
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

      {/* Reminder Detail Modal */}
      <ReminderDetailModal
        open={!!reminderDetailId}
        reminderId={reminderDetailId}
        onClose={() => setReminderDetailId(null)}
        onComplete={(remId) => {
          remindersApi.complete(remId).then(() => {
            message.success('Reminder completed');
            queryClient.invalidateQueries({ queryKey: ['applicantReminders', applicant.id] });
            queryClient.invalidateQueries({ queryKey: ['reminderCounts'] });
            setReminderDetailId(null);
          }).catch(() => message.error('Failed to complete reminder'));
        }}
        onSnooze={(remId) => {
          setReminderDetailId(null);
          setSnoozeReminderId(remId);
        }}
        onDismiss={(remId) => {
          remindersApi.dismiss(remId).then(() => {
            message.success('Reminder dismissed');
            queryClient.invalidateQueries({ queryKey: ['applicantReminders', applicant.id] });
            queryClient.invalidateQueries({ queryKey: ['reminderCounts'] });
            setReminderDetailId(null);
          }).catch(() => message.error('Failed to dismiss reminder'));
        }}
        onReopen={(remId) => {
          remindersApi.reopen(remId).then(() => {
            message.success('Reminder reopened');
            queryClient.invalidateQueries({ queryKey: ['applicantReminders', applicant.id] });
            queryClient.invalidateQueries({ queryKey: ['reminderCounts'] });
            setReminderDetailId(null);
          }).catch(() => message.error('Failed to reopen reminder'));
        }}
      />

      {/* Snooze Modal */}
      <SnoozeModal
        open={!!snoozeReminderId}
        onClose={() => setSnoozeReminderId(null)}
        onSnooze={async (snoozeUntil) => {
          if (snoozeReminderId) {
            await remindersApi.snooze(snoozeReminderId, { snoozeUntil });
            message.success('Reminder snoozed');
            queryClient.invalidateQueries({ queryKey: ['applicantReminders', applicant.id] });
            queryClient.invalidateQueries({ queryKey: ['reminderCounts'] });
            setSnoozeReminderId(null);
          }
        }}
      />
    </div>
  );
};

// Helper to get primary phone
const getPrimaryPhone = (phoneNumbers?: { number: string; isPrimary: boolean }[]) => {
  if (!phoneNumbers || phoneNumbers.length === 0) return '-';
  const primary = phoneNumbers.find(p => p.isPrimary);
  return primary?.number || phoneNumbers[0]?.number || '-';
};

// Overview Tab
interface OverviewTabProps {
  applicant: ApplicantDto;
}

const OverviewTab = ({ applicant }: OverviewTabProps) => {
  const { husband, wife, address } = applicant;
  const hs = applicant.housingSearch;
  const [editPreferencesOpen, setEditPreferencesOpen] = useState(false);

  // Get phone icon based on type
  const getPhoneIcon = (type: string) => {
    switch (type.toLowerCase()) {
      case 'mobile':
      case 'cell':
        return <MobileOutlined />;
      case 'home':
        return <HomeOutlined />;
      case 'work':
      case 'office':
        return <PhoneOutlined />;
      default:
        return <PhoneOutlined />;
    }
  };

  // Format phone numbers - one per line with icons
  const formatPhones = (phoneNumbers?: { number: string; type: string; isPrimary: boolean }[]) => {
    if (!phoneNumbers || phoneNumbers.length === 0) return '-';
    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
        {phoneNumbers.map((p, idx) => (
          <span key={idx} style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
            <Tooltip title={p.type}>
              <span style={{ color: '#666' }}>{getPhoneIcon(p.type)}</span>
            </Tooltip>
            <a href={`tel:${p.number}`}>{p.number}</a>
            {p.isPrimary && (
              <Tooltip title="Primary">
                <StarFilled style={{ color: '#faad14', fontSize: 12 }} />
              </Tooltip>
            )}
          </span>
        ))}
      </div>
    );
  };

  return (
    <div className="tab-content">
      {/* Two-column layout for Husband and Wife */}
      <div className="family-info-grid">
        {/* Husband Info */}
        <Card title="Husband" size="small" className="info-card">
          <Descriptions column={1} size="small" labelStyle={{ width: 120 }}>
            <Descriptions.Item label="Name">
              {husband.firstName} {husband.lastName}
            </Descriptions.Item>
            <Descriptions.Item label="Father's Name">
              {husband.fatherName || '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Email">
              {husband.email ? <a href={`mailto:${husband.email}`}>{husband.email}</a> : '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Phone">
              {formatPhones(husband.phoneNumbers)}
            </Descriptions.Item>
            <Descriptions.Item label="Occupation">
              {husband.occupation || '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Employer">
              {husband.employerName || '-'}
            </Descriptions.Item>
          </Descriptions>
        </Card>

        {/* Wife Info */}
        <Card title="Wife" size="small" className="info-card">
          {wife ? (
            <Descriptions column={1} size="small" labelStyle={{ width: 120 }}>
              <Descriptions.Item label="Name">
                {wife.firstName} {wife.maidenName ? `(${wife.maidenName})` : ''}
              </Descriptions.Item>
              <Descriptions.Item label="Father's Name">
                {wife.fatherName || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Email">
                {wife.email ? <a href={`mailto:${wife.email}`}>{wife.email}</a> : '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Phone">
                {formatPhones(wife.phoneNumbers)}
              </Descriptions.Item>
              <Descriptions.Item label="Occupation">
                {wife.occupation || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="High School">
                {wife.highSchool || '-'}
              </Descriptions.Item>
            </Descriptions>
          ) : (
            <Text type="secondary">No wife information</Text>
          )}
        </Card>
      </div>

      {/* Two-column layout for Family Details and Children */}
      <div className="family-info-grid" style={{ marginTop: 16 }}>
        {/* Family Details - Address & Community */}
        <Card title="Family Details" size="small" className="info-card">
          <Descriptions column={1} size="small" labelStyle={{ width: 120 }}>
            <Descriptions.Item label="Address">
              {address ? (
                <>
                  {address.street}{address.street2 ? `, ${address.street2}` : ''}<br />
                  {address.city}, {address.state} {address.zipCode}
                </>
              ) : (
                '-'
              )}
            </Descriptions.Item>
            <Descriptions.Item label="Current Kehila">
              {applicant.currentKehila || '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Shabbos Shul">
              {applicant.shabbosShul || '-'}
            </Descriptions.Item>
          </Descriptions>
        </Card>

        {/* Children */}
        <Card title={`Children (${applicant.children?.length || 0})`} size="small" className="info-card">
          {applicant.children && applicant.children.length > 0 ? (
            <Table
              dataSource={applicant.children}
              columns={[
                { title: 'Name', dataIndex: 'name', key: 'name', render: (v: string) => v || '-' },
                { title: 'Age', dataIndex: 'age', key: 'age', width: 60 },
                { title: 'Gender', dataIndex: 'gender', key: 'gender', width: 80 },
                { title: 'School', dataIndex: 'school', key: 'school', render: (v: string) => v || '-' },
              ]}
              rowKey={(record, index) => record.name || `child-${index}`}
              pagination={false}
              size="small"
            />
          ) : (
            <Empty description="No children listed" image={Empty.PRESENTED_IMAGE_SIMPLE} />
          )}
        </Card>
      </div>

      {/* Housing Searches - show all, active first */}
      {applicant.allHousingSearches && applicant.allHousingSearches.length > 0 && (
        <>
          {applicant.allHousingSearches.map((search, index) => (
            <Card
              key={search.id}
              title={index === 0 && search.isActive ? 'Housing Search' : `Housing Search (${new Date(search.createdDate).toLocaleDateString()})`}
              size="small"
              className="info-card"
              style={{
                marginTop: 16,
                opacity: search.isActive ? 1 : 0.7,
                borderColor: search.isActive ? undefined : '#d9d9d9',
              }}
              extra={
                <Space>
                  <Tag color={search.isActive ? 'green' : 'default'}>{search.isActive ? 'Active' : 'Inactive'}</Tag>
                  {search.isActive && (
                    <Button size="small" icon={<EditOutlined />} onClick={() => setEditPreferencesOpen(true)}>
                      Edit Preferences
                    </Button>
                  )}
                </Space>
              }
            >
              <div className="family-info-grid">
                {/* Status */}
                <Descriptions column={1} size="small" labelStyle={{ width: 140 }}>
                  <Descriptions.Item label="Stage">
                    <Tag>{search.stage}</Tag>
                  </Descriptions.Item>
                  <Descriptions.Item label="Stage Changed">
                    {new Date(search.stageChangedDate).toLocaleDateString()}
                  </Descriptions.Item>
                  <Descriptions.Item label="Failed Contracts">
                    {search.failedContractCount}
                  </Descriptions.Item>
                  {search.currentContract && (
                    <>
                      <Descriptions.Item label="Contract Price">
                        ${search.currentContract.price.toLocaleString()}
                      </Descriptions.Item>
                      <Descriptions.Item label="Contract Date">
                        {new Date(search.currentContract.contractDate).toLocaleDateString()}
                      </Descriptions.Item>
                      {search.currentContract.expectedClosingDate && (
                        <Descriptions.Item label="Expected Closing">
                          {new Date(search.currentContract.expectedClosingDate).toLocaleDateString()}
                        </Descriptions.Item>
                      )}
                    </>
                  )}
                </Descriptions>

                {/* Preferences - only show for active search */}
                {search.isActive && search.preferences && (
                  <Descriptions column={1} size="small" labelStyle={{ width: 140 }}>
                    <Descriptions.Item label="Budget">
                      {search.preferences.budgetAmount
                        ? `$${search.preferences.budgetAmount.toLocaleString()}`
                        : '-'}
                    </Descriptions.Item>
                    <Descriptions.Item label="Bedrooms">
                      {search.preferences.minBedrooms ? `${search.preferences.minBedrooms}+` : '-'}
                    </Descriptions.Item>
                    <Descriptions.Item label="Bathrooms">
                      {search.preferences.minBathrooms ? `${search.preferences.minBathrooms}+` : '-'}
                    </Descriptions.Item>
                    <Descriptions.Item label="Move Timeline">
                      {search.preferences.moveTimeline || '-'}
                    </Descriptions.Item>
                    {search.preferences.requiredFeatures && search.preferences.requiredFeatures.length > 0 && (
                      <Descriptions.Item label="Required Features">
                        <Space wrap size={[4, 4]}>
                          {search.preferences.requiredFeatures.map((feature) => (
                            <Tag key={feature} style={{ margin: 0 }}>{feature}</Tag>
                          ))}
                        </Space>
                      </Descriptions.Item>
                    )}
                  </Descriptions>
                )}
              </div>
            </Card>
          ))}
        </>
      )}

      {/* Edit Preferences Modal - uses the active housing search */}
      {hs && (
        <EditPreferencesModal
          open={editPreferencesOpen}
          onClose={() => setEditPreferencesOpen(false)}
          housingSearchId={hs.id}
          applicantId={applicant.id}
          preferences={hs.preferences}
        />
      )}
    </div>
  );
};


// Suggested Listings Tab
interface PropertyMatchesTabProps {
  housingSearchId: string;
  applicantId: string;
  applicantName: string;
  onCreateMatch: () => void;
  onScheduleShowing: (matches: MatchScheduleData[]) => void;
  onOpenScheduler: () => void;
}

const PropertyMatchesTab = ({ housingSearchId, onCreateMatch, onScheduleShowing, onOpenScheduler }: PropertyMatchesTabProps) => {
  return (
    <div className="tab-content">
      <PropertyMatchList
        housingSearchId={housingSearchId}
        onCreateMatch={onCreateMatch}
        onScheduleShowings={onScheduleShowing}
        onOpenScheduler={onOpenScheduler}
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

  const handleView = (documentId: string) => {
    const url = documentsApi.getViewUrl(documentId);
    window.open(url, '_blank');
  };

  const handleDownload = (documentId: string) => {
    const url = documentsApi.getDownloadUrl(documentId);
    window.open(url, '_blank');
  };

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
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (_: unknown, record: { id: string }) => (
        <Space>
          <Button
            type="link"
            size="small"
            icon={<EyeOutlined />}
            onClick={() => handleView(record.id)}
          >
            View
          </Button>
          <Button
            type="link"
            size="small"
            icon={<DownloadOutlined />}
            onClick={() => handleDownload(record.id)}
          >
            Download
          </Button>
        </Space>
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
          <Button type="primary" icon={<FormOutlined />} onClick={onLogActivity}>
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
        <Button type="primary" icon={<FormOutlined />} onClick={onLogActivity}>
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

// Friendly names for entity types
const entityTypeLabels: Record<string, string> = {
  Applicant: 'Applicant',
  HousingSearch: 'Housing Search',
  PropertyMatch: 'Property Match',
  Showing: 'Showing',
};

// Friendly names for common field names
const fieldLabels: Record<string, string> = {
  Status: 'Status',
  Stage: 'Stage',
  ScheduledDate: 'Scheduled Date',
  ScheduledTime: 'Scheduled Time',
  MatchScore: 'Match Score',
  IsAutoMatched: 'Auto Matched',
  Notes: 'Notes',
  BoardDecision: 'Board Decision',
  MinBedrooms: 'Min Bedrooms',
  MaxBudget: 'Max Budget',
  MinBudget: 'Min Budget',
  PreferredCities: 'Preferred Cities',
  MustHaveFeatures: 'Must Have Features',
  NiceToHaveFeatures: 'Nice to Have Features',
  BrokerUserId: 'Broker',
  CompletedAt: 'Completed At',
  PropertyMatchId: 'Property Match',
  HousingSearchId: 'Housing Search',
  PropertyId: 'Property',
  ApplicantId: 'Applicant',
  ShowingId: 'Showing',
  CreatedAt: 'Created',
  CreatedById: 'Created By',
  CreatedDate: 'Created',
  LastModifiedAt: 'Modified',
  LastModifiedById: 'Modified By',
  LastModifiedDate: 'Modified',
  ModifiedAt: 'Modified',
  ModifiedById: 'Modified By',
  ModifiedDate: 'Modified',
};

// Fields that should only show new value (no old → new comparison)
const metadataFields = new Set([
  'CreatedAt', 'CreatedById', 'LastModifiedAt', 'LastModifiedById',
  'ModifiedAt', 'ModifiedById', 'CreatedBy', 'ModifiedBy',
  'CreatedDate', 'ModifiedDate', 'LastModifiedDate',
]);

// Status value labels
const statusLabels: Record<string, string> = {
  MatchIdentified: 'Match Identified',
  ShowingRequested: 'Showing Requested',
  ApplicantInterested: 'Interested',
  ApplicantRejected: 'Rejected',
  OfferMade: 'Offer Made',
  Scheduled: 'Scheduled',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  NoShow: 'No Show',
  Submitted: 'Submitted',
  AwaitingAgreements: 'Awaiting Agreements',
  BoardReview: 'Board Review',
  HouseHunting: 'House Hunting',
  UnderContract: 'Under Contract',
  Closed: 'Closed',
};

// Format timestamp in friendly format with full date and time
const formatTimestamp = (timestamp: string): string => {
  const date = new Date(timestamp);
  const now = new Date();
  const isToday = date.toDateString() === now.toDateString();
  const isYesterday = new Date(now.getTime() - 86400000).toDateString() === date.toDateString();

  const timeStr = date.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit', hour12: true });

  if (isToday) {
    return `Today at ${timeStr}`;
  }
  if (isYesterday) {
    return `Yesterday at ${timeStr}`;
  }

  const dateStr = date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined,
  });

  return `${dateStr} at ${timeStr}`;
};

// Check if a value looks like a GUID
const isGuid = (value: string): boolean => {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value);
};

// Format a value for display, with optional resolved names lookup
const formatValue = (field: string, value: string, resolvedNames?: Record<string, string>): string => {
  // Handle null/undefined values
  if (value === 'null' || value === 'undefined' || value === '') {
    return '';
  }

  // Check if this GUID has a resolved name
  if (isGuid(value) && resolvedNames?.[value]) {
    return resolvedNames[value];
  }

  // Check for status labels
  if (statusLabels[value]) return statusLabels[value];

  // Format booleans
  if (value === 'true' || value === 'True') return 'Yes';
  if (value === 'false' || value === 'False') return 'No';

  // Format datetimes (fields ending in 'At' OR Modified/Created dates - these have time component)
  const isDateTimeField = field.toLowerCase().endsWith('at') ||
    field.toLowerCase().includes('modified') ||
    field.toLowerCase().includes('created');
  if (isDateTimeField && !field.toLowerCase().includes('time')) {
    const date = new Date(value);
    if (!isNaN(date.getTime())) {
      return date.toLocaleString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
        hour12: true,
      });
    }
  }

  // Format dates (without time) - for fields like ScheduledDate
  if (field.toLowerCase().includes('date') && !field.toLowerCase().includes('time') && !isDateTimeField) {
    const date = new Date(value);
    if (!isNaN(date.getTime()) && value.includes('-')) {
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    }
  }

  // Format times
  if (field.toLowerCase().includes('time') && value.includes(':')) {
    const [hours, minutes] = value.split(':');
    const hour = parseInt(hours, 10);
    const ampm = hour >= 12 ? 'PM' : 'AM';
    const hour12 = hour % 12 || 12;
    return `${hour12}:${minutes} ${ampm}`;
  }

  // Format currency
  if (field.toLowerCase().includes('budget') || field.toLowerCase().includes('price')) {
    const num = parseFloat(value);
    if (!isNaN(num)) {
      return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(num);
    }
  }

  // If it's still a GUID without a resolved name
  if (isGuid(value)) {
    // Handle empty/zero GUID (system/no user)
    if (value.startsWith('00000000-0000-0000') || value === '00000000-0000-0000-0000-000000000000') {
      return 'System';
    }
    // Show truncated version for other unresolved GUIDs
    return value.substring(0, 8) + '...';
  }

  return value;
};

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

  const getActionLabel = (action: string) => {
    switch (action.toLowerCase()) {
      case 'added': return 'Created';
      case 'modified': return 'Updated';
      case 'deleted': return 'Deleted';
      default: return action;
    }
  };

  const formatChanges = (
    oldValues?: Record<string, unknown>,
    newValues?: Record<string, unknown>,
    resolvedNames?: Record<string, string>
  ) => {
    if (!newValues) return null;
    const changes: { field: string; fieldLabel: string; oldValue?: string; newValue: string; isMetadata: boolean }[] = [];

    for (const [key, newVal] of Object.entries(newValues)) {
      // Skip the primary Id field
      if (key === 'Id') continue;

      const oldVal = oldValues?.[key];

      // Normalize empty values: treat null, undefined, "" as equivalent
      const normalizeEmpty = (v: unknown) => (v === null || v === undefined || v === '' ? null : v);
      const normalizedOld = normalizeEmpty(oldVal);
      const normalizedNew = normalizeEmpty(newVal);

      // Skip if no actual change (after normalizing empties)
      if (normalizedOld === normalizedNew) continue;

      const newValStr = String(newVal ?? '');
      // For metadata fields (Created/Modified dates and users), only show new value
      const isMetadata = metadataFields.has(key);
      const oldValStr = !isMetadata && oldVal !== undefined && oldVal !== null && oldVal !== ''
        ? String(oldVal)
        : undefined;

      changes.push({
        field: key,
        fieldLabel: fieldLabels[key] || key.replace(/([A-Z])/g, ' $1').trim(),
        oldValue: oldValStr ? formatValue(key, oldValStr, resolvedNames) : undefined,
        newValue: formatValue(key, newValStr, resolvedNames),
        isMetadata,
      });
    }

    // Sort: metadata fields (Modified/Created) first, then others
    changes.sort((a, b) => {
      if (a.isMetadata && !b.isMetadata) return -1;
      if (!a.isMetadata && b.isMetadata) return 1;
      return 0;
    });

    return changes;
  };

  return (
    <div className="tab-content">
      <Timeline
        items={auditLogs.map((log) => {
          const changes = formatChanges(log.oldValues, log.newValues, log.resolvedNames);
          const entityLabel = entityTypeLabels[log.entityType] || log.entityType;
          const actionLabel = getActionLabel(log.action);
          // Use entityDescription if available, otherwise just the entity type
          const entityInfo = log.entityDescription
            ? `${entityLabel} - ${log.entityDescription}`
            : entityLabel;

          return {
            dot: <HistoryOutlined style={{ color: '#8c8c8c' }} />,
            color: getActionColor(log.action),
            children: (
              <div className="timeline-item">
                <div className="timeline-header">
                  <Space>
                    <Tag color={getActionColor(log.action)}>{actionLabel}</Tag>
                    <Text strong>{entityInfo}</Text>
                  </Space>
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    {formatTimestamp(log.timestamp)}
                  </Text>
                </div>
                {changes && changes.length > 0 && (
                  <div style={{ marginTop: 8, fontSize: 13 }}>
                    {changes.map((change, idx) => (
                      <div key={idx} style={{ color: '#666', marginBottom: 2 }}>
                        <Text style={{ color: change.isMetadata ? '#595959' : '#8c8c8c', fontWeight: change.isMetadata ? 600 : 400 }}>
                          {change.fieldLabel}:
                        </Text>{' '}
                        {change.oldValue && <Text delete type="secondary">{change.oldValue}</Text>}
                        {change.oldValue && ' → '}
                        <Text strong={change.isMetadata}>{change.newValue}</Text>
                      </div>
                    ))}
                  </div>
                )}
                {(!changes || changes.length === 0) && (
                  <Text type="secondary" style={{ display: 'block', marginTop: 4, fontSize: 13 }}>
                    {actionLabel} {entityLabel.toLowerCase()}
                  </Text>
                )}
                {log.userEmail && (
                  <Text type="secondary" style={{ fontSize: 12, display: 'block', marginTop: 4 }}>
                    by {log.userEmail}
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
