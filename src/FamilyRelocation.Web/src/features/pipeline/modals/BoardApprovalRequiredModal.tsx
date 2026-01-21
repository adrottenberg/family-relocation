import { useState } from 'react';
import { Modal, Result, Button, Form, Select, DatePicker, Input, message, Space } from 'antd';
import { ExclamationCircleOutlined, CheckCircleOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { applicantsApi } from '../../../api';
import dayjs from 'dayjs';

const { TextArea } = Input;

interface BoardApprovalRequiredModalProps {
  open: boolean;
  onClose: () => void;
  applicantId: string;
  familyName: string;
  onApprovalComplete?: () => void;
  canApprove?: boolean;
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

const BoardApprovalRequiredModal = ({
  open,
  onClose,
  applicantId,
  familyName,
  onApprovalComplete,
  canApprove = false,
}: BoardApprovalRequiredModalProps) => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [showDecisionForm, setShowDecisionForm] = useState(false);
  const [form] = Form.useForm<FormValues>();

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      applicantsApi.setBoardDecision(applicantId, {
        decision: values.decision,
        reviewDate: values.reviewDate.toISOString(),
        notes: values.notes,
      }),
    onSuccess: (data) => {
      if (data.newStage === 'AwaitingAgreements') {
        message.success('Board decision recorded! Applicant approved and awaiting agreements.');
        queryClient.invalidateQueries({ queryKey: ['applicant', applicantId] });
        queryClient.invalidateQueries({ queryKey: ['applicants'] });
        queryClient.invalidateQueries({ queryKey: ['pipeline'] });
        onApprovalComplete?.();
        handleClose();
      } else {
        message.info(data.message || 'Board decision recorded');
        queryClient.invalidateQueries({ queryKey: ['applicant', applicantId] });
        queryClient.invalidateQueries({ queryKey: ['applicants'] });
        queryClient.invalidateQueries({ queryKey: ['pipeline'] });
        handleClose();
      }
    },
    onError: (error: Error & { response?: { status: number } }) => {
      if (error.response?.status === 403) {
        message.error('You do not have permission to set board decisions');
      } else {
        message.error('Failed to record board decision');
      }
    },
  });

  const handleClose = () => {
    setShowDecisionForm(false);
    form.resetFields();
    onClose();
  };

  const handleGoToApplicant = () => {
    handleClose();
    navigate(`/applicants/${applicantId}`);
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      mutation.mutate(values);
    } catch {
      // Validation failed
    }
  };

  return (
    <Modal
      open={open}
      onCancel={handleClose}
      footer={
        showDecisionForm
          ? [
              <Button key="back" onClick={() => setShowDecisionForm(false)}>
                Back
              </Button>,
              <Button
                key="submit"
                type="primary"
                onClick={handleSubmit}
                loading={mutation.isPending}
              >
                Save Decision
              </Button>,
            ]
          : canApprove
            ? [
                <Button key="cancel" onClick={handleClose}>
                  Cancel
                </Button>,
                <Button key="decide" type="primary" onClick={() => setShowDecisionForm(true)}>
                  Record Decision Now
                </Button>,
                <Button key="go" onClick={handleGoToApplicant}>
                  Go to Applicant Page
                </Button>,
              ]
            : [
                <Button key="cancel" onClick={handleClose}>
                  Cancel
                </Button>,
                <Button key="go" type="primary" onClick={handleGoToApplicant}>
                  Go to Applicant Page
                </Button>,
              ]
      }
      width={500}
    >
      {!showDecisionForm ? (
        <Result
          icon={<ExclamationCircleOutlined style={{ color: '#faad14' }} />}
          title="Board Approval Required"
          subTitle={
            canApprove ? (
              <>
                <strong>{familyName} Family</strong> needs board approval before they can start searching.
                <br /><br />
                You can record the decision now or go to the applicant detail page.
              </>
            ) : (
              <>
                <strong>{familyName} Family</strong> needs board approval before they can start searching.
                <br /><br />
                Only board members can record decisions. Please contact a board member or admin to approve this applicant.
              </>
            )
          }
        />
      ) : (
        <>
          <div style={{ marginBottom: 16 }}>
            <Space>
              <CheckCircleOutlined style={{ color: '#1890ff', fontSize: 20 }} />
              <span style={{ fontSize: 16, fontWeight: 500 }}>Record Board Decision</span>
            </Space>
          </div>

          <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 6 }}>
            <strong>Applicant:</strong> {familyName} Family
          </div>

          <Form
            form={form}
            layout="vertical"
            initialValues={{
              reviewDate: dayjs(),
            }}
          >
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
                rows={3}
                placeholder="Optional notes about the board decision..."
              />
            </Form.Item>
          </Form>
        </>
      )}
    </Modal>
  );
};

export default BoardApprovalRequiredModal;
