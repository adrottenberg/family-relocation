import { useEffect } from 'react';
import { Modal, Form, DatePicker, TimePicker, message } from 'antd';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { showingsApi } from '../../api';
import dayjs from 'dayjs';

interface RescheduleShowingModalProps {
  open: boolean;
  onClose: () => void;
  showingId: string;
  currentDate?: string;
  currentTime?: string;
  propertyInfo?: {
    street: string;
    city: string;
  };
}

const RescheduleShowingModal = ({
  open,
  onClose,
  showingId,
  currentDate,
  currentTime,
  propertyInfo,
}: RescheduleShowingModalProps) => {
  const queryClient = useQueryClient();
  const [form] = Form.useForm();

  useEffect(() => {
    if (open && currentDate && currentTime) {
      form.setFieldsValue({
        scheduledDate: dayjs(currentDate),
        scheduledTime: dayjs(`2000-01-01T${currentTime}`),
      });
    }
  }, [open, currentDate, currentTime, form]);

  const rescheduleMutation = useMutation({
    mutationFn: (data: { newDate: string; newTime: string }) =>
      showingsApi.reschedule(showingId, data),
    onSuccess: () => {
      message.success('Showing rescheduled successfully');
      queryClient.invalidateQueries({ queryKey: ['showings'] });
      onClose();
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to reschedule showing');
    },
  });

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      rescheduleMutation.mutate({
        newDate: values.scheduledDate.format('YYYY-MM-DD'),
        newTime: values.scheduledTime.format('HH:mm:ss'),
      });
    } catch {
      // Validation error
    }
  };

  return (
    <Modal
      title="Reschedule Showing"
      open={open}
      onCancel={onClose}
      onOk={handleSubmit}
      confirmLoading={rescheduleMutation.isPending}
      width={450}
    >
      {propertyInfo && (
        <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 4 }}>
          <strong>Property:</strong> {propertyInfo.street}, {propertyInfo.city}
        </div>
      )}

      <Form form={form} layout="vertical">
        <Form.Item
          name="scheduledDate"
          label="New Date"
          rules={[{ required: true, message: 'Please select a date' }]}
        >
          <DatePicker
            style={{ width: '100%' }}
            disabledDate={(current) => current && current < dayjs().startOf('day')}
          />
        </Form.Item>

        <Form.Item
          name="scheduledTime"
          label="New Time"
          rules={[{ required: true, message: 'Please select a time' }]}
        >
          <TimePicker
            style={{ width: '100%' }}
            format="h:mm A"
            minuteStep={15}
            use12Hours
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default RescheduleShowingModal;
