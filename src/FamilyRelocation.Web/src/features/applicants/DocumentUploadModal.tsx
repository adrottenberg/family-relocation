import { useState } from 'react';
import { Modal, Upload, Button, message, Space, Card, Tag, Progress } from 'antd';
import {
  UploadOutlined,
  FileTextOutlined,
  CheckCircleOutlined,
  CloudUploadOutlined,
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
}

const DocumentUploadModal = ({ open, onClose, applicant }: DocumentUploadModalProps) => {
  const queryClient = useQueryClient();
  const housingSearch = applicant.housingSearch;
  const brokerSigned = housingSearch?.brokerAgreementSigned || false;
  const takanosSigned = housingSearch?.communityTakanosSigned || false;

  const [uploadState, setUploadState] = useState<UploadState>({
    brokerFile: null,
    takanosFile: null,
    brokerUploading: false,
    takanosUploading: false,
    brokerProgress: 0,
    takanosProgress: 0,
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
      setUploadState((s) => ({ ...s, takanosUploading: false, takanosFile: null }));
    },
    onError: () => {
      message.error('Failed to upload Community Takanos');
      setUploadState((s) => ({ ...s, takanosUploading: false, takanosProgress: 0 }));
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

  const bothSigned = brokerSigned && takanosSigned;

  return (
    <Modal
      title="Upload Signed Agreements"
      open={open}
      onCancel={onClose}
      footer={[
        <Button key="close" onClick={onClose}>
          {bothSigned ? 'Done' : 'Close'}
        </Button>,
      ]}
      width={520}
    >
      <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 6 }}>
        <strong>{applicant.husband.lastName} Family</strong> - Upload signed agreements to proceed with house hunting.
      </div>

      <Space direction="vertical" style={{ width: '100%' }} size="middle">
        {/* Broker Agreement */}
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
          {brokerSigned ? (
            <div style={{ color: '#52c41a', display: 'flex', alignItems: 'center', gap: 8 }}>
              <CheckCircleOutlined /> Agreement has been uploaded
            </div>
          ) : (
            <Space direction="vertical" style={{ width: '100%' }}>
              <Upload
                beforeUpload={(file) => {
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
                  setUploadState((s) => ({ ...s, brokerFile: file as unknown as UploadFile }));
                  return false;
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
                  Upload Broker Agreement
                </Button>
              )}
            </Space>
          )}
        </Card>

        {/* Community Takanos */}
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
          {takanosSigned ? (
            <div style={{ color: '#52c41a', display: 'flex', alignItems: 'center', gap: 8 }}>
              <CheckCircleOutlined /> Agreement has been uploaded
            </div>
          ) : (
            <Space direction="vertical" style={{ width: '100%' }}>
              <Upload
                beforeUpload={(file) => {
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
                  setUploadState((s) => ({ ...s, takanosFile: file as unknown as UploadFile }));
                  return false;
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
                  Upload Community Takanos
                </Button>
              )}
            </Space>
          )}
        </Card>

        {/* Status summary */}
        {bothSigned && (
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
            All agreements signed! Ready to start house hunting.
          </div>
        )}
      </Space>
    </Modal>
  );
};

export default DocumentUploadModal;
