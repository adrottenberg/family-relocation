import { useState, useEffect } from 'react';
import {
  Modal,
  Form,
  Input,
  Select,
  InputNumber,
  Switch,
  DatePicker,
  TimePicker,
  message,
  Space,
} from 'antd';
import { PhoneOutlined, FileTextOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { activitiesApi, LogActivityRequest } from '../../api';
import { toUtcString } from '../../utils/datetime';

const { TextArea } = Input;
const { Option } = Select;

interface LogActivityModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
  entityType: string;
  entityId: string;
  entityName: string;
}

const activityTypes = [
  { value: 'PhoneCall', label: 'Phone Call', icon: <PhoneOutlined /> },
  { value: 'Note', label: 'Note', icon: <FileTextOutlined /> },
  // Email and SMS disabled for now as per requirements
  // { value: 'Email', label: 'Email', icon: <MailOutlined /> },
  // { value: 'SMS', label: 'SMS', icon: <MessageOutlined /> },
];

const callOutcomes = [
  { value: 'Connected', label: 'Connected' },
  { value: 'Voicemail', label: 'Voicemail' },
  { value: 'NoAnswer', label: 'No Answer' },
  { value: 'Busy', label: 'Busy' },
  { value: 'LeftMessage', label: 'Left Message' },
];

const LogActivityModal = ({
  open,
  onClose,
  onSuccess,
  entityType,
  entityId,
  entityName,
}: LogActivityModalProps) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [selectedType, setSelectedType] = useState<string>('PhoneCall');

  useEffect(() => {
    if (open) {
      form.resetFields();
      setSelectedType('PhoneCall');
    }
  }, [open, form]);

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setLoading(true);

      // Combine follow-up date and time, convert to UTC
      let followUpDateTime: string | undefined;
      if (values.createFollowUp && values.followUpDate) {
        let combinedDateTime = values.followUpDate;
        if (values.followUpTime) {
          combinedDateTime = values.followUpDate
            .hour(values.followUpTime.hour())
            .minute(values.followUpTime.minute())
            .second(0);
        } else {
          // Default to 9 AM if no time specified
          combinedDateTime = values.followUpDate.hour(9).minute(0).second(0);
        }
        followUpDateTime = toUtcString(combinedDateTime);
      }

      const request: LogActivityRequest = {
        entityType,
        entityId,
        type: values.type,
        description: values.description,
        durationMinutes: values.durationMinutes,
        outcome: values.outcome,
        createFollowUp: values.createFollowUp || false,
        followUpDate: followUpDateTime,
        followUpTitle: values.followUpTitle,
      };

      const result = await activitiesApi.log(request);

      if (result.followUpReminderId) {
        message.success('Activity logged and follow-up reminder created');
      } else {
        message.success('Activity logged successfully');
      }

      form.resetFields();
      onSuccess();
    } catch (error) {
      if (error instanceof Error && 'errorFields' in error) {
        return;
      }
      console.error('Failed to log activity:', error);
      message.error('Failed to log activity');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    form.resetFields();
    onClose();
  };

  const isPhoneCall = selectedType === 'PhoneCall';

  return (
    <Modal
      title="Log Activity"
      open={open}
      onCancel={handleClose}
      onOk={handleSubmit}
      okText="Log Activity"
      confirmLoading={loading}
      width={600}
      destroyOnClose
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={{
          type: 'PhoneCall',
          createFollowUp: false,
        }}
        preserve={false}
      >
        <Form.Item label="Entity">
          <Input value={`${entityType}: ${entityName}`} disabled />
        </Form.Item>

        <Form.Item
          name="type"
          label="Activity Type"
          rules={[{ required: true, message: 'Please select an activity type' }]}
        >
          <Select
            placeholder="Select type"
            onChange={(value) => setSelectedType(value)}
          >
            {activityTypes.map((type) => (
              <Option key={type.value} value={type.value}>
                <Space>
                  {type.icon}
                  {type.label}
                </Space>
              </Option>
            ))}
          </Select>
        </Form.Item>

        {isPhoneCall && (
          <Space style={{ width: '100%' }} size="middle">
            <Form.Item
              name="durationMinutes"
              label="Duration (minutes)"
              style={{ flex: 1 }}
            >
              <InputNumber min={0} max={480} style={{ width: '100%' }} placeholder="e.g., 15" />
            </Form.Item>

            <Form.Item
              name="outcome"
              label="Call Outcome"
              style={{ flex: 1 }}
            >
              <Select placeholder="Select outcome" allowClear>
                {callOutcomes.map((outcome) => (
                  <Option key={outcome.value} value={outcome.value}>
                    {outcome.label}
                  </Option>
                ))}
              </Select>
            </Form.Item>
          </Space>
        )}

        <Form.Item
          name="description"
          label="Notes"
          rules={[
            { required: true, message: 'Please enter notes about this activity' },
            { max: 2000, message: 'Notes must not exceed 2000 characters' },
          ]}
        >
          <TextArea
            rows={4}
            placeholder={
              isPhoneCall
                ? 'Summarize the phone conversation...'
                : 'Enter your notes...'
            }
            maxLength={2000}
            showCount
          />
        </Form.Item>

        <Form.Item
          name="createFollowUp"
          label="Create Follow-up Reminder"
          valuePropName="checked"
        >
          <Switch />
        </Form.Item>

        <Form.Item
          noStyle
          shouldUpdate={(prevValues, currentValues) =>
            prevValues.createFollowUp !== currentValues.createFollowUp
          }
        >
          {({ getFieldValue }) =>
            getFieldValue('createFollowUp') && (
              <>
                <Space style={{ width: '100%' }} size="middle">
                  <Form.Item
                    name="followUpDate"
                    label="Follow-up Date"
                    rules={[{ required: true, message: 'Please select a follow-up date' }]}
                    style={{ flex: 1 }}
                  >
                    <DatePicker
                      style={{ width: '100%' }}
                      disabledDate={(current) => current && current < dayjs().startOf('day')}
                      format="MMMM D, YYYY"
                    />
                  </Form.Item>

                  <Form.Item
                    name="followUpTime"
                    label="Time (optional, default 9 AM)"
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

                <Form.Item
                  name="followUpTitle"
                  label="Reminder Title (optional)"
                >
                  <Input placeholder="e.g., Call back about documents" />
                </Form.Item>
              </>
            )
          }
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default LogActivityModal;
