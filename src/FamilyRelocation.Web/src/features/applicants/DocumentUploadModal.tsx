import { useState } from 'react';
import { Modal, Upload, Button, message, Space, Card, Tag, Progress, Tooltip } from 'antd';
import {
  UploadOutlined,
  FileTextOutlined,
  CheckCircleOutlined,
  CloudUploadOutlined,
  EyeOutlined,
  SwapOutlined,
  ArrowRightOutlined,
} from '@ant-design/icons';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { documentsApi, applicantsApi } from '../../api';
import type { ApplicantDto } from '../../api/types';
import type { UploadFile } from 'antd/es/upload/interface';

interface DocumentUploadModalProps {
  open: boolean;
  onClose: () => void;
  applicant: ApplicantDto;
}

interface UploadState {
  brokerFile: UploadFile | null;
  takanosFile: UploadFile | null;
  brokerUploading: boolean;
  takanosUploading: boolean;
  brokerProgress: number;
  takanosProgress: number;
  brokerReplaceMode: boolean;
  takanosReplaceMode: boolean;
}

const DocumentUploadModal = ({ open, onClose, applicant }: DocumentUploadModalProps) => {
  const queryClient = useQueryClient();
  const housingSearch = applicant.housingSearch;
  const brokerSigned = housingSearch?.brokerAgreementSigned || false;
  const takanosSigned = housingSearch?.communityTakanosSigned || false;
  const brokerDocUrl = housingSearch?.brokerAgreementDocumentUrl;
  const takanosDocUrl = housingSearch?.communityTakanosDocumentUrl;

  const [uploadState, setUploadState] = useState<UploadState>({
    brokerFile: null,
    takanosFile: null,
    brokerUploading: false,
    takanosUploading: false,
    brokerProgress: 0,
    takanosProgress: 0,
    brokerReplaceMode: false,
    takanosReplaceMode: false,
  });

  const uploadBrokerMutation = useMutation({
    mutationFn: async (file: File) => {
      setUploadState((s) => ({ ...s, brokerUploading: true, brokerProgress: 30 }));

      // Upload to S3
      const result = await documentsApi.upload(file, applicant.id, 'BrokerAgreement');
      setUploadState((s) => ({ ...s, brokerProgress: 70 }));

      // Record the agreement
      await applicantsApi.recordBrokerAgreement(applicant.id, result.documentUrl);
      setUploadState((s) => ({ ...s, brokerProgress: 100 }));

      return result;
    },
    onSuccess: () => {
      message.success('Broker Agreement uploaded successfully');
      queryClient.invalidateQueries({ queryKey: ['applicant', applicant.id] });
      setUploadState((s) => ({ ...s, brokerUploading: false, brokerFile: null, brokerReplaceMode: false }));
    },
    onError: () => {
      message.error('Failed to upload Broker Agreement');
      setUploadState((s) => ({ ...s, brokerUploading: false, brokerProgress: 0 }));
    },
  });

  const uploadTakanosMutation = useMutation({
    mutationFn: async (file: File) => {
      setUploadState((s) => ({ ...s, takanosUploading: true, takanosProgress: 30 }));

      // Upload to S3
      const result = await documentsApi.upload(file, applicant.id, 'CommunityTakanos');
      setUploadState((s) => ({ ...s, takanosProgress: 70 }));

      // Record the agreement
      await applicantsApi.recordCommunityTakanos(applicant.id, result.documentUrl);
      setUploadState((s) => ({ ...s, takanosProgress: 100 }));

      return result;
    },
    onSuccess: () => {
      message.success('Community Takanos uploaded successfully');
      queryClient.invalidateQueries({ queryKey: ['applicant', applicant.id] });
      setUploadState((s) => ({ ...s, takanosUploading: false, takanosFile: null, takanosReplaceMode: false }));
    },
    onError: () => {
      message.error('Failed to upload Community Takanos');
      setUploadState((s) => ({ ...s, takanosUploading: false, takanosProgress: 0 }));
    },
  });

  const startHouseHuntingMutation = useMutation({
    mutationFn: () => applicantsApi.startHouseHunting(applicant.id),
    onSuccess: () => {
      message.success('House hunting started!');
      queryClient.invalidateQueries({ queryKey: ['applicant', applicant.id] });
      queryClient.invalidateQueries({ queryKey: ['applicants'] });
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
      onClose();
    },
    onError: () => {
      message.error('Failed to start house hunting');
    },
  });

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

  const handleViewDocument = async (documentUrl: string) => {
    try {
      // Extract the S3 key from the URL - the key is the path after the bucket name
      const url = new URL(documentUrl);
      const key = url.pathname.substring(1); // Remove leading slash

      const result = await documentsApi.getPresignedUrl(key);
      window.open(result.url, '_blank');
    } catch {
      message.error('Failed to get document URL');
    }
  };

  const bothSigned = brokerSigned && takanosSigned;
  const canStartHouseHunting = bothSigned && housingSearch?.stage === 'BoardApproved';

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

  const renderBrokerCard = () => {
    const showUploadUI = !brokerSigned || uploadState.brokerReplaceMode;

    return (
      <Card
        size="small"
        title={
          <Space>
            <FileTextOutlined />
            Broker Agreement
            {brokerSigned && <Tag color="success">Signed</Tag>}
          </Space>
        }
      >
        {brokerSigned && !uploadState.brokerReplaceMode ? (
          <Space direction="vertical" style={{ width: '100%' }}>
            <div style={{ color: '#52c41a', display: 'flex', alignItems: 'center', gap: 8 }}>
              <CheckCircleOutlined /> Agreement has been uploaded
            </div>
            <Space>
              {brokerDocUrl && (
                <Tooltip title="View uploaded document">
                  <Button
                    icon={<EyeOutlined />}
                    onClick={() => handleViewDocument(brokerDocUrl)}
                  >
                    View
                  </Button>
                </Tooltip>
              )}
              <Tooltip title="Upload a new version">
                <Button
                  icon={<SwapOutlined />}
                  onClick={() => setUploadState((s) => ({ ...s, brokerReplaceMode: true }))}
                >
                  Replace
                </Button>
              </Tooltip>
            </Space>
          </Space>
        ) : (
          <Space direction="vertical" style={{ width: '100%' }}>
            {uploadState.brokerReplaceMode && (
              <div style={{ marginBottom: 8, color: '#666' }}>
                Upload a new document to replace the existing one.
                <Button
                  type="link"
                  size="small"
                  onClick={() => setUploadState((s) => ({ ...s, brokerReplaceMode: false, brokerFile: null }))}
                >
                  Cancel
                </Button>
              </div>
            )}
            <Upload
              beforeUpload={(file) => {
                const result = beforeUploadValidation(file);
                if (result !== Upload.LIST_IGNORE) {
                  setUploadState((s) => ({ ...s, brokerFile: file as unknown as UploadFile }));
                }
                return result;
              }}
              fileList={uploadState.brokerFile ? [uploadState.brokerFile] : []}
              onRemove={() => setUploadState((s) => ({ ...s, brokerFile: null }))}
              maxCount={1}
            >
              <Button icon={<UploadOutlined />}>Select File</Button>
            </Upload>

            {uploadState.brokerUploading && (
              <Progress percent={uploadState.brokerProgress} size="small" />
            )}

            {uploadState.brokerFile && !uploadState.brokerUploading && (
              <Button
                type="primary"
                icon={<CloudUploadOutlined />}
                onClick={handleBrokerUpload}
                loading={uploadState.brokerUploading}
              >
                {uploadState.brokerReplaceMode ? 'Replace' : 'Upload'} Broker Agreement
              </Button>
            )}
          </Space>
        )}
      </Card>
    );
  };

  const renderTakanosCard = () => {
    const showUploadUI = !takanosSigned || uploadState.takanosReplaceMode;

    return (
      <Card
        size="small"
        title={
          <Space>
            <FileTextOutlined />
            Community Takanos
            {takanosSigned && <Tag color="success">Signed</Tag>}
          </Space>
        }
      >
        {takanosSigned && !uploadState.takanosReplaceMode ? (
          <Space direction="vertical" style={{ width: '100%' }}>
            <div style={{ color: '#52c41a', display: 'flex', alignItems: 'center', gap: 8 }}>
              <CheckCircleOutlined /> Agreement has been uploaded
            </div>
            <Space>
              {takanosDocUrl && (
                <Tooltip title="View uploaded document">
                  <Button
                    icon={<EyeOutlined />}
                    onClick={() => handleViewDocument(takanosDocUrl)}
                  >
                    View
                  </Button>
                </Tooltip>
              )}
              <Tooltip title="Upload a new version">
                <Button
                  icon={<SwapOutlined />}
                  onClick={() => setUploadState((s) => ({ ...s, takanosReplaceMode: true }))}
                >
                  Replace
                </Button>
              </Tooltip>
            </Space>
          </Space>
        ) : (
          <Space direction="vertical" style={{ width: '100%' }}>
            {uploadState.takanosReplaceMode && (
              <div style={{ marginBottom: 8, color: '#666' }}>
                Upload a new document to replace the existing one.
                <Button
                  type="link"
                  size="small"
                  onClick={() => setUploadState((s) => ({ ...s, takanosReplaceMode: false, takanosFile: null }))}
                >
                  Cancel
                </Button>
              </div>
            )}
            <Upload
              beforeUpload={(file) => {
                const result = beforeUploadValidation(file);
                if (result !== Upload.LIST_IGNORE) {
                  setUploadState((s) => ({ ...s, takanosFile: file as unknown as UploadFile }));
                }
                return result;
              }}
              fileList={uploadState.takanosFile ? [uploadState.takanosFile] : []}
              onRemove={() => setUploadState((s) => ({ ...s, takanosFile: null }))}
              maxCount={1}
            >
              <Button icon={<UploadOutlined />}>Select File</Button>
            </Upload>

            {uploadState.takanosUploading && (
              <Progress percent={uploadState.takanosProgress} size="small" />
            )}

            {uploadState.takanosFile && !uploadState.takanosUploading && (
              <Button
                type="primary"
                icon={<CloudUploadOutlined />}
                onClick={handleTakanosUpload}
                loading={uploadState.takanosUploading}
              >
                {uploadState.takanosReplaceMode ? 'Replace' : 'Upload'} Community Takanos
              </Button>
            )}
          </Space>
        )}
      </Card>
    );
  };

  return (
    <Modal
      title="Upload Signed Agreements"
      open={open}
      onCancel={onClose}
      footer={[
        <Button key="close" onClick={onClose}>
          {bothSigned ? 'Done' : 'Close'}
        </Button>,
        canStartHouseHunting && (
          <Button
            key="start-hunting"
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
        <strong>{applicant.husband.lastName} Family</strong> - Upload signed agreements to proceed with house hunting.
      </div>

      <Space direction="vertical" style={{ width: '100%' }} size="middle">
        {renderBrokerCard()}
        {renderTakanosCard()}

        {/* Status summary */}
        {bothSigned && (
          <div
            style={{
              padding: 12,
              background: canStartHouseHunting ? '#f6ffed' : '#fff7e6',
              border: `1px solid ${canStartHouseHunting ? '#b7eb8f' : '#ffd591'}`,
              borderRadius: 6,
              textAlign: 'center',
            }}
          >
            <CheckCircleOutlined style={{ color: canStartHouseHunting ? '#52c41a' : '#faad14', marginRight: 8 }} />
            {canStartHouseHunting
              ? 'All agreements signed! Click "Move to House Hunting" to proceed.'
              : 'All agreements signed! Ready to start house hunting.'}
          </div>
        )}
      </Space>
    </Modal>
  );
};

export default DocumentUploadModal;
