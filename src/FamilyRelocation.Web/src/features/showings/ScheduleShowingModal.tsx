import { useEffect } from 'react';
import { Modal, Form, DatePicker, TimePicker, Input, message } from 'antd';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { showingsApi } from '../../api';
import dayjs from 'dayjs';

const { TextArea } = Input;

interface ScheduleShowingModalProps {
  open: boolean;
  onClose: () => void;
  propertyMatchId: string;
  propertyInfo?: {
    street: string;
    city: string;
  };
  applicantInfo?: {
    name: string;
  };
}

const ScheduleShowingModal = ({
  open,
  onClose,
  propertyMatchId,
  propertyInfo,
  applicantInfo,
}: ScheduleShowingModalProps) => {
  const queryClient = useQueryClient();
  const [form] = Form.useForm();

  useEffect(() => {
    if (open) {
      form.resetFields();
      // Default to tomorrow at 10:00 AM
      const tomorrow = dayjs().add(1, 'day');
      form.setFieldsValue({
        scheduledDate: tomorrow,
        scheduledTime: dayjs().hour(10).minute(0),
      });
    }
  }, [open, form]);

  const scheduleMutation = useMutation({
    mutationFn: showingsApi.schedule,
    onSuccess: () => {
      message.success('Showing scheduled successfully');
      queryClient.invalidateQueries({ queryKey: ['showings'] });
      queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
      onClose();
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to schedule showing');
    },
  });

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      scheduleMutation.mutate({
        propertyMatchId,
        scheduledDate: values.scheduledDate.format('YYYY-MM-DD'),
        scheduledTime: values.scheduledTime.format('HH:mm:ss'),
        notes: values.notes,
      });
    } catch {
      // Validation error
    }
  };

  return (
    <Modal
      title="Schedule Showing"
      open={open}
      onCancel={onClose}
      onOk={handleSubmit}
      confirmLoading={scheduleMutation.isPending}
      width={450}
    >
      {propertyInfo && (
        <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 4 }}>
          <strong>Property:</strong> {propertyInfo.street}, {propertyInfo.city}
          {applicantInfo && (
            <>
              <br />
              <strong>Family:</strong> {applicantInfo.name}
            </>
          )}
        </div>
      )}

      <Form form={form} layout="vertical">
        <Form.Item
          name="scheduledDate"
          label="Date"
          rules={[{ required: true, message: 'Please select a date' }]}
        >
          <DatePicker
            style={{ width: '100%' }}
            disabledDate={(current) => current && current < dayjs().startOf('day')}
          />
        </Form.Item>

        <Form.Item
          name="scheduledTime"
          label="Time"
          rules={[{ required: true, message: 'Please select a time' }]}
        >
          <TimePicker
            style={{ width: '100%' }}
            format="h:mm A"
            minuteStep={15}
            use12Hours
          />
        </Form.Item>

        <Form.Item name="notes" label="Notes">
          <TextArea rows={3} placeholder="Any special instructions or notes..." />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default ScheduleShowingModal;
