import { useEffect } from 'react';
import { Modal, Form, InputNumber, Select, Checkbox, message } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { housingSearchesApi, shulsApi } from '../../api';
import type { HousingPreferencesDto } from '../../api/types';

interface EditPreferencesModalProps {
  open: boolean;
  onClose: () => void;
  housingSearchId: string;
  applicantId: string;
  preferences?: HousingPreferencesDto;
}

const FEATURES = [
  { value: 'Garage', label: 'Garage' },
  { value: 'Finished Basement', label: 'Finished Basement' },
  { value: 'Central Air', label: 'Central Air' },
  { value: 'In-Ground Pool', label: 'In-Ground Pool' },
  { value: 'Large Backyard', label: 'Large Backyard' },
  { value: 'Modern Kitchen', label: 'Modern Kitchen' },
  { value: 'Master Suite', label: 'Master Suite' },
  { value: 'Home Office', label: 'Home Office' },
];

const MOVE_TIMELINES = [
  { value: 'Immediate', label: 'Immediate (within 1 month)' },
  { value: 'ShortTerm', label: 'Short Term (1-3 months)' },
  { value: 'MediumTerm', label: 'Medium Term (3-6 months)' },
  { value: 'LongTerm', label: 'Long Term (6+ months)' },
  { value: 'Flexible', label: 'Flexible' },
];

const EditPreferencesModal = ({
  open,
  onClose,
  housingSearchId,
  applicantId,
  preferences,
}: EditPreferencesModalProps) => {
  const queryClient = useQueryClient();
  const [form] = Form.useForm();

  // Fetch shuls from API
  const { data: shulsData, isLoading: shulsLoading } = useQuery({
    queryKey: ['shuls'],
    queryFn: () => shulsApi.getAll({ pageSize: 100 }),
    enabled: open,
  });

  const shulOptions = shulsData?.items.map(shul => ({
    value: shul.id,
    label: shul.name,
  })) || [];

  useEffect(() => {
    if (open) {
      form.setFieldsValue({
        budgetAmount: preferences?.budgetAmount,
        minBedrooms: preferences?.minBedrooms,
        minBathrooms: preferences?.minBathrooms,
        requiredFeatures: preferences?.requiredFeatures || [],
        moveTimeline: preferences?.moveTimeline,
        maxWalkingMinutes: preferences?.shulProximity?.maxWalkingMinutes,
        preferredShulIds: preferences?.shulProximity?.preferredShulIds || [],
      });
    }
  }, [open, preferences, form]);

  const updateMutation = useMutation({
    mutationFn: (values: Parameters<typeof housingSearchesApi.updatePreferences>[1]) =>
      housingSearchesApi.updatePreferences(housingSearchId, values),
    onSuccess: () => {
      message.success('Housing preferences updated');
      queryClient.invalidateQueries({ queryKey: ['applicant', applicantId] });
      queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
      onClose();
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to update preferences');
    },
  });

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();

      const request = {
        budgetAmount: values.budgetAmount || undefined,
        minBedrooms: values.minBedrooms || undefined,
        minBathrooms: values.minBathrooms || undefined,
        requiredFeatures: values.requiredFeatures?.length > 0 ? values.requiredFeatures : undefined,
        moveTimeline: values.moveTimeline || undefined,
        shulProximity: values.maxWalkingMinutes || (values.preferredShulIds && values.preferredShulIds.length > 0)
          ? {
              maxWalkingTimeMinutes: values.maxWalkingMinutes,
              preferredShulIds: values.preferredShulIds,
            }
          : undefined,
      };

      updateMutation.mutate(request);
    } catch {
      // Validation error
    }
  };

  return (
    <Modal
      title="Edit Housing Preferences"
      open={open}
      onCancel={onClose}
      onOk={handleSubmit}
      confirmLoading={updateMutation.isPending}
      width={600}
    >
      <p style={{ color: '#666', marginBottom: 16 }}>
        Update the family's housing preferences. All fields are optional.
      </p>

      <Form form={form} layout="vertical">
        <Form.Item name="budgetAmount" label="Budget">
          <InputNumber
            style={{ width: '100%' }}
            formatter={(value) => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
            parser={(value) => Number(value?.replace(/\$\s?|(,*)/g, '') || 0) as unknown as 0}
            placeholder="Enter budget"
            min={0}
            step={10000}
          />
        </Form.Item>

        <Form.Item name="minBedrooms" label="Minimum Bedrooms">
          <InputNumber min={1} max={10} placeholder="Number of bedrooms" style={{ width: 150 }} />
        </Form.Item>

        <Form.Item name="minBathrooms" label="Minimum Bathrooms">
          <InputNumber min={1} max={10} step={0.5} placeholder="Number of bathrooms" style={{ width: 150 }} />
        </Form.Item>

        <Form.Item name="requiredFeatures" label="Desired Features">
          <Checkbox.Group options={FEATURES} />
        </Form.Item>

        <Form.Item name="moveTimeline" label="Move Timeline">
          <Select placeholder="When are they looking to move?" options={MOVE_TIMELINES} allowClear />
        </Form.Item>

        <Form.Item name="maxWalkingMinutes" label="Maximum Walk to Shul (minutes)">
          <InputNumber min={1} max={60} placeholder="15" style={{ width: 150 }} />
        </Form.Item>

        <Form.Item name="preferredShulIds" label="Preferred Shuls">
          <Select
            mode="multiple"
            placeholder="Select preferred shuls"
            options={shulOptions}
            loading={shulsLoading}
            allowClear
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default EditPreferencesModal;
