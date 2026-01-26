import { Modal, Form, InputNumber, DatePicker, Select, message, Typography, Space, Spin } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { housingSearchesApi, propertiesApi } from '../../../api';
import dayjs from 'dayjs';
import type { PropertyListDto } from '../../../api/types';

const { Text } = Typography;

interface ContractInfoModalProps {
  open: boolean;
  onClose: () => void;
  housingSearchId: string;
  familyName: string;
  preselectedPropertyId?: string;
  offerAmount?: number; // Default contract price from property match offer
}

interface FormValues {
  propertyId: string;
  contractPrice: number;
  contractDate: dayjs.Dayjs;
  expectedClosingDate?: dayjs.Dayjs;
}

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 0,
  }).format(price);
};

const ContractInfoModal = ({
  open,
  onClose,
  housingSearchId,
  familyName,
  preselectedPropertyId,
  offerAmount,
}: ContractInfoModalProps) => {
  const [form] = Form.useForm<FormValues>();
  const queryClient = useQueryClient();

  // Fetch available properties (Active status)
  const { data: propertiesData, isLoading: propertiesLoading } = useQuery({
    queryKey: ['properties', 'active'],
    queryFn: () => propertiesApi.getAll({ status: 'Active', pageSize: 100 }),
    enabled: open,
  });

  const properties = propertiesData?.items || [];

  const mutation = useMutation({
    mutationFn: async (values: FormValues) => {
      return housingSearchesApi.changeStage(housingSearchId, {
        newStage: 'UnderContract',
        contract: {
          propertyId: values.propertyId,
          price: values.contractPrice,
          expectedClosingDate: values.expectedClosingDate?.toISOString(),
        },
      });
    },
    onSuccess: () => {
      message.success('Moved to Under Contract');
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
      queryClient.invalidateQueries({ queryKey: ['applicants'] });
      queryClient.invalidateQueries({ queryKey: ['properties'] });
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

  // Update form when property is selected - only update price if not already set from offer
  const handlePropertyChange = (propertyId: string) => {
    const selectedProperty = properties.find((p: PropertyListDto) => p.id === propertyId);
    if (selectedProperty) {
      const currentPrice = form.getFieldValue('contractPrice');
      // Only auto-fill if no price is set
      if (!currentPrice) {
        form.setFieldsValue({
          contractPrice: selectedProperty.price,
        });
      }
    }
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
      width={500}
    >
      <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 6 }}>
        Moving <strong>{familyName}</strong> to Under Contract
      </div>

      <Form
        form={form}
        layout="vertical"
        initialValues={{
          contractDate: dayjs(),
          propertyId: preselectedPropertyId,
          contractPrice: offerAmount,
        }}
      >
        <Form.Item
          name="propertyId"
          label="Property"
          rules={[{ required: true, message: 'Please select a property' }]}
        >
          <Select
            placeholder="Select property"
            size="large"
            showSearch
            loading={propertiesLoading}
            notFoundContent={propertiesLoading ? <Spin size="small" /> : 'No properties found'}
            filterOption={(input, option) =>
              (option?.label?.toString() || '').toLowerCase().includes(input.toLowerCase())
            }
            onChange={handlePropertyChange}
            options={properties.map((property: PropertyListDto) => ({
              value: property.id,
              label: `${property.street}, ${property.city}`,
              property,
            }))}
            optionRender={(option) => {
              const property = option.data.property as PropertyListDto;
              return (
                <Space direction="vertical" size={0} style={{ width: '100%' }}>
                  <Text strong>{property.street}</Text>
                  <Space size="large">
                    <Text type="secondary">{property.city}</Text>
                    <Text type="success">{formatPrice(property.price)}</Text>
                    <Text type="secondary">{property.bedrooms} bed · {property.bathrooms} bath</Text>
                  </Space>
                </Space>
              );
            }}
          />
        </Form.Item>

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
      </Form>
    </Modal>
  );
};

export default ContractInfoModal;
