import { Modal, Form, Select, DatePicker, Input, message } from 'antd';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { applicantsApi } from '../../api';
import type { ApplicantDto } from '../../api/types';
import dayjs from 'dayjs';

const { TextArea } = Input;

interface SetBoardDecisionModalProps {
  open: boolean;
  onClose: () => void;
  applicant: ApplicantDto;
}

interface FormValues {
  decision: string;
  reviewDate: dayjs.Dayjs;
  notes?: string;
}

const DECISIONS = [
  { value: 'Approved', label: 'Approved', color: '#52c41a' },
  { value: 'Rejected', label: 'Rejected', color: '#ff4d4f' },
  { value: 'Deferred', label: 'Deferred', color: '#faad14' },
];

const SetBoardDecisionModal = ({ open, onClose, applicant }: SetBoardDecisionModalProps) => {
  const [form] = Form.useForm<FormValues>();
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      applicantsApi.setBoardDecision(applicant.id, {
        decision: values.decision,
        reviewDate: values.reviewDate.toISOString(),
        notes: values.notes,
      }),
    onSuccess: (data) => {
      message.success(data.message || 'Board decision recorded successfully');
      queryClient.invalidateQueries({ queryKey: ['applicant', applicant.id] });
      queryClient.invalidateQueries({ queryKey: ['applicants'] });
      form.resetFields();
      onClose();
    },
    onError: () => {
      message.error('Failed to record board decision');
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

  const currentDecision = applicant.boardReview?.decision;

  return (
    <Modal
      title="Record Board Decision"
      open={open}
      onOk={handleSubmit}
      onCancel={handleCancel}
      okText="Save Decision"
      confirmLoading={mutation.isPending}
      destroyOnClose
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={{
          reviewDate: dayjs(),
          decision: currentDecision !== 'Pending' ? currentDecision : undefined,
        }}
      >
        <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 6 }}>
          <strong>Applicant:</strong> {applicant.husband.firstName} {applicant.husband.lastName}
          {applicant.wife && ` & ${applicant.wife.firstName}`}
        </div>

        <Form.Item
          name="decision"
          label="Board Decision"
          rules={[{ required: true, message: 'Please select a decision' }]}
        >
          <Select
            placeholder="Select decision"
            options={DECISIONS}
            size="large"
          />
        </Form.Item>

        <Form.Item
          name="reviewDate"
          label="Review Date"
          rules={[{ required: true, message: 'Please select the review date' }]}
        >
          <DatePicker
            style={{ width: '100%' }}
            size="large"
            disabledDate={(current) => current && current > dayjs().endOf('day')}
          />
        </Form.Item>

        <Form.Item
          name="notes"
          label="Notes"
        >
          <TextArea
            rows={4}
            placeholder="Optional notes about the board decision..."
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default SetBoardDecisionModal;
