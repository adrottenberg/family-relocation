import { useState } from 'react';
import { Steps, Button, Modal, Input, message, Tooltip, Space, Tag } from 'antd';
import {
  CheckCircleOutlined,
  FileTextOutlined,
  SearchOutlined,
  FileProtectOutlined,
  HomeOutlined,
  PauseCircleOutlined,
  PlayCircleOutlined,
  ExclamationCircleOutlined,
  RightOutlined,
} from '@ant-design/icons';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { housingSearchesApi } from '../../api/endpoints/housingSearches';
import {
  validateTransition,
  formatStage,
  getPipelineStage,
  type PipelineStage,
  type TransitionType,
} from '../pipeline/transitionRules';
import './StageTimeline.css';

const { TextArea } = Input;

interface StageTimelineProps {
  applicantId: string;
  housingSearchId?: string;
  boardDecision: string;
  currentStage: string;
  onTransitionModalOpen: (type: TransitionType) => void;
  onBoardDecisionClick: () => void;
  canApproveBoardDecisions?: boolean;
}

// Stage configuration for the visual timeline
const stageConfig: {
  key: PipelineStage;
  label: string;
  icon: React.ReactNode;
}[] = [
  { key: 'AwaitingAgreements', label: 'Agreements', icon: <FileTextOutlined /> },
  { key: 'Searching', label: 'Searching', icon: <SearchOutlined /> },
  { key: 'UnderContract', label: 'Under Contract', icon: <FileProtectOutlined /> },
  { key: 'Closed', label: 'Closed', icon: <HomeOutlined /> },
];

// Action button labels for each transition
const actionLabels: Record<string, string> = {
  needsAgreements: 'Mark Agreements Complete',
  needsContractInfo: 'Enter Contract Details',
  needsClosingInfo: 'Confirm Closing',
  contractFailed: 'Contract Fell Through',
};

