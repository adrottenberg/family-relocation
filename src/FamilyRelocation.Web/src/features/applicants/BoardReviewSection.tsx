import { Card, Button, Descriptions, Tag, Space, Alert, Spin } from 'antd';
import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  EditOutlined,
  UploadOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { documentsApi, getDocumentTypes } from '../../api';
import type { ApplicantDto } from '../../api/types';

interface BoardReviewSectionProps {
  applicant: ApplicantDto;
  onRecordDecision: () => void;
  onUploadDocuments: () => void;
  canApprove?: boolean;
}

const getDecisionConfig = (decision: string) => {
  switch (decision) {
    case 'Approved':
      return {
        color: 'success',
        icon: <CheckCircleOutlined />,
        bgColor: '#f6ffed',
        borderColor: '#b7eb8f',
      };
    case 'Rejected':
      return {
        color: 'error',
        icon: <CloseCircleOutlined />,
        bgColor: '#fff2f0',
        borderColor: '#ffccc7',
      };
    case 'Deferred':
      return {
        color: 'warning',
        icon: <ClockCircleOutlined />,
        bgColor: '#fffbe6',
        borderColor: '#ffe58f',
      };
    default:
      return {
        color: 'default',
        icon: <ClockCircleOutlined />,
        bgColor: '#fafafa',
        borderColor: '#d9d9d9',
      };
  }
};

const BoardReviewSection = ({ applicant, onRecordDecision, onUploadDocuments, canApprove = false }: BoardReviewSectionProps) => {
  const boardReview = applicant.boardReview;
  const decision = boardReview?.decision || 'Pending';
  const config = getDecisionConfig(decision);

  const isPending = decision === 'Pending';
  const isApproved = decision === 'Approved';

  // Fetch document types and applicant documents
  const { data: documentTypes, isLoading: typesLoading } = useQuery({
    queryKey: ['documentTypes'],
    queryFn: () => getDocumentTypes(true),
    enabled: isApproved,
  });

  const { data: applicantDocuments, isLoading: docsLoading } = useQuery({
    queryKey: ['applicantDocuments', applicant.id],
    queryFn: () => documentsApi.getApplicantDocuments(applicant.id),
    enabled: isApproved,
  });

  // Check which documents are missing
  const getMissingDocuments = () => {
    if (!documentTypes || !applicantDocuments) return [];
    return documentTypes.filter(
      (dt) => !applicantDocuments.some((doc) => doc.documentTypeId === dt.id)
    );
  };

  const missingDocuments = getMissingDocuments();
  const allDocumentsUploaded = documentTypes && applicantDocuments && missingDocuments.length === 0;
  const isLoading = typesLoading || docsLoading;

  return (
    <Card
      title="Board Review"
      size="small"
      className="info-card"
      extra={
        canApprove && (
          <Button
            type={isPending ? 'primary' : 'default'}
            icon={isPending ? undefined : <EditOutlined />}
            onClick={onRecordDecision}
            size="small"
          >
            {isPending ? 'Record Decision' : 'Update'}
          </Button>
        )
      }
    >
      <Space direction="vertical" style={{ width: '100%' }} size="middle">
        {/* Decision Display */}
        <div
          style={{
            padding: 16,
            background: config.bgColor,
            border: `1px solid ${config.borderColor}`,
            borderRadius: 8,
            display: 'flex',
            alignItems: 'center',
            gap: 12,
          }}
        >
          <span style={{ fontSize: 24 }}>{config.icon}</span>
          <div>
            <div style={{ fontWeight: 600, fontSize: 16 }}>
              <Tag color={config.color as 'success' | 'error' | 'warning' | 'default'}>
                {decision}
              </Tag>
            </div>
            {boardReview?.reviewDate && (
              <div style={{ color: '#666', fontSize: 13, marginTop: 4 }}>
                Reviewed on {new Date(boardReview.reviewDate).toLocaleDateString()}
              </div>
            )}
          </div>
        </div>

        {/* Notes */}
        {boardReview?.notes && (
          <Descriptions column={1} size="small">
            <Descriptions.Item label="Notes">
              {boardReview.notes}
            </Descriptions.Item>
          </Descriptions>
        )}

        {/* Next Steps Alert for Approved */}
        {isApproved && isLoading && (
          <div style={{ textAlign: 'center', padding: 16 }}>
            <Spin size="small" />
          </div>
        )}

        {isApproved && !isLoading && missingDocuments.length > 0 && (
          <Alert
            type="info"
            showIcon
            message="Next Steps Required"
            description={
              <div>
                <ul style={{ margin: '8px 0 12px', paddingLeft: 20 }}>
                  {missingDocuments.map((doc) => (
                    <li key={doc.id}>{doc.displayName} needs to be uploaded</li>
                  ))}
                </ul>
                <Button
                  type="primary"
                  icon={<UploadOutlined />}
                  onClick={onUploadDocuments}
                >
                  Upload Agreements
                </Button>
              </div>
            }
          />
        )}

        {isApproved && !isLoading && allDocumentsUploaded && (
          <Alert
            type="success"
            showIcon
            message="Ready for House Hunting"
            description="All required agreements have been signed. The applicant can proceed to house hunting."
          />
        )}
      </Space>
    </Card>
  );
};

export default BoardReviewSection;
