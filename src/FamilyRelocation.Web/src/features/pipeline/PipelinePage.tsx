import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, Input, Select, Typography, Spin, Empty, message } from 'antd';
import { SearchOutlined, UserOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { applicantsApi, housingSearchesApi, getStageRequirements } from '../../api';
import type { ApplicantListItemDto } from '../../api/types';
import { colors } from '../../theme/antd-theme';
import { validateTransition, getPipelineStage, type Stage, type TransitionType } from './transitionRules';
import { useAuthStore } from '../../store/authStore';
import {
  TransitionBlockedModal,
  BoardApprovalRequiredModal,
  AgreementsRequiredModal,
  ContractInfoModal,
  ClosingConfirmModal,
  ContractFailedModal,
} from './modals';
import './PipelinePage.css';

const { Title, Text } = Typography;
const { Option } = Select;

// Stage configuration for pipeline columns
// Note: With the refactored model, we show 5 columns: Submitted, AwaitingAgreements, Searching, UnderContract, Closed
const stageConfig: Record<string, { color: string; bg: string; label: string }> = {
  Submitted: { color: '#3b82f6', bg: '#dbeafe', label: 'Submitted' },
  AwaitingAgreements: { color: '#06b6d4', bg: '#cffafe', label: 'Awaiting Agreements' },
  Searching: { color: '#f59e0b', bg: '#fef3c7', label: 'Searching' },
  UnderContract: { color: '#8b5cf6', bg: '#ede9fe', label: 'Under Contract' },
  Closed: { color: '#10b981', bg: '#d1fae5', label: 'Closed' },
};

// Pipeline item type for kanban cards
interface PipelineItem {
  applicantId: string;
  housingSearchId: string;
  familyName: string;
  husbandFirstName: string;
  wifeFirstName?: string;
  childrenCount: number;
  boardDecision: string;
  stage: string;
  daysInStage: number;
  budget?: number;
  preferredCities?: string[];
}

// Pipeline stage type for kanban columns
interface PipelineStage {
  stage: string;
  count: number;
  items: PipelineItem[];
}

// Modal state type
interface ModalState {
  type: TransitionType | null;
  applicantId: string;
  housingSearchId: string;
  familyName: string;
  fromStage: string;
  toStage: string;
  message: string;
}

const initialModalState: ModalState = {
  type: null,
  applicantId: '',
  housingSearchId: '',
  familyName: '',
  fromStage: '',
  toStage: '',
  message: '',
};

const PipelinePage = () => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const canApproveBoardDecisions = useAuthStore((state) => state.canApproveBoardDecisions);
  const [search, setSearch] = useState('');
  const [cityFilter, setCityFilter] = useState<string | undefined>();
  const [boardDecisionFilter, setBoardDecisionFilter] = useState<string | undefined>();
  const [modalState, setModalState] = useState<ModalState>(initialModalState);

  // Fetch all applicants with a large page size for the pipeline view
  const { data: rawData, isLoading, error } = useQuery({
    queryKey: ['pipeline', search, boardDecisionFilter],
    queryFn: () =>
      applicantsApi.getAll({
        pageSize: 1000,
        search: search || undefined,
        boardDecision: boardDecisionFilter || undefined,
      }),
  });

  // Transform flat applicant list into pipeline stages
  // Uses getPipelineStage to determine the correct column based on board decision and housing search stage
  const data = useMemo(() => {
    if (!rawData?.items) return null;

    // Pipeline stages: Submitted, AwaitingAgreements, Searching, UnderContract, Closed
    const stageOrder = ['Submitted', 'AwaitingAgreements', 'Searching', 'UnderContract', 'Closed'];

    const stages: PipelineStage[] = stageOrder.map((stageName) => {
      const stageItems = rawData.items
        .filter((a: ApplicantListItemDto) => {
          // Determine pipeline stage from board decision and housing search stage
          // Returns null for rejected applicants (filtered out)
          const pipelineStage = getPipelineStage(a.boardDecision, a.stage);
          return pipelineStage === stageName;
        })
        .map((a: ApplicantListItemDto): PipelineItem => {
          const nameParts = a.husbandFullName.split(' ');
          const familyName = nameParts.pop() || a.husbandFullName;
          const husbandFirstName = nameParts.join(' ') || '';
          const createdDate = new Date(a.createdDate);
          const daysInStage = Math.floor((Date.now() - createdDate.getTime()) / (1000 * 60 * 60 * 24));

          return {
            applicantId: a.id,
            housingSearchId: a.housingSearchId || '',
            familyName,
            husbandFirstName,
            wifeFirstName: a.wifeMaidenName,
            childrenCount: 0, // Not available in list view
            boardDecision: a.boardDecision || 'Pending',
            stage: stageName, // Use the stage we're filtering for (already validated by filter)
            daysInStage,
            budget: undefined, // Not available in list view
            preferredCities: undefined, // Not available in list view
          };
        });

      return {
        stage: stageName,
        count: stageItems.length,
        items: stageItems,
      };
    });

    return {
      stages,
      totalCount: rawData.totalCount,
    };
  }, [rawData]);

  const changeStage = useMutation({
    mutationFn: ({ housingSearchId, newStage }: { housingSearchId: string; newStage: string }) =>
      housingSearchesApi.changeStage(housingSearchId, { newStage }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
      message.success('Stage updated successfully');
    },
    onError: () => {
      message.error('Failed to update stage');
    },
  });

  const handleDragStart = (e: React.DragEvent, item: PipelineItem) => {
    e.dataTransfer.setData('applicantId', item.applicantId);
    e.dataTransfer.setData('housingSearchId', item.housingSearchId);
    e.dataTransfer.setData('currentStage', item.stage);
    e.dataTransfer.setData('familyName', item.familyName);
    e.dataTransfer.setData('boardDecision', item.boardDecision);
    e.dataTransfer.effectAllowed = 'move';
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
  };

  const handleDrop = async (e: React.DragEvent, targetStage: string) => {
    e.preventDefault();
    const applicantId = e.dataTransfer.getData('applicantId');
    const housingSearchId = e.dataTransfer.getData('housingSearchId');
    const currentStage = e.dataTransfer.getData('currentStage');
    const familyName = e.dataTransfer.getData('familyName');
    const boardDecision = e.dataTransfer.getData('boardDecision');

    if (currentStage === targetStage) return;

    // Validate the transition
    const result = validateTransition(
      currentStage as Stage,
      targetStage as Stage,
      {
        boardDecision,
      }
    );

    if (result.type === 'direct') {
      // Direct transition allowed
      changeStage.mutate({ housingSearchId, newStage: targetStage });
    } else if (result.type === 'needsAgreements') {
      // Check if all required documents are already uploaded
      try {
        const requirements = await getStageRequirements(currentStage, targetStage, applicantId);
        const allRequiredUploaded = requirements.requirements.every(
          (req) => !req.isRequired || req.isUploaded
        );
        if (allRequiredUploaded) {
          // All docs uploaded - transition directly
          changeStage.mutate({ housingSearchId, newStage: targetStage });
        } else {
          // Show modal for missing documents
          setModalState({
            type: result.type,
            applicantId,
            housingSearchId,
            familyName,
            fromStage: currentStage,
            toStage: targetStage,
            message: result.message || '',
          });
        }
      } catch {
        // On error, show modal anyway
        setModalState({
          type: result.type,
          applicantId,
          housingSearchId,
          familyName,
          fromStage: currentStage,
          toStage: targetStage,
          message: result.message || '',
        });
      }
    } else {
      // Show appropriate modal
      setModalState({
        type: result.type,
        applicantId,
        housingSearchId,
        familyName,
        fromStage: currentStage,
        toStage: targetStage,
        message: result.message || '',
      });
    }
  };

  const closeModal = () => {
    setModalState(initialModalState);
  };

  const handleCardClick = (applicantId: string) => {
    navigate(`/applicants/${applicantId}`);
  };

  // City filter is not available in list view (would need to fetch full applicant details)
  // For now, show common cities as static options
  const commonCities = ['Union', 'Roselle Park', 'Elizabeth'];

  if (error) {
    return (
      <Card>
        <Empty description="Failed to load pipeline data" />
      </Card>
    );
  }

  // Order stages for display (data is already ordered from useMemo)
  const orderedStages = data?.stages || [];

  return (
    <div className="pipeline-page">
      {/* Header */}
      <div className="pipeline-header">
        <div className="header-left">
          <Title level={3} style={{ margin: 0 }}>Pipeline</Title>
          {data && (
            <Text type="secondary">{data.totalCount} families</Text>
          )}
        </div>
        <div className="header-filters">
          <Input
            placeholder="Search families..."
            prefix={<SearchOutlined />}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            style={{ width: 220 }}
            allowClear
          />
          <Select
            placeholder="City"
            value={cityFilter}
            onChange={setCityFilter}
            style={{ width: 140 }}
            allowClear
          >
            {commonCities.map((city) => (
              <Option key={city} value={city}>{city}</Option>
            ))}
          </Select>
          <Select
            placeholder="Board Status"
            value={boardDecisionFilter}
            onChange={setBoardDecisionFilter}
            style={{ width: 140 }}
            allowClear
          >
            <Option value="Pending">Pending</Option>
            <Option value="Approved">Approved</Option>
            <Option value="Deferred">Deferred</Option>
          </Select>
        </div>
      </div>

      {/* Kanban Board */}
      {isLoading ? (
        <div className="loading-container">
          <Spin size="large" />
        </div>
      ) : (
        <div className="kanban-board">
          {orderedStages.map((stage) => (
            <KanbanColumn
              key={stage.stage}
              stage={stage}
              config={stageConfig[stage.stage] || stageConfig.Submitted}
              onDragStart={handleDragStart}
              onDragOver={handleDragOver}
              onDrop={handleDrop}
              onCardClick={handleCardClick}
            />
          ))}
        </div>
      )}

      {/* Transition Modals */}
      <TransitionBlockedModal
        open={modalState.type === 'blocked'}
        onClose={closeModal}
        message={modalState.message}
        familyName={modalState.familyName}
      />

      <BoardApprovalRequiredModal
        open={modalState.type === 'needsBoardApproval'}
        onClose={closeModal}
        applicantId={modalState.applicantId}
        familyName={modalState.familyName}
        canApprove={canApproveBoardDecisions()}
      />

      <AgreementsRequiredModal
        open={modalState.type === 'needsAgreements'}
        onClose={closeModal}
        applicantId={modalState.applicantId}
        housingSearchId={modalState.housingSearchId}
        familyName={modalState.familyName}
        fromStage={modalState.fromStage}
        toStage={modalState.toStage}
        onTransitionComplete={() => {
          queryClient.invalidateQueries({ queryKey: ['pipeline'] });
          closeModal();
        }}
      />

      <ContractInfoModal
        open={modalState.type === 'needsContractInfo'}
        onClose={closeModal}
        housingSearchId={modalState.housingSearchId}
        familyName={modalState.familyName}
      />

      <ClosingConfirmModal
        open={modalState.type === 'needsClosingInfo'}
        onClose={closeModal}
        housingSearchId={modalState.housingSearchId}
        familyName={modalState.familyName}
      />

      <ContractFailedModal
        open={modalState.type === 'contractFailed'}
        onClose={closeModal}
        housingSearchId={modalState.housingSearchId}
        familyName={modalState.familyName}
      />
    </div>
  );
};

