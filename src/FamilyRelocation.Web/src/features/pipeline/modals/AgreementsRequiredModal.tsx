import { useState } from 'react';
import { Modal, Button, List, Tag, Upload, Progress, Space, message, Divider } from 'antd';
import {
  FileTextOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  UploadOutlined,
  CloudUploadOutlined,
  ArrowRightOutlined,
} from '@ant-design/icons';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { documentsApi, applicantsApi } from '../../../api';
import type { UploadFile } from 'antd/es/upload/interface';

interface AgreementsRequiredModalProps {
  open: boolean;
  onClose: () => void;
  applicantId: string;
  familyName: string;
  brokerAgreementSigned: boolean;
  communityTakanosSigned: boolean;
  onTransitionComplete?: () => void;
}

interface UploadState {
  brokerFile: UploadFile | null;
  takanosFile: UploadFile | null;
  brokerUploading: boolean;
  takanosUploading: boolean;
  brokerProgress: number;
  takanosProgress: number;
}

const AgreementsRequiredModal = ({
  open,
  onClose,
  applicantId,
  familyName,
  brokerAgreementSigned: initialBrokerSigned,
  communityTakanosSigned: initialTakanosSigned,
  onTransitionComplete,
}: AgreementsRequiredModalProps) => {
  const queryClient = useQueryClient();

  // Track local signed state that updates after uploads
  const [brokerSigned, setBrokerSigned] = useState(initialBrokerSigned);
  const [takanosSigned, setTakanosSigned] = useState(initialTakanosSigned);

  const [uploadState, setUploadState] = useState<UploadState>({
    brokerFile: null,
    takanosFile: null,
    brokerUploading: false,
    takanosUploading: false,
    brokerProgress: 0,
    takanosProgress: 0,
  });

  // Reset state when modal opens with new props
  useState(() => {
    setBrokerSigned(initialBrokerSigned);
    setTakanosSigned(initialTakanosSigned);
  });

  const uploadBrokerMutation = useMutation({
    mutationFn: async (file: File) => {
      setUploadState((s) => ({ ...s, brokerUploading: true, brokerProgress: 30 }));
      const result = await documentsApi.upload(file, applicantId, 'BrokerAgreement');
      setUploadState((s) => ({ ...s, brokerProgress: 70 }));
      await applicantsApi.recordBrokerAgreement(applicantId, result.documentUrl);
      setUploadState((s) => ({ ...s, brokerProgress: 100 }));
      return result;
    },
    onSuccess: () => {
      message.success('Broker Agreement uploaded successfully');
      setBrokerSigned(true);
      queryClient.invalidateQueries({ queryKey: ['applicant', applicantId] });
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
      setUploadState((s) => ({ ...s, brokerUploading: false, brokerFile: null }));
    },
    onError: () => {
      message.error('Failed to upload Broker Agreement');
      setUploadState((s) => ({ ...s, brokerUploading: false, brokerProgress: 0 }));
    },
  });

  const uploadTakanosMutation = useMutation({
    mutationFn: async (file: File) => {
      setUploadState((s) => ({ ...s, takanosUploading: true, takanosProgress: 30 }));
      const result = await documentsApi.upload(file, applicantId, 'CommunityTakanos');
      setUploadState((s) => ({ ...s, takanosProgress: 70 }));
      await applicantsApi.recordCommunityTakanos(applicantId, result.documentUrl);
      setUploadState((s) => ({ ...s, takanosProgress: 100 }));
      return result;
    },
    onSuccess: () => {
      message.success('Community Takanos uploaded successfully');
      setTakanosSigned(true);
      queryClient.invalidateQueries({ queryKey: ['applicant', applicantId] });
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
      setUploadState((s) => ({ ...s, takanosUploading: false, takanosFile: null }));
    },
    onError: () => {
      message.error('Failed to upload Community Takanos');
      setUploadState((s) => ({ ...s, takanosUploading: false, takanosProgress: 0 }));
    },
  });

  const startHouseHuntingMutation = useMutation({
    mutationFn: () => applicantsApi.startHouseHunting(applicantId),
    onSuccess: () => {
      message.success('Moved to House Hunting!');
      queryClient.invalidateQueries({ queryKey: ['applicant', applicantId] });
      queryClient.invalidateQueries({ queryKey: ['applicants'] });
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
      onTransitionComplete?.();
      onClose();
    },
    onError: () => {
      message.error('Failed to move to house hunting');
    },
  });

  const beforeUploadValidation = (file: File) => {
    const isValid = file.type === 'application/pdf' ||
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

  const handleBrokerUpload = () => {
    if (uploadState.brokerFile) {
      uploadBrokerMutation.mutate(uploadState.brokerFile as unknown as File);
    }
  };

  const handleTakanosUpload = () => {
    if (uploadState.takanosFile) {
      uploadTakanosMutation.mutate(uploadState.takanosFile as unknown as File);
    }
  };

  const bothSigned = brokerSigned && takanosSigned;

  const agreements = [
    {
      key: 'broker',
      name: 'Broker Agreement',
      signed: brokerSigned,
      file: uploadState.brokerFile,
      uploading: uploadState.brokerUploading,
      progress: uploadState.brokerProgress,
      onFileSelect: (file: UploadFile) => setUploadState((s) => ({ ...s, brokerFile: file })),
      onFileRemove: () => setUploadState((s) => ({ ...s, brokerFile: null })),
      onUpload: handleBrokerUpload,
    },
    {
      key: 'takanos',
      name: 'Community Takanos',
      signed: takanosSigned,
      file: uploadState.takanosFile,
      uploading: uploadState.takanosUploading,
      progress: uploadState.takanosProgress,
      onFileSelect: (file: UploadFile) => setUploadState((s) => ({ ...s, takanosFile: file })),
      onFileRemove: () => setUploadState((s) => ({ ...s, takanosFile: null })),
      onUpload: handleTakanosUpload,
    },
  ];

  return (
    <Modal
      title={
        <Space>
          <FileTextOutlined style={{ color: '#1890ff' }} />
          Signed Agreements Required
        </Space>
      }
      open={open}
      onCancel={onClose}
      footer={[
        <Button key="cancel" onClick={onClose}>
          Cancel
        </Button>,
        bothSigned && (
          <Button
            key="proceed"
            type="primary"
            icon={<ArrowRightOutlined />}
            onClick={() => startHouseHuntingMutation.mutate()}
            loading={startHouseHuntingMutation.isPending}
          >
            Move to House Hunting
          </Button>
        ),
      ].filter(Boolean)}
      width={520}
    >
      <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 6 }}>
        <strong>{familyName} Family</strong> needs signed agreements before starting house hunting.
      </div>

      <List
        dataSource={agreements}
        renderItem={(item) => (
          <List.Item style={{ display: 'block' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
              <Space>
                {item.signed ? (
                  <CheckCircleOutlined style={{ color: '#52c41a', fontSize: 18 }} />
                ) : (
                  <CloseCircleOutlined style={{ color: '#ff4d4f', fontSize: 18 }} />
                )}
                <span style={{ fontWeight: 500 }}>{item.name}</span>
              </Space>
              <Tag color={item.signed ? 'success' : 'error'}>
                {item.signed ? 'Signed' : 'Not Signed'}
              </Tag>
            </div>

            {!item.signed && (
              <div style={{ marginLeft: 26 }}>
                <Space direction="vertical" style={{ width: '100%' }} size="small">
                  <Upload
                    beforeUpload={(file) => {
                      const result = beforeUploadValidation(file);
                      if (result !== Upload.LIST_IGNORE) {
                        item.onFileSelect(file as unknown as UploadFile);
                      }
                      return result;
                    }}
                    fileList={item.file ? [item.file] : []}
                    onRemove={() => item.onFileRemove()}
                    maxCount={1}
                  >
                    <Button icon={<UploadOutlined />} size="small">
                      Select File
                    </Button>
                  </Upload>

                  {item.uploading && (
                    <Progress percent={item.progress} size="small" />
                  )}

                  {item.file && !item.uploading && (
                    <Button
                      type="primary"
                      size="small"
                      icon={<CloudUploadOutlined />}
                      onClick={item.onUpload}
                    >
                      Upload {item.name}
                    </Button>
                  )}
                </Space>
              </div>
            )}
          </List.Item>
        )}
      />

      {bothSigned && (
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
            All agreements signed! Click "Move to House Hunting" to proceed.
          </div>
        </>
      )}
    </Modal>
  );
};

export default AgreementsRequiredModal;
