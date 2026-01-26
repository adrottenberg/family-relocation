import { useState, useEffect } from 'react';
import {
  Modal,
  Form,
  Input,
  DatePicker,
  TimePicker,
  Select,
  Switch,
  message,
  Space,
  Spin,
} from 'antd';
import dayjs from 'dayjs';
import { useQuery } from '@tanstack/react-query';
import { remindersApi, CreateReminderRequest, ReminderPriority, applicantsApi, propertiesApi } from '../../api';
import { toUtcString } from '../../utils/datetime';

const { TextArea } = Input;
const { Option } = Select;

interface CreateReminderModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
  // Optional: pre-fill entity context when opened from a detail page
  entityType?: string;
  entityId?: string;
  entityDisplayName?: string;
}

const priorityOptions: { value: ReminderPriority; label: string; color: string }[] = [
  { value: 'Urgent', label: 'Urgent', color: '#ff4d4f' },
  { value: 'High', label: 'High', color: '#fa8c16' },
  { value: 'Normal', label: 'Normal', color: '#1890ff' },
  { value: 'Low', label: 'Low', color: '#8c8c8c' },
];

const CreateReminderModal = ({
  open,
  onClose,
  onSuccess,
  entityType: prefilledEntityType,
  entityId: prefilledEntityId,
  entityDisplayName,
}: CreateReminderModalProps) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [selectedEntityType, setSelectedEntityType] = useState<string>(prefilledEntityType || '');

  // Reset selected entity type when modal opens/closes or prefilled type changes
  useEffect(() => {
    setSelectedEntityType(prefilledEntityType || '');
  }, [prefilledEntityType, open]);

  // Fetch applicants for Applicant and HousingSearch entity types
  const { data: applicantsData, isLoading: isLoadingApplicants } = useQuery({
    queryKey: ['applicants', 'all'],
    queryFn: () => applicantsApi.getAll({ pageSize: 500 }),
    enabled: open && (selectedEntityType === 'Applicant' || selectedEntityType === 'HousingSearch') && !prefilledEntityId,
    staleTime: 30000, // Cache for 30 seconds
  });

  // Fetch properties for Property entity type
  const { data: propertiesData, isLoading: isLoadingProperties } = useQuery({
    queryKey: ['properties', 'all'],
    queryFn: () => propertiesApi.getAll({ pageSize: 500 }),
    enabled: open && selectedEntityType === 'Property' && !prefilledEntityId,
    staleTime: 30000,
  });

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setLoading(true);

      // Combine date and time into a single datetime
      // If no time is provided, default to 2 PM (14:00)
      let combinedDateTime = values.dueDate;
      if (values.dueTime) {
        combinedDateTime = values.dueDate
          .hour(values.dueTime.hour())
          .minute(values.dueTime.minute())
          .second(0);
      } else {
        combinedDateTime = values.dueDate.hour(14).minute(0).second(0);
      }

      const request: CreateReminderRequest = {
        title: values.title,
        dueDateTime: toUtcString(combinedDateTime),
        priority: values.priority || 'Normal',
        entityType: values.entityType,
        entityId: values.entityId,
        notes: values.notes || undefined,
        sendEmailNotification: values.sendEmailNotification || false,
      };

      await remindersApi.create(request);
      message.success('Reminder created successfully');
      form.resetFields();
      onSuccess();
    } catch (error) {
      if (error instanceof Error && 'errorFields' in error) {
        // Validation error, handled by form
        return;
      }
      console.error('Failed to create reminder:', error);
      message.error('Failed to create reminder');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    form.resetFields();
    onClose();
  };

  // Set initial values when modal opens with prefilled entity
  const initialValues = {
    priority: 'Normal',
    sendEmailNotification: false,
    entityType: prefilledEntityType,
    entityId: prefilledEntityId,
  };

  return (
    <Modal
      title="Create Reminder"
      open={open}
      onCancel={handleClose}
      onOk={handleSubmit}
      okText="Create"
      confirmLoading={loading}
      width={600}
      destroyOnClose
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={initialValues}
        preserve={false}
      >
        <Form.Item
          name="title"
          label="Title"
          rules={[
            { required: true, message: 'Please enter a title' },
            { max: 200, message: 'Title must be 200 characters or less' },
          ]}
        >
          <Input placeholder="e.g., Follow up with Cohen family about documents" />
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
              disabledDate={(current) => current && current < dayjs().startOf('day')}
              format="MMMM D, YYYY"
            />
          </Form.Item>

          <Form.Item name="dueTime" label="Due Time (optional)" style={{ flex: 1 }}>
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

        <Space style={{ width: '100%' }} size="middle">
          <Form.Item
            name="entityType"
            label="Related To"
            rules={[{ required: true, message: 'Please select entity type' }]}
            style={{ flex: 1 }}
          >
            <Select
              placeholder="Select type"
              onChange={(value) => {
                setSelectedEntityType(value);
                if (value === 'General') {
                  // For general reminders, set a placeholder entityId
                  form.setFieldValue('entityId', '00000000-0000-0000-0000-000000000000');
                } else {
                  form.setFieldValue('entityId', '');
                }
              }}
              disabled={!!prefilledEntityType}
            >
              <Option value="Applicant">Applicant</Option>
              <Option value="HousingSearch">Housing Search</Option>
              <Option value="Property">Property</Option>
              <Option value="General">General (No specific entity)</Option>
            </Select>
          </Form.Item>

          {selectedEntityType === 'Applicant' && !prefilledEntityId && (
            <Form.Item
              name="entityId"
              label="Applicant"
              rules={[{ required: true, message: 'Please select an applicant' }]}
              style={{ flex: 1 }}
            >
              <Select
                showSearch
                placeholder="Search for an applicant..."
                optionFilterProp="label"
                loading={isLoadingApplicants}
                notFoundContent={isLoadingApplicants ? <Spin size="small" /> : 'No applicants found'}
                options={applicantsData?.items.map(a => ({
                  value: a.id,
                  label: a.husbandFullName,
                }))}
              />
            </Form.Item>
          )}

          {selectedEntityType === 'HousingSearch' && !prefilledEntityId && (
            <Form.Item
              name="entityId"
              label="Applicant's Housing Search"
              rules={[{ required: true, message: 'Please select a housing search' }]}
              style={{ flex: 1 }}
            >
              <Select
                showSearch
                placeholder="Search by applicant name..."
                optionFilterProp="label"
                loading={isLoadingApplicants}
                notFoundContent={isLoadingApplicants ? <Spin size="small" /> : 'No housing searches found'}
                options={applicantsData?.items
                  .filter(a => a.housingSearchId) // Only show applicants with active housing searches
                  .map(a => ({
                    value: a.housingSearchId!,
                    label: `${a.husbandFullName} (${a.stage || 'Unknown Stage'})`,
                  }))}
              />
            </Form.Item>
          )}

          {selectedEntityType === 'Property' && !prefilledEntityId && (
            <Form.Item
              name="entityId"
              label="Property"
              rules={[{ required: true, message: 'Please select a property' }]}
              style={{ flex: 1 }}
            >
              <Select
                showSearch
                placeholder="Search by address..."
                optionFilterProp="label"
                loading={isLoadingProperties}
                notFoundContent={isLoadingProperties ? <Spin size="small" /> : 'No properties found'}
                options={propertiesData?.items.map(p => ({
                  value: p.id,
                  label: `${p.street}, ${p.city}`,
                }))}
              />
            </Form.Item>
          )}

          {prefilledEntityId && entityDisplayName && (
            <Form.Item label="Entity" style={{ flex: 1 }}>
              <Input value={entityDisplayName} disabled />
              <Form.Item name="entityId" hidden>
                <Input />
              </Form.Item>
            </Form.Item>
          )}
        </Space>

        <Form.Item name="notes" label="Notes (optional)">
          <TextArea
            rows={3}
            placeholder="Additional notes or context..."
            maxLength={2000}
            showCount
          />
        </Form.Item>

        <Form.Item
          name="sendEmailNotification"
          label="Email Notification"
          valuePropName="checked"
        >
          <Switch />
          <span style={{ marginLeft: 8, color: '#888' }}>
            Send email notification when reminder is due
          </span>
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default CreateReminderModal;
