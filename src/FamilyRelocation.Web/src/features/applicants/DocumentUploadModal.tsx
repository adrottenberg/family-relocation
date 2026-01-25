import { useState, useEffect } from 'react';
import { Modal, Upload, Button, message, Space, Card, Tag, Progress, Tooltip, Spin, Empty } from 'antd';
import {
  UploadOutlined,
  FileTextOutlined,
  CheckCircleOutlined,
  CloudUploadOutlined,
  EyeOutlined,
  SwapOutlined,
} from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { documentsApi, getDocumentTypes } from '../../api';
import type { ApplicantDto, DocumentTypeDto, ApplicantDocumentDto } from '../../api/types';
import type { UploadFile } from 'antd/es/upload/interface';

interface DocumentUploadModalProps {
  open: boolean;
  onClose: () => void;
  applicant: ApplicantDto;
}

interface DocumentUploadState {
  file: UploadFile | null;
  uploading: boolean;
  progress: number;
  replaceMode: boolean;
}

const DocumentUploadModal = ({ open, onClose, applicant }: DocumentUploadModalProps) => {
  const queryClient = useQueryClient();

  // Track upload state per document type
  const [uploadStates, setUploadStates] = useState<Record<string, DocumentUploadState>>({});

  // Fetch document types
  const { data: documentTypes, isLoading: typesLoading } = useQuery({
    queryKey: ['documentTypes'],
    queryFn: () => getDocumentTypes(true),
    enabled: open,
  });

  // Fetch applicant's uploaded documents
  const { data: applicantDocuments, isLoading: docsLoading } = useQuery({
    queryKey: ['applicantDocuments', applicant.id],
    queryFn: () => documentsApi.getApplicantDocuments(applicant.id),
    enabled: open,
  });

  // Initialize upload states when document types load
  useEffect(() => {
    if (documentTypes) {
      const initialStates: Record<string, DocumentUploadState> = {};
      documentTypes.forEach((dt) => {
        if (!uploadStates[dt.id]) {
          initialStates[dt.id] = {
            file: null,
            uploading: false,
            progress: 0,
            replaceMode: false,
          };
        }
      });
      if (Object.keys(initialStates).length > 0) {
        setUploadStates((prev) => ({ ...prev, ...initialStates }));
      }
    }
  }, [documentTypes]);

  // Check if a document type is already uploaded
  const isDocumentUploaded = (documentTypeId: string): ApplicantDocumentDto | undefined => {
    return applicantDocuments?.find((doc) => doc.documentTypeId === documentTypeId);
  };

  // Get the number of required documents that are uploaded (for stage transition check)
  const getAllRequiredUploaded = (): boolean => {
    if (!documentTypes || !applicantDocuments) return false;
    // For now, check if all document types have been uploaded
    // In a full implementation, this would check against stage requirements
    return documentTypes.every((dt) => isDocumentUploaded(dt.id));
  };

  const uploadMutation = useMutation({
    mutationFn: async ({ file, documentTypeId }: { file: File; documentTypeId: string }) => {
      setUploadStates((prev) => ({
        ...prev,
        [documentTypeId]: { ...prev[documentTypeId], uploading: true, progress: 30 },
      }));

      // Upload to S3
      const result = await documentsApi.upload(file, applicant.id, documentTypeId);
      setUploadStates((prev) => ({
        ...prev,
        [documentTypeId]: { ...prev[documentTypeId], progress: 100 },
      }));

      return { result, documentTypeId };
    },
    onSuccess: ({ documentTypeId }) => {
      const docType = documentTypes?.find((dt) => dt.id === documentTypeId);
      message.success(`${docType?.displayName || 'Document'} uploaded successfully`);
      queryClient.invalidateQueries({ queryKey: ['applicant', applicant.id] });
      queryClient.invalidateQueries({ queryKey: ['applicantDocuments', applicant.id] });
      setUploadStates((prev) => ({
        ...prev,
        [documentTypeId]: { file: null, uploading: false, progress: 0, replaceMode: false },
      }));
    },
    onError: (_, { documentTypeId }) => {
      const docType = documentTypes?.find((dt) => dt.id === documentTypeId);
      message.error(`Failed to upload ${docType?.displayName || 'document'}`);
      setUploadStates((prev) => ({
        ...prev,
        [documentTypeId]: { ...prev[documentTypeId], uploading: false, progress: 0 },
      }));
    },
  });

  const handleUpload = (documentTypeId: string) => {
    const state = uploadStates[documentTypeId];
    if (state?.file) {
      uploadMutation.mutate({
        file: state.file as unknown as File,
        documentTypeId,
      });
    }
  };

  const handleViewDocument = async (storageKey: string) => {
    try {
      const result = await documentsApi.getPresignedUrl(storageKey);
      window.open(result.url, '_blank');
    } catch {
      message.error('Failed to get document URL');
    }
  };

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

  const setUploadState = (documentTypeId: string, updates: Partial<DocumentUploadState>) => {
    setUploadStates((prev) => ({
      ...prev,
      [documentTypeId]: { ...prev[documentTypeId], ...updates },
    }));
  };

  const allUploaded = getAllRequiredUploaded();

  const renderDocumentCard = (docType: DocumentTypeDto) => {
    const uploadedDoc = isDocumentUploaded(docType.id);
    const state = uploadStates[docType.id] || {
      file: null,
      uploading: false,
      progress: 0,
      replaceMode: false,
    };

    return (
      <Card
        key={docType.id}
        size="small"
        title={
          <Space>
            <FileTextOutlined />
            {docType.displayName}
            {uploadedDoc && <Tag color="success">Uploaded</Tag>}
          </Space>
        }
      >
        {uploadedDoc && !state.replaceMode ? (
          <Space direction="vertical" style={{ width: '100%' }}>
            <div style={{ color: '#52c41a', display: 'flex', alignItems: 'center', gap: 8 }}>
              <CheckCircleOutlined /> {uploadedDoc.fileName}
            </div>
            <div style={{ color: '#888', fontSize: 12 }}>
              Uploaded {new Date(uploadedDoc.uploadedAt).toLocaleDateString()}
            </div>
            <Space>
              <Tooltip title="View uploaded document">
                <Button
                  icon={<EyeOutlined />}
                  onClick={() => handleViewDocument(uploadedDoc.storageKey)}
                >
                  View
                </Button>
              </Tooltip>
              <Tooltip title="Upload a new version">
                <Button
                  icon={<SwapOutlined />}
                  onClick={() => setUploadState(docType.id, { replaceMode: true })}
                >
                  Replace
                </Button>
              </Tooltip>
            </Space>
          </Space>
        ) : (
          <Space direction="vertical" style={{ width: '100%' }}>
            {state.replaceMode && (
              <div style={{ marginBottom: 8, color: '#666' }}>
                Upload a new document to replace the existing one.
                <Button
                  type="link"
                  size="small"
                  onClick={() => setUploadState(docType.id, { replaceMode: false, file: null })}
                >
                  Cancel
                </Button>
              </div>
            )}
            <Upload
              accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png"
              beforeUpload={(file) => {
                const result = beforeUploadValidation(file);
                if (result !== Upload.LIST_IGNORE) {
                  setUploadState(docType.id, { file: file as unknown as UploadFile });
                }
                return result;
              }}
              fileList={state.file ? [state.file] : []}
              onRemove={() => setUploadState(docType.id, { file: null })}
              maxCount={1}
            >
              <Button type="primary" icon={<UploadOutlined />}>Select File (PDF, JPEG, PNG)</Button>
            </Upload>

            {state.uploading && <Progress percent={state.progress} size="small" />}

            {state.file && !state.uploading && (
              <Button
                type="primary"
                icon={<CloudUploadOutlined />}
                onClick={() => handleUpload(docType.id)}
                loading={state.uploading}
              >
                {state.replaceMode ? 'Replace' : 'Upload'} {docType.displayName}
              </Button>
            )}
          </Space>
        )}
      </Card>
    );
  };

  const isLoading = typesLoading || docsLoading;

  return (
    <Modal
      title="Upload Signed Agreements"
      open={open}
      onCancel={onClose}
      footer={
        <Button key="close" onClick={onClose}>
          {allUploaded ? 'Done' : 'Close'}
        </Button>
      }
      width={520}
    >
      <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 6 }}>
        <strong>{applicant.husband.firstName} {applicant.husband.lastName}</strong> - Upload required documents.
      </div>

      {isLoading ? (
        <div style={{ textAlign: 'center', padding: 40 }}>
          <Spin />
        </div>
      ) : !documentTypes || documentTypes.length === 0 ? (
        <Empty description="No document types configured" />
      ) : (
        <Space direction="vertical" style={{ width: '100%' }} size="middle">
          {documentTypes.map(renderDocumentCard)}

          {/* Status summary */}
          {allUploaded && (
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
              All required documents uploaded!
            </div>
          )}
        </Space>
      )}
    </Modal>
  );
};

export default DocumentUploadModal;
