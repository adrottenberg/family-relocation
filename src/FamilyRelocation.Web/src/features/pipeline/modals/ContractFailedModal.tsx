import { Modal, Form, Input, Select, message } from 'antd';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { applicantsApi } from '../../../api';

interface ContractFailedModalProps {
  open: boolean;
  onClose: () => void;
  applicantId: string;
  familyName: string;
}

interface FormValues {
  reason: string;
  notes?: string;
}

const FAILURE_REASONS = [
  { value: 'inspection', label: 'Inspection Issues' },
  { value: 'financing', label: 'Financing Fell Through' },
  { value: 'appraisal', label: 'Appraisal Issues' },
  { value: 'title', label: 'Title Issues' },
  { value: 'buyer_cold_feet', label: 'Buyer Changed Mind' },
  { value: 'seller_backed_out', label: 'Seller Backed Out' },
  { value: 'other', label: 'Other' },
];

const ContractFailedModal = ({
  open,
  onClose,
  applicantId,
  familyName,
}: ContractFailedModalProps) => {
  const [form] = Form.useForm<FormValues>();
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async (values: FormValues) => {
      const reasonLabel = FAILURE_REASONS.find(r => r.value === values.reason)?.label || values.reason;
      return applicantsApi.changeStage(applicantId, {
        newStage: 'HouseHunting',
        notes: `Contract failed: ${reasonLabel}${values.notes ? `. ${values.notes}` : ''}`,
      });
    },
    onSuccess: () => {
      message.info('Moved back to House Hunting');
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
      queryClient.invalidateQueries({ queryKey: ['applicants'] });
      form.resetFields();
      onClose();
    },
    onError: () => {
      message.error('Failed to update stage');
    },
  });

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      mutation.mutate(values);
    } catch {
      // Validation failed
    }
  };

  const handleCancel = () => {
    form.resetFields();
    onClose();
  };

  return (
    <Modal
      title="Contract Fell Through"
      open={open}
      onOk={handleSubmit}
      onCancel={handleCancel}
      okText="Move to House Hunting"
      confirmLoading={mutation.isPending}
      destroyOnClose
    >
      <div style={{ marginBottom: 16, padding: 12, background: '#fff2f0', borderRadius: 6, border: '1px solid #ffccc7' }}>
        Recording that <strong>{familyName} Family</strong>'s contract fell through
      </div>

      <Form form={form} layout="vertical">
        <Form.Item
          name="reason"
          label="Reason"
          rules={[{ required: true, message: 'Please select a reason' }]}
        >
          <Select
            placeholder="Select reason"
            options={FAILURE_REASONS}
            size="large"
          />
        </Form.Item>

        <Form.Item
          name="notes"
          label="Additional Details"
        >
          <Input.TextArea
            rows={3}
            placeholder="Any additional details about why the contract fell through..."
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default ContractFailedModal;
