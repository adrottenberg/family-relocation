import { useEffect } from 'react';
import { Modal, Form, Input, InputNumber, message, Row, Col } from 'antd';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { shulsApi } from '../../api';
import type { ShulListDto } from '../../api/types';

const { TextArea } = Input;

interface ShulFormModalProps {
  open: boolean;
  onClose: () => void;
  shul: ShulListDto | null;
}

const ShulFormModal = ({ open, onClose, shul }: ShulFormModalProps) => {
  const queryClient = useQueryClient();
  const [form] = Form.useForm();
  const isEditing = !!shul;

  useEffect(() => {
    if (open) {
      if (shul) {
        form.setFieldsValue({
          name: shul.name,
          street: shul.street,
          city: shul.city,
          state: shul.state,
          zipCode: shul.zipCode,
          rabbi: shul.rabbi,
          denomination: shul.denomination,
          latitude: shul.latitude,
          longitude: shul.longitude,
        });
      } else {
        form.resetFields();
        // Default state to NJ
        form.setFieldValue('state', 'NJ');
      }
    }
  }, [open, shul, form]);

  const createMutation = useMutation({
    mutationFn: shulsApi.create,
    onSuccess: () => {
      message.success('Shul created successfully');
      queryClient.invalidateQueries({ queryKey: ['shuls'] });
      onClose();
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to create shul');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Parameters<typeof shulsApi.update>[1] }) =>
      shulsApi.update(id, data),
    onSuccess: () => {
      message.success('Shul updated successfully');
      queryClient.invalidateQueries({ queryKey: ['shuls'] });
      onClose();
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to update shul');
    },
  });

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();

      const data = {
        name: values.name,
        street: values.street,
        street2: values.street2 || undefined,
        city: values.city,
        state: values.state,
        zipCode: values.zipCode,
        rabbi: values.rabbi || undefined,
        denomination: values.denomination || undefined,
        website: values.website || undefined,
        notes: values.notes || undefined,
        latitude: values.latitude || undefined,
        longitude: values.longitude || undefined,
      };

      if (isEditing && shul) {
        updateMutation.mutate({ id: shul.id, data: { ...data, id: shul.id } });
      } else {
        createMutation.mutate(data);
      }
    } catch {
      // Validation error
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;

  return (
    <Modal
      title={isEditing ? 'Edit Shul' : 'Add Shul'}
      open={open}
      onCancel={onClose}
      onOk={handleSubmit}
      confirmLoading={isPending}
      width={600}
    >
      <Form form={form} layout="vertical">
        <Form.Item
          name="name"
          label="Shul Name"
          rules={[{ required: true, message: 'Please enter the shul name' }]}
        >
          <Input placeholder="e.g., Congregation Israel" />
        </Form.Item>

        <Form.Item
          name="street"
          label="Street Address"
          rules={[{ required: true, message: 'Please enter the street address' }]}
        >
          <Input placeholder="e.g., 339 Mountain Ave" />
        </Form.Item>

        <Form.Item name="street2" label="Address Line 2">
          <Input placeholder="Suite, unit, etc." />
        </Form.Item>

        <Row gutter={16}>
          <Col span={10}>
            <Form.Item
              name="city"
              label="City"
              rules={[{ required: true, message: 'Required' }]}
            >
              <Input placeholder="e.g., Springfield" />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item
              name="state"
              label="State"
              rules={[{ required: true, message: 'Required' }]}
            >
              <Input placeholder="NJ" maxLength={2} />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item
              name="zipCode"
              label="Zip Code"
              rules={[{ required: true, message: 'Required' }]}
            >
              <Input placeholder="07081" />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item name="rabbi" label="Rabbi">
              <Input placeholder="e.g., Rabbi David Cohen" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item name="denomination" label="Denomination">
              <Input placeholder="e.g., Orthodox, Young Israel, Chabad" />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item name="website" label="Website">
          <Input placeholder="https://example.com" />
        </Form.Item>

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="latitude"
              label="Latitude"
              tooltip="Leave blank to auto-geocode from address"
            >
              <InputNumber style={{ width: '100%' }} step={0.0001} placeholder="40.6892" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="longitude"
              label="Longitude"
              tooltip="Leave blank to auto-geocode from address"
            >
              <InputNumber style={{ width: '100%' }} step={0.0001} placeholder="-74.2631" />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item name="notes" label="Notes">
          <TextArea rows={3} placeholder="Additional notes about this shul..." />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default ShulFormModal;
