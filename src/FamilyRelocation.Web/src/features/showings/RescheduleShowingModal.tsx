import { useEffect } from 'react';
import { Modal, Form, DatePicker, TimePicker, message } from 'antd';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { showingsApi } from '../../api';
import dayjs from 'dayjs';
import { toUtcString, parseUtcToLocal } from '../../utils/datetime';

interface RescheduleShowingModalProps {
  open: boolean;
  onClose: () => void;
  showingId: string;
  currentDateTime?: string;
  propertyInfo?: {
    street: string;
    city: string;
  };
}

const RescheduleShowingModal = ({
  open,
  onClose,
  showingId,
  currentDateTime,
  propertyInfo,
}: RescheduleShowingModalProps) => {
  const queryClient = useQueryClient();
  const [form] = Form.useForm();

  useEffect(() => {
    if (open && currentDateTime) {
      const localDt = parseUtcToLocal(currentDateTime);
      form.setFieldsValue({
        scheduledDate: localDt,
        scheduledTime: localDt,
      });
    }
  }, [open, currentDateTime, form]);

  const rescheduleMutation = useMutation({
    mutationFn: (data: { newScheduledDateTime: string }) =>
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
      // Combine date and time into a single datetime and convert to UTC
      const combinedDateTime = values.scheduledDate
        .hour(values.scheduledTime.hour())
        .minute(values.scheduledTime.minute())
        .second(0);
      rescheduleMutation.mutate({
        newScheduledDateTime: toUtcString(combinedDateTime),
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