// Kanban Column Component
interface KanbanColumnProps {
  stage: PipelineStage;
  config: { color: string; bg: string; label: string };
  onDragStart: (e: React.DragEvent, item: PipelineItem) => void;
  onDragOver: (e: React.DragEvent) => void;
  onDrop: (e: React.DragEvent, stage: string) => void;
  onCardClick: (applicantId: string) => void;
}

const KanbanColumn = ({
  stage,
  config,
  onDragStart,
  onDragOver,
  onDrop,
  onCardClick,
}: KanbanColumnProps) => {
  return (
    <div
      className="kanban-column"
      onDragOver={onDragOver}
      onDrop={(e) => onDrop(e, stage.stage)}
    >
      <div className="column-header">
        <div className="column-title">
          <span className="stage-dot" style={{ backgroundColor: config.color }} />
          <span>{config.label}</span>
        </div>
        <span className="column-count">{stage.count}</span>
      </div>
      <div className="column-content">
        {stage.items.length === 0 ? (
          <div className="empty-column">
            <Text type="secondary">No families</Text>
          </div>
        ) : (
          stage.items.map((item) => (
            <KanbanCard
              key={item.applicantId}
              item={item}
              borderColor={config.color}
              onDragStart={onDragStart}
              onClick={onCardClick}
            />
          ))
        )}
      </div>
    </div>
  );
};

