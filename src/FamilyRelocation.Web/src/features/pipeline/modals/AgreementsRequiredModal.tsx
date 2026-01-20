import { Modal, Result, Button, List, Tag } from 'antd';
import { FileTextOutlined, CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';

interface AgreementsRequiredModalProps {
  open: boolean;
  onClose: () => void;
  applicantId: string;
  familyName: string;
  brokerAgreementSigned: boolean;
  communityTakanosSigned: boolean;
}

const AgreementsRequiredModal = ({
  open,
  onClose,
  applicantId,
  familyName,
  brokerAgreementSigned,
  communityTakanosSigned,
}: AgreementsRequiredModalProps) => {
  const navigate = useNavigate();

  const handleGoToApplicant = () => {
    onClose();
    navigate(`/applicants/${applicantId}`);
  };

  const agreements = [
    {
      name: 'Broker Agreement',
      signed: brokerAgreementSigned,
    },
    {
      name: 'Community Takanos',
      signed: communityTakanosSigned,
    },
  ];

  return (
    <Modal
      open={open}
      onCancel={onClose}
      footer={[
        <Button key="cancel" onClick={onClose}>
          Cancel
        </Button>,
        <Button key="go" type="primary" onClick={handleGoToApplicant}>
          Go to Applicant
        </Button>,
      ]}
      width={480}
    >
      <Result
        icon={<FileTextOutlined style={{ color: '#1890ff' }} />}
        title="Signed Agreements Required"
        subTitle={
          <>
            <strong>{familyName} Family</strong> needs to sign the following agreements before starting house hunting:
          </>
        }
      />
      <List
        dataSource={agreements}
        renderItem={(item) => (
          <List.Item>
            <List.Item.Meta
              avatar={
                item.signed ? (
                  <CheckCircleOutlined style={{ color: '#52c41a', fontSize: 20 }} />
                ) : (
                  <CloseCircleOutlined style={{ color: '#ff4d4f', fontSize: 20 }} />
                )
              }
              title={item.name}
            />
            <Tag color={item.signed ? 'success' : 'error'}>
              {item.signed ? 'Signed' : 'Not Signed'}
            </Tag>
          </List.Item>
        )}
        style={{ marginBottom: 16 }}
      />
      <div style={{ textAlign: 'center', color: '#666' }}>
        Please upload the signed agreements on the applicant detail page.
      </div>
    </Modal>
  );
};

export default AgreementsRequiredModal;
