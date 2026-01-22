import { Modal, Form, Select, Input, message } from 'antd';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { propertyMatchesApi } from '../../api';
import type { PropertyMatchStatus } from '../../api/types';

const { TextArea } = Input;
const { Option } = Select;

interface UpdateMatchStatusModalProps {
  open: boolean;
  onClose: () => void;
  matchId: string;
  currentStatus: PropertyMatchStatus;
  propertyInfo?: {
    street: string;
    city: string;
  };
  applicantName?: string;
}

const statusOptions: { value: PropertyMatchStatus; label: string }[] = [
  { value: 'MatchIdentified', label: 'Match Identified' },
  { value: 'ShowingRequested', label: 'Showing Requested' },
  { value: 'ApplicantInterested', label: 'Applicant Interested' },
  { value: 'OfferMade', label: 'Offer Made' },
  { value: 'ApplicantRejected', label: 'Applicant Rejected' },
];

const UpdateMatchStatusModal = ({
  open,
  onClose,
  matchId,
  currentStatus,
  propertyInfo,
  applicantName,
}: UpdateMatchStatusModalProps) => {
  const queryClient = useQueryClient();
  const [form] = Form.useForm();

  const updateStatusMutation = useMutation({
    mutationFn: (data: { status: PropertyMatchStatus; notes?: string }) =>
      propertyMatchesApi.updateStatus(matchId, data),
    onSuccess: () => {
      message.success('Match status updated successfully');
      queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
      onClose();
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to update status');
    },
  });

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      updateStatusMutation.mutate({
        status: values.status,
        notes: values.notes,
      });
    } catch {
      // Validation error
    }
  };

  // Filter out current status from options
  const availableOptions = statusOptions.filter((opt) => opt.value !== currentStatus);

  return (
    <Modal
      title="Update Match Status"
      open={open}
      onCancel={onClose}
      onOk={handleSubmit}
      confirmLoading={updateStatusMutation.isPending}
      width={450}
    >
      {(propertyInfo || applicantName) && (
        <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 4 }}>
          {propertyInfo && (
            <div>
              <strong>Property:</strong> {propertyInfo.street}, {propertyInfo.city}
            </div>
          )}
          {applicantName && (
            <div style={{ marginTop: propertyInfo ? 4 : 0 }}>
              <strong>Family:</strong> {applicantName}
            </div>
          )}
          <div style={{ marginTop: 4, color: '#666' }}>
            <strong>Current Status:</strong> {statusOptions.find((s) => s.value === currentStatus)?.label || currentStatus}
          </div>
        </div>
      )}

      <Form form={form} layout="vertical" initialValues={{ status: undefined }}>
        <Form.Item
          name="status"
          label="New Status"
          rules={[{ required: true, message: 'Please select a status' }]}
        >
          <Select placeholder="Select new status">
            {availableOptions.map((option) => (
              <Option key={option.value} value={option.value}>
                {option.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item name="notes" label="Notes">
          <TextArea rows={3} placeholder="Optional notes about this status change..." />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default UpdateMatchStatusModal;