// Kanban Card Component
interface KanbanCardProps {
  item: PipelineItem;
  borderColor: string;
  onDragStart: (e: React.DragEvent, item: PipelineItem) => void;
  onClick: (applicantId: string) => void;
}

const KanbanCard = ({ item, borderColor, onDragStart, onClick }: KanbanCardProps) => {
  const getBoardDecisionStyle = (decision: string) => {
    const styles: Record<string, { bg: string; color: string }> = {
      Pending: { bg: colors.status.pendingBg, color: '#92400e' },
      Approved: { bg: colors.status.approvedBg, color: '#065f46' },
      Rejected: { bg: colors.status.rejectedBg, color: '#991b1b' },
      Deferred: { bg: colors.status.deferredBg, color: '#3730a3' },
    };
    return styles[decision] || styles.Pending;
  };

  const decisionStyle = getBoardDecisionStyle(item.boardDecision);

  return (
    <div
      className="kanban-card"
      style={{ borderLeftColor: borderColor }}
      draggable
      onDragStart={(e) => onDragStart(e, item)}
      onClick={() => onClick(item.applicantId)}
    >
      <div className="card-header">
        <Text strong className="family-name">{item.familyName}</Text>
        <span
          className="board-badge"
          style={{ backgroundColor: decisionStyle.bg, color: decisionStyle.color }}
        >
          {item.boardDecision}
        </span>
      </div>
      <div className="card-subtitle">
        <UserOutlined style={{ fontSize: 12, color: colors.neutral[400] }} />
        <Text type="secondary" style={{ fontSize: 13 }}>
          {item.husbandFirstName}
        </Text>
      </div>
      <div className="card-details">
        {item.childrenCount > 0 && (
          <span className="detail-item">
            {item.childrenCount} {item.childrenCount === 1 ? 'child' : 'children'}
          </span>
        )}
        {item.budget && (
          <span className="detail-item">
            ${(item.budget / 1000).toFixed(0)}k budget
          </span>
        )}
      </div>
      {item.preferredCities && item.preferredCities.length > 0 && (
        <div className="card-cities">
          {item.preferredCities.slice(0, 2).map((city) => (
            <span key={city} className="city-tag">{city}</span>
          ))}
          {item.preferredCities.length > 2 && (
            <span className="city-tag">+{item.preferredCities.length - 2}</span>
          )}
        </div>
      )}
      <div className="card-footer">
        <Text type="secondary" style={{ fontSize: 12 }}>
          {item.daysInStage} {item.daysInStage === 1 ? 'day' : 'days'} in stage
        </Text>
      </div>
    </div>
  );
};

export default PipelinePage;
