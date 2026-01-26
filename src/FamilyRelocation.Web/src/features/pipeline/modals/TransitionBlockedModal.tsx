import { Modal, Result, Button } from 'antd';
import { CloseCircleOutlined } from '@ant-design/icons';

interface TransitionBlockedModalProps {
  open: boolean;
  onClose: () => void;
  message: string;
  familyName: string;
}

const TransitionBlockedModal = ({
  open,
  onClose,
  message,
  familyName,
}: TransitionBlockedModalProps) => {
  return (
    <Modal
      open={open}
      onCancel={onClose}
      footer={[
        <Button key="ok" type="primary" onClick={onClose}>
          OK
        </Button>,
      ]}
      width={400}
    >
      <Result
        icon={<CloseCircleOutlined style={{ color: '#ff4d4f' }} />}
        title="Transition Not Allowed"
        subTitle={
          <>
            <strong>{familyName}</strong>
            <br />
            {message}
          </>
        }
      />
    </Modal>
  );
};

export default TransitionBlockedModal;
