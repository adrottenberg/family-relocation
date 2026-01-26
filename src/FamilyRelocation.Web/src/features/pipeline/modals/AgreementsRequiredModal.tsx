import { useState, useEffect, useRef } from 'react';
import { Modal, Button, List, Tag, Upload, Progress, Space, message, Divider, Spin, Empty } from 'antd';
import {
  FileTextOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  UploadOutlined,
  CloudUploadOutlined,
  ArrowRightOutlined,
} from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { documentsApi, housingSearchesApi, getStageRequirements } from '../../../api';
import type { DocumentRequirementDto } from '../../../api/types';
import type { UploadFile } from 'antd/es/upload/interface';

interface AgreementsRequiredModalProps {
  open: boolean;
  onClose: () => void;
  applicantId: string;
  housingSearchId: string;
  familyName: string;
  fromStage: string;
  toStage: string;
  onTransitionComplete?: () => void;
}

interface DocumentUploadState {
  file: UploadFile | null;
  uploading: boolean;
  progress: number;
}

const AgreementsRequiredModal = ({
  open,
  onClose,
  applicantId,
  housingSearchId,
  familyName,
  fromStage,
  toStage,
  onTransitionComplete,
}: AgreementsRequiredModalProps) => {
  const queryClient = useQueryClient();

  // Track upload state per document type
  const [uploadStates, setUploadStates] = useState<Record<string, DocumentUploadState>>({});

  // Track local uploaded state that updates after uploads
  const [localUploadedStatus, setLocalUploadedStatus] = useState<Record<string, boolean>>({});

  // Fetch stage requirements with applicant's document status
  const { data: requirements, isLoading } = useQuery({
    queryKey: ['stageRequirements', fromStage, toStage, applicantId],
    queryFn: () => getStageRequirements(fromStage, toStage, applicantId),
    enabled: open,
  });

  // Initialize states when requirements load
  useEffect(() => {
    if (requirements?.requirements) {
      const initialUploadStates: Record<string, DocumentUploadState> = {};
      const initialUploadedStatus: Record<string, boolean> = {};

      requirements.requirements.forEach((req) => {
        initialUploadStates[req.documentTypeId] = {
          file: null,
          uploading: false,
          progress: 0,
        };
        initialUploadedStatus[req.documentTypeId] = req.isUploaded;
      });

      setUploadStates(initialUploadStates);
      setLocalUploadedStatus(initialUploadedStatus);
    }
  }, [requirements]);

  const uploadMutation = useMutation({
    mutationFn: async ({ file, documentTypeId }: { file: File; documentTypeId: string }) => {
      setUploadStates((prev) => ({
        ...prev,
        [documentTypeId]: { ...prev[documentTypeId], uploading: true, progress: 30 },
      }));

      const result = await documentsApi.upload(file, applicantId, documentTypeId);
      setUploadStates((prev) => ({
        ...prev,
        [documentTypeId]: { ...prev[documentTypeId], progress: 100 },
      }));

      return { result, documentTypeId };
    },
    onSuccess: ({ documentTypeId }) => {
      const req = requirements?.requirements.find((r) => r.documentTypeId === documentTypeId);
      message.success(`${req?.documentTypeName || 'Document'} uploaded successfully`);
      setLocalUploadedStatus((prev) => ({ ...prev, [documentTypeId]: true }));
      queryClient.invalidateQueries({ queryKey: ['applicant', applicantId] });
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
      setUploadStates((prev) => ({
        ...prev,
        [documentTypeId]: { file: null, uploading: false, progress: 0 },
      }));
    },
    onError: (_, { documentTypeId }) => {
      const req = requirements?.requirements.find((r) => r.documentTypeId === documentTypeId);
      message.error(`Failed to upload ${req?.documentTypeName || 'document'}`);
      setUploadStates((prev) => ({
        ...prev,
        [documentTypeId]: { ...prev[documentTypeId], uploading: false, progress: 0 },
      }));
    },
  });

  const transitionMutation = useMutation({
    mutationFn: () => housingSearchesApi.changeStage(housingSearchId, { newStage: toStage }),
    onSuccess: () => {
      message.success(`Moved to ${toStage}!`);
      queryClient.invalidateQueries({ queryKey: ['applicant', applicantId] });
      queryClient.invalidateQueries({ queryKey: ['applicants'] });
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
      onTransitionComplete?.();
      onClose();
    },
    onError: () => {
      message.error(`Failed to transition to ${toStage}`);
    },
  });

  const beforeUploadValidation = (file: File) => {
    const isValid =
      file.type === 'application/pdf' ||
      file.type === 'image/jpeg' ||
      file.type === 'image/png';
    if (!isValid) {
      message.error('Only PDF, JPEG, and PNG files are allowed');
      return Upload.LIST_IGNORE;
    }
    const isLt10M = file.size / 1024 / 1024 < 10;
    if (!isLt10M) {
      message.error('File must be smaller than 10MB');
      return Upload.LIST_IGNORE;
    }
    return false;
  };

  const handleUpload = (documentTypeId: string) => {
    const state = uploadStates[documentTypeId];
    if (state?.file) {
      uploadMutation.mutate({
        file: state.file as unknown as File,
        documentTypeId,
      });
    }
  };

  const setUploadState = (documentTypeId: string, updates: Partial<DocumentUploadState>) => {
    setUploadStates((prev) => ({
      ...prev,
      [documentTypeId]: { ...prev[documentTypeId], ...updates },
    }));
  };

  // Check if all required documents are uploaded
  const allRequiredUploaded =
    requirements?.requirements.every(
      (req) => !req.isRequired || localUploadedStatus[req.documentTypeId]
    ) ?? false;

  // Auto-transition if modal is opened and all required documents are already uploaded
  // (or if there are no requirements at all)
  const hasAutoTransitioned = useRef(false);

  useEffect(() => {
    if (
      open &&
      requirements &&
      !isLoading &&
      allRequiredUploaded &&
      !hasAutoTransitioned.current &&
      !transitionMutation.isPending
    ) {
      hasAutoTransitioned.current = true;
      transitionMutation.mutate();
    }
  }, [open, requirements, isLoading, allRequiredUploaded, transitionMutation]);

  // Reset auto-transition flag when modal closes
  useEffect(() => {
    if (!open) {
      hasAutoTransitioned.current = false;
    }
  }, [open]);

  const renderRequirementItem = (req: DocumentRequirementDto) => {
    const isUploaded = localUploadedStatus[req.documentTypeId];
    const state = uploadStates[req.documentTypeId] || {
      file: null,
      uploading: false,
      progress: 0,
    };

    return (
      <List.Item style={{ display: 'block' }}>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            marginBottom: 8,
          }}
        >
          <Space>
            {isUploaded ? (
              <CheckCircleOutlined style={{ color: '#52c41a', fontSize: 18 }} />
            ) : (
              <CloseCircleOutlined style={{ color: '#ff4d4f', fontSize: 18 }} />
            )}
            <span style={{ fontWeight: 500 }}>{req.documentTypeName}</span>
            {req.isRequired && <Tag color="red">Required</Tag>}
          </Space>
          <Tag color={isUploaded ? 'success' : 'error'}>
            {isUploaded ? 'Uploaded' : 'Not Uploaded'}
          </Tag>
        </div>

        {!isUploaded && (
          <div style={{ marginLeft: 26 }}>
            <Space direction="vertical" style={{ width: '100%' }} size="small">
              <Upload
                accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png"
                beforeUpload={(file) => {
                  const result = beforeUploadValidation(file);
                  if (result !== Upload.LIST_IGNORE) {
                    setUploadState(req.documentTypeId, {
                      file: file as unknown as UploadFile,
                    });
                  }
                  return result;
                }}
                fileList={state.file ? [state.file] : []}
                onRemove={() => setUploadState(req.documentTypeId, { file: null })}
                maxCount={1}
              >
                <Button type="primary" icon={<UploadOutlined />} size="small">
                  Select File (PDF, JPEG, PNG)
                </Button>
              </Upload>

              {state.uploading && <Progress percent={state.progress} size="small" />}

              {state.file && !state.uploading && (
                <Button
                  type="primary"
                  size="small"
                  icon={<CloudUploadOutlined />}
                  onClick={() => handleUpload(req.documentTypeId)}
                >
                  Upload {req.documentTypeName}
                </Button>
              )}
            </Space>
          </div>
        )}
      </List.Item>
    );
  };

  return (
    <Modal
      title={
        <Space>
          <FileTextOutlined style={{ color: '#1890ff' }} />
          Documents Required for Stage Transition
        </Space>
      }
      open={open}
      onCancel={onClose}
      footer={[
        <Button key="cancel" onClick={onClose}>
          Cancel
        </Button>,
        allRequiredUploaded && (
          <Button
            key="proceed"
            type="primary"
            icon={<ArrowRightOutlined />}
            onClick={() => transitionMutation.mutate()}
            loading={transitionMutation.isPending}
          >
            Move to {toStage}
          </Button>
        ),
      ].filter(Boolean)}
      width={520}
    >
      <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 6 }}>
        <strong>{familyName}</strong> needs the following documents before transitioning from{' '}
        <Tag>{fromStage}</Tag> to <Tag>{toStage}</Tag>.
      </div>

      {isLoading ? (
        <div style={{ textAlign: 'center', padding: 40 }}>
          <Spin />
        </div>
      ) : !requirements?.requirements || requirements.requirements.length === 0 ? (
        <Empty description="No document requirements for this transition" />
      ) : (
        <List dataSource={requirements.requirements} renderItem={renderRequirementItem} />
      )}

      {allRequiredUploaded && requirements?.requirements && requirements.requirements.length > 0 && (
        <>
          <Divider />
          <div
            style={{
              padding: 12,
              background: '#f6ffed',
              border: '1px solid #b7eb8f',
              borderRadius: 6,
              textAlign: 'center',
            }}
          >
            <CheckCircleOutlined style={{ color: '#52c41a', marginRight: 8 }} />
            All required documents uploaded! Click "Move to {toStage}" to proceed.
          </div>
        </>
      )}
    </Modal>
  );
};

export default AgreementsRequiredModal;
