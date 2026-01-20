import { Card, Button, Descriptions, Tag, Space, Alert } from 'antd';
import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  EditOutlined,
  UploadOutlined,
} from '@ant-design/icons';
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
  const housingSearch = applicant.housingSearch;

  // Check if agreements are signed (for approved applicants)
  const brokerSigned = housingSearch?.brokerAgreementSigned || false;
  const takanosSigned = housingSearch?.communityTakanosSigned || false;
  const bothSigned = brokerSigned && takanosSigned;

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
        {isApproved && !bothSigned && (
          <Alert
            type="info"
            showIcon
            message="Next Steps Required"
            description={
              <div>
                <ul style={{ margin: '8px 0 12px', paddingLeft: 20 }}>
                  {!brokerSigned && <li>Broker Agreement needs to be signed</li>}
                  {!takanosSigned && <li>Community Takanos needs to be signed</li>}
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

        {isApproved && bothSigned && (
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
