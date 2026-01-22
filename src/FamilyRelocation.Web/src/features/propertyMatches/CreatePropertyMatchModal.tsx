import { useEffect } from 'react';
import { Modal, Form, Select, Input, message, Spin } from 'antd';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { propertyMatchesApi, propertiesApi, applicantsApi } from '../../api';
import type { ApplicantListItemDto } from '../../api/types';

const { TextArea } = Input;
const { Option } = Select;

interface CreatePropertyMatchModalProps {
  open: boolean;
  onClose: () => void;
  housingSearchId?: string;
  propertyId?: string;
}

const CreatePropertyMatchModal = ({
  open,
  onClose,
  housingSearchId,
  propertyId,
}: CreatePropertyMatchModalProps) => {
  const queryClient = useQueryClient();
  const [form] = Form.useForm();

  useEffect(() => {
    if (open) {
      form.resetFields();
      if (housingSearchId) {
        form.setFieldValue('housingSearchId', housingSearchId);
      }
      if (propertyId) {
        form.setFieldValue('propertyId', propertyId);
      }
    }
  }, [open, housingSearchId, propertyId, form]);

  // Load properties for selection
  const { data: propertiesData, isLoading: loadingProperties } = useQuery({
    queryKey: ['properties', 'list', 'active'],
    queryFn: () => propertiesApi.getAll({ status: 'Active', pageSize: 100 }),
    enabled: open && !propertyId,
  });

  // Load applicants with active housing searches
  const { data: applicantsData, isLoading: loadingApplicants } = useQuery({
    queryKey: ['applicants', 'list', 'approved'],
    queryFn: () => applicantsApi.getAll({ page: 1, pageSize: 100 }),
    enabled: open && !housingSearchId,
  });

  const createMutation = useMutation({
    mutationFn: propertyMatchesApi.create,
    onSuccess: () => {
      message.success('Property match created successfully');
      queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
      onClose();
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to create property match');
    },
  });

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      // Use prop values if provided (since those fields are hidden)
      createMutation.mutate({
        housingSearchId: housingSearchId || values.housingSearchId,
        propertyId: propertyId || values.propertyId,
        notes: values.notes,
      });
    } catch {
      // Validation error
    }
  };

  // Filter applicants to only those with housing searches
  const applicantsWithSearch = (applicantsData?.items || []).filter(
    (a: ApplicantListItemDto) => a.housingSearchId && a.stage
  );

  return (
    <Modal
      title="Create Property Match"
      open={open}
      onCancel={onClose}
      onOk={handleSubmit}
      confirmLoading={createMutation.isPending}
      width={500}
    >
      <Form form={form} layout="vertical">
        {!housingSearchId && (
          <Form.Item
            name="housingSearchId"
            label="Family"
            rules={[{ required: true, message: 'Please select a family' }]}
          >
            <Select
              placeholder="Select a family"
              loading={loadingApplicants}
              showSearch
              filterOption={(input, option) =>
                (option?.children as unknown as string)?.toLowerCase().includes(input.toLowerCase())
              }
            >
              {applicantsWithSearch.map((applicant) => (
                <Option key={applicant.housingSearchId} value={applicant.housingSearchId}>
                  {applicant.husbandFullName}
                  {applicant.stage && ` (${applicant.stage})`}
                </Option>
              ))}
            </Select>
          </Form.Item>
        )}

        {!propertyId && (
          <Form.Item
            name="propertyId"
            label="Property"
            rules={[{ required: true, message: 'Please select a property' }]}
          >
            {loadingProperties ? (
              <Spin />
            ) : (
              <Select
                placeholder="Select a property"
                showSearch
                filterOption={(input, option) =>
                  (option?.children as unknown as string)?.toLowerCase().includes(input.toLowerCase())
                }
              >
                {propertiesData?.items.map((property) => (
                  <Option key={property.id} value={property.id}>
                    {property.street}, {property.city} - ${property.price.toLocaleString()}
                  </Option>
                ))}
              </Select>
            )}
          </Form.Item>
        )}

        <Form.Item name="notes" label="Notes">
          <TextArea rows={3} placeholder="Optional notes about this match..." />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default CreatePropertyMatchModal;