const StageTimeline = ({
  applicantId,
  housingSearchId,
  boardDecision,
  currentStage,
  onTransitionModalOpen,
  onBoardDecisionClick,
  canApproveBoardDecisions = false,
}: StageTimelineProps) => {
  const queryClient = useQueryClient();
  const [pauseModalOpen, setPauseModalOpen] = useState(false);
  const [pauseReason, setPauseReason] = useState('');

  // Get the current pipeline stage
  const pipelineStage = getPipelineStage(boardDecision, currentStage);
  const isPaused = currentStage === 'Paused';

  // Find current step index for the Steps component
  const getCurrentStepIndex = () => {
    if (!pipelineStage || pipelineStage === 'Submitted') return -1;
    return stageConfig.findIndex((s) => s.key === pipelineStage);
  };

  // Pause mutation
  const pauseMutation = useMutation({
    mutationFn: (reason: string) =>
      housingSearchesApi.changeStage(housingSearchId!, {
        newStage: 'Paused',
        reason: reason || undefined,
      }),
    onSuccess: () => {
      message.success('Housing search paused');
      queryClient.invalidateQueries({ queryKey: ['applicant', applicantId] });
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
      setPauseModalOpen(false);
      setPauseReason('');
    },
    onError: () => {
      message.error('Failed to pause housing search');
    },
  });

  // Resume mutation
  const resumeMutation = useMutation({
    mutationFn: () =>
      housingSearchesApi.changeStage(housingSearchId!, {
        newStage: 'Searching',
      }),
    onSuccess: () => {
      message.success('Housing search resumed');
      queryClient.invalidateQueries({ queryKey: ['applicant', applicantId] });
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
    },
    onError: () => {
      message.error('Failed to resume housing search');
    },
  });

  // Handle next action click
  const handleNextAction = () => {
    if (!pipelineStage || !nextStage) return;

    const transition = validateTransition(pipelineStage, nextStage.key, {
      boardDecision,
    });

    switch (transition.type) {
      case 'needsAgreements':
      case 'needsContractInfo':
      case 'needsClosingInfo':
      case 'contractFailed':
        onTransitionModalOpen(transition.type);
        break;
      default:
        break;
    }
  };

  // Get step status for the Steps component
  const getStepStatus = (index: number): 'finish' | 'process' | 'wait' | 'error' => {
    const currentIndex = getCurrentStepIndex();

    if (index < currentIndex) return 'finish';
    if (index === currentIndex) {
      if (isPaused) return 'error';
      return 'process';
    }
    return 'wait';
  };

  // If in Submitted stage (no housing search or pending board approval)
  if (!pipelineStage || pipelineStage === 'Submitted') {
    return (
      <div className="stage-timeline stage-timeline-submitted">
        <div className="submitted-message">
          <ExclamationCircleOutlined style={{ fontSize: 20, marginRight: 12, color: '#faad14' }} />
          <span>Awaiting board approval to begin housing search</span>
          {boardDecision === 'Pending' && canApproveBoardDecisions && (
            <Button
              type="primary"
              onClick={onBoardDecisionClick}
              style={{ marginLeft: 16 }}
            >
              Record Board Decision
            </Button>
          )}
        </div>
      </div>
    );
  }

  const currentIndex = getCurrentStepIndex();

  // Find the next stage and its transition type
  const nextStageIndex = currentIndex + 1;
  const nextStage = nextStageIndex < stageConfig.length ? stageConfig[nextStageIndex] : null;
  const nextTransition = nextStage && !isPaused
    ? validateTransition(pipelineStage, nextStage.key, { boardDecision })
    : null;
  const canAdvance = nextTransition && nextTransition.type !== 'blocked';

  return (
    <div className="stage-timeline">
      <Steps
        current={currentIndex}
        size="small"
        className={`stage-steps ${isPaused ? 'stage-steps-paused' : ''}`}
        items={stageConfig.map((stage, index) => {
          const status = getStepStatus(index);

          return {
            title: stage.label,
            icon: status === 'finish' ? <CheckCircleOutlined /> : stage.icon,
            status,
          };
        })}
      />

      {/* Action bar below the timeline */}
      <div className="stage-action-bar">
        {/* Left side: Pause/Resume for Searching stage */}
        <div className="stage-action-left">
          {pipelineStage === 'Searching' && housingSearchId && (
            isPaused ? (
              <Space>
                <Tag color="orange" icon={<PauseCircleOutlined />}>
                  Paused
                </Tag>
                <Button
                  size="small"
                  icon={<PlayCircleOutlined />}
                  onClick={() => resumeMutation.mutate()}
                  loading={resumeMutation.isPending}
                >
                  Resume
                </Button>
              </Space>
            ) : (
              <Tooltip title="Temporarily pause the housing search">
                <Button
                  size="small"
                  icon={<PauseCircleOutlined />}
                  onClick={() => setPauseModalOpen(true)}
                >
                  Pause
                </Button>
              </Tooltip>
            )
          )}
        </div>

        {/* Right side: Next action button */}
        <div className="stage-action-right">
          {canAdvance && nextStage && nextTransition && (
            <Button
              type="primary"
              icon={<RightOutlined />}
              onClick={handleNextAction}
            >
              {actionLabels[nextTransition.type] || `Move to ${formatStage(nextStage.key)}`}
            </Button>
          )}
          {pipelineStage === 'Closed' && (
            <Tag color="green" icon={<CheckCircleOutlined />}>
              Complete
            </Tag>
          )}
        </div>
      </div>

      {/* Pause Modal */}
      <Modal
        title={
          <Space>
            <ExclamationCircleOutlined style={{ color: '#faad14' }} />
            Pause Housing Search
          </Space>
        }
        open={pauseModalOpen}
        onCancel={() => {
          setPauseModalOpen(false);
          setPauseReason('');
        }}
        onOk={() => pauseMutation.mutate(pauseReason)}
        okText="Pause Search"
        okButtonProps={{
          loading: pauseMutation.isPending,
          icon: <PauseCircleOutlined />,
        }}
        cancelButtonProps={{ disabled: pauseMutation.isPending }}
      >
        <p>
          Pausing the housing search will temporarily suspend all activities. You can resume at any time.
        </p>
        <TextArea
          placeholder="Reason for pausing (optional)"
          value={pauseReason}
          onChange={(e) => setPauseReason(e.target.value)}
          rows={3}
          style={{ marginTop: 16 }}
        />
      </Modal>
    </div>
  );
};

export default StageTimeline;
