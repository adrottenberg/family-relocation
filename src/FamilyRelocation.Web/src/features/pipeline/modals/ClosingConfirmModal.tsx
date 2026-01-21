import { Modal, Form, DatePicker, Input, message } from 'antd';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { housingSearchesApi } from '../../../api';
import dayjs from 'dayjs';

interface ClosingConfirmModalProps {
  open: boolean;
  onClose: () => void;
  housingSearchId: string;
  familyName: string;
}

interface FormValues {
  closingDate: dayjs.Dayjs;
  notes?: string;
}

const ClosingConfirmModal = ({
  open,
  onClose,
  housingSearchId,
  familyName,
}: ClosingConfirmModalProps) => {
  const [form] = Form.useForm<FormValues>();
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async (values: FormValues) => {
      return housingSearchesApi.changeStage(housingSearchId, {
        newStage: 'Closed',
        closingDate: values.closingDate.toISOString(),
      });
    },
    onSuccess: () => {
      message.success('Congratulations! Deal closed successfully');
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
      title="Confirm Closing"
      open={open}
      onOk={handleSubmit}
      onCancel={handleCancel}
      okText="Confirm Closing"
      confirmLoading={mutation.isPending}
      destroyOnClose
    >
      <div style={{ marginBottom: 16, padding: 12, background: '#f6ffed', borderRadius: 6, border: '1px solid #b7eb8f' }}>
        Congratulations! <strong>{familyName} Family</strong> is closing on their new home!
      </div>

      <Form
        form={form}
        layout="vertical"
        initialValues={{
          closingDate: dayjs(),
        }}
      >
        <Form.Item
          name="closingDate"
          label="Closing Date"
          rules={[{ required: true, message: 'Please select the closing date' }]}
        >
          <DatePicker
            style={{ width: '100%' }}
            size="large"
          />
        </Form.Item>

        <Form.Item
          name="notes"
          label="Notes"
        >
          <Input.TextArea
            rows={3}
            placeholder="Any additional notes about the closing..."
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default ClosingConfirmModal;
