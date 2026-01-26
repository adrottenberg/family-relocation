import { useEffect, useState } from 'react';
import {
  Modal,
  Form,
  Input,
  DatePicker,
  TimePicker,
  Select,
  message,
  Space,
  Spin,
  Alert,
} from 'antd';
import { remindersApi, ReminderDto, ReminderPriority } from '../../api';
import { toUtcString, parseUtcToLocal } from '../../utils/datetime';

const { TextArea } = Input;
const { Option } = Select;

interface EditReminderModalProps {
  open: boolean;
  reminderId: string | null;
  onClose: () => void;
  onSuccess: () => void;
}

const priorityOptions: { value: ReminderPriority; label: string; color: string }[] = [
  { value: 'Urgent', label: 'Urgent', color: '#ff4d4f' },
  { value: 'High', label: 'High', color: '#fa8c16' },
  { value: 'Normal', label: 'Normal', color: '#1890ff' },
  { value: 'Low', label: 'Low', color: '#8c8c8c' },
];

const EditReminderModal = ({
  open,
  reminderId,
  onClose,
  onSuccess,
}: EditReminderModalProps) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [reminder, setReminder] = useState<ReminderDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Load reminder data when modal opens
  useEffect(() => {
    if (!open) {
      return;
    }

    if (!reminderId) {
      setError('No reminder ID provided');
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    remindersApi
      .getById(reminderId)
      .then((data) => {
        setReminder(data);
        const localDueDateTime = parseUtcToLocal(data.dueDateTime);
        form.setFieldsValue({
          title: data.title,
          dueDate: localDueDateTime,
          dueTime: localDueDateTime,
          priority: data.priority,
          notes: data.notes || '',
        });
        setLoading(false);
      })
      .catch((err) => {
        console.error('Failed to fetch reminder:', err);
        setError('Failed to load reminder');
        message.error('Failed to load reminder');
        setLoading(false);
      });
  }, [open, reminderId, form]);

  // Reset state when modal closes
  useEffect(() => {
    if (!open) {
      setReminder(null);
      setLoading(true);
      setError(null);
      form.resetFields();
    }
  }, [open, form]);

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSaving(true);

      // Combine date and time into a single datetime
      let combinedDateTime = values.dueDate;
      if (values.dueTime) {
        combinedDateTime = values.dueDate
          .hour(values.dueTime.hour())
          .minute(values.dueTime.minute())
          .second(0);
      }

      const utcString = toUtcString(combinedDateTime);

      await remindersApi.update(reminderId!, {
        title: values.title,
        dueDateTime: utcString,
        priority: values.priority,
        notes: values.notes || undefined,
      });

      message.success('Reminder updated successfully');
      onSuccess();
      onClose();
    } catch (error) {
      if (error instanceof Error && 'errorFields' in error) {
        return;
      }
      console.error('Failed to update reminder:', error);
      message.error('Failed to update reminder');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      title="Edit Reminder"
      open={open}
      onCancel={onClose}
      onOk={handleSubmit}
      okText="Save"
      confirmLoading={saving}
      okButtonProps={{ disabled: loading || !!error }}
      width={600}
    >
      {loading ? (
        <div style={{ textAlign: 'center', padding: 40 }}>
          <Spin />
          <div style={{ marginTop: 16 }}>Loading reminder...</div>
        </div>
      ) : error ? (
        <Alert type="error" message={error} />
      ) : (
        <>
          {reminder && (
            <Alert
              type="info"
              style={{ marginBottom: 16 }}
              message={
                <span><strong>Related to:</strong> {reminder.entityType} - {reminder.entityDisplayName || reminder.entityId}</span>
              }
            />
          )}

          <Form form={form} layout="vertical">
            <Form.Item
              name="title"
              label="Title"
              rules={[
                { required: true, message: 'Please enter a title' },
                { max: 200, message: 'Title must be 200 characters or less' },
              ]}
            >
              <Input placeholder="Reminder title" />
            </Form.Item>

            <Space style={{ width: '100%' }} size="middle">
              <Form.Item
                name="dueDate"
                label="Due Date"
                rules={[{ required: true, message: 'Please select a due date' }]}
                style={{ flex: 1 }}
              >
                <DatePicker
                  style={{ width: '100%' }}
                  format="MMMM D, YYYY"
                />
              </Form.Item>

              <Form.Item
                name="dueTime"
                label="Due Time"
                rules={[{ required: true, message: 'Please select a due time' }]}
                style={{ flex: 1 }}
              >
                <TimePicker
                  style={{ width: '100%' }}
                  format="h:mm A"
                  use12Hours
                  minuteStep={15}
                />
              </Form.Item>
            </Space>

            <Form.Item name="priority" label="Priority">
              <Select placeholder="Select priority">
                {priorityOptions.map((opt) => (
                  <Option key={opt.value} value={opt.value}>
                    <span style={{ color: opt.color }}>{opt.label}</span>
                  </Option>
                ))}
              </Select>
            </Form.Item>

            <Form.Item name="notes" label="Notes">
              <TextArea
                rows={3}
                placeholder="Additional notes..."
                maxLength={2000}
                showCount
              />
            </Form.Item>
          </Form>
        </>
      )}
    </Modal>
  );
};

export default EditReminderModal;
