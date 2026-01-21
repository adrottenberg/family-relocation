import { Modal, Form, InputNumber, DatePicker, Input, message } from 'antd';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { housingSearchesApi } from '../../../api';
import dayjs from 'dayjs';

interface ContractInfoModalProps {
  open: boolean;
  onClose: () => void;
  housingSearchId: string;
  familyName: string;
}

interface FormValues {
  contractPrice: number;
  contractDate: dayjs.Dayjs;
  expectedClosingDate?: dayjs.Dayjs;
  propertyAddress?: string;
}

const ContractInfoModal = ({
  open,
  onClose,
  housingSearchId,
  familyName,
}: ContractInfoModalProps) => {
  const [form] = Form.useForm<FormValues>();
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async (values: FormValues) => {
      return housingSearchesApi.changeStage(housingSearchId, {
        newStage: 'UnderContract',
        contract: {
          price: values.contractPrice,
          expectedClosingDate: values.expectedClosingDate?.toISOString(),
        },
      });
    },
    onSuccess: () => {
      message.success('Moved to Under Contract');
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
      title="Enter Contract Details"
      open={open}
      onOk={handleSubmit}
      onCancel={handleCancel}
      okText="Move to Under Contract"
      confirmLoading={mutation.isPending}
      destroyOnClose
    >
      <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 6 }}>
        Moving <strong>{familyName} Family</strong> to Under Contract
      </div>

      <Form
        form={form}
        layout="vertical"
        initialValues={{
          contractDate: dayjs(),
        }}
      >
        <Form.Item
          name="contractPrice"
          label="Contract Price"
          rules={[{ required: true, message: 'Please enter the contract price' }]}
        >
          <InputNumber
            style={{ width: '100%' }}
            size="large"
            formatter={(value) => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
            parser={(value) => Number(value?.replace(/\$\s?|(,*)/g, '') || 0) as unknown as 0}
            min={0}
            placeholder="Enter contract price"
          />
        </Form.Item>

        <Form.Item
          name="contractDate"
          label="Contract Date"
          rules={[{ required: true, message: 'Please select the contract date' }]}
        >
          <DatePicker
            style={{ width: '100%' }}
            size="large"
          />
        </Form.Item>

        <Form.Item
          name="expectedClosingDate"
          label="Expected Closing Date"
        >
          <DatePicker
            style={{ width: '100%' }}
            size="large"
          />
        </Form.Item>

        <Form.Item
          name="propertyAddress"
          label="Property Address"
        >
          <Input.TextArea
            rows={2}
            placeholder="Enter property address (optional)"
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default ContractInfoModal;
