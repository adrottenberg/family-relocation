import { Modal, Result, Button } from 'antd';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';

interface BoardApprovalRequiredModalProps {
  open: boolean;
  onClose: () => void;
  applicantId: string;
  familyName: string;
}

const BoardApprovalRequiredModal = ({
  open,
  onClose,
  applicantId,
  familyName,
}: BoardApprovalRequiredModalProps) => {
  const navigate = useNavigate();

  const handleGoToApplicant = () => {
    onClose();
    navigate(`/applicants/${applicantId}`);
  };

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
      width={450}
    >
      <Result
        icon={<ExclamationCircleOutlined style={{ color: '#faad14' }} />}
        title="Board Approval Required"
        subTitle={
          <>
            <strong>{familyName} Family</strong> needs board approval before they can start house hunting.
            <br /><br />
            Please record the board decision on the applicant detail page first.
          </>
        }
      />
    </Modal>
  );
};

export default BoardApprovalRequiredModal;
