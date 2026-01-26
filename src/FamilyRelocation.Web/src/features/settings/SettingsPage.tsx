import {
  Card,
  Table,
  Tag,
  Typography,
  Alert,
  Spin,
  Empty,
  Button,
  Modal,
  Form,
  Input,
  Select,
  Space,
  Popconfirm,
  message,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getDocumentTypes,
  getAllStageRequirements,
  getApiError,
  createDocumentType,
  updateDocumentType,
  deleteDocumentType,
  createStageRequirement,
  deleteStageRequirement,
} from '../../api';
import type { DocumentTypeDto, StageTransitionRequirementDto } from '../../api/types';
import './SettingsPage.css';
import { useState } from 'react';

const { Title, Text } = Typography;

type SettingsSection = 'documentTypes' | 'stageRequirements';

interface SettingsPageProps {
  section?: SettingsSection;
}

const STAGES = [
  { value: 'AwaitingAgreements', label: 'Awaiting Agreements' },
  { value: 'Searching', label: 'Searching' },
  { value: 'UnderContract', label: 'Under Contract' },
  { value: 'Closed', label: 'Closed' },
  { value: 'MovedIn', label: 'Moved In' },
  { value: 'Paused', label: 'Paused' },
];

const SettingsPage = ({ section = 'documentTypes' }: SettingsPageProps) => {
  const renderContent = () => {
    switch (section) {
      case 'documentTypes':
        return <DocumentTypesSection />;
      case 'stageRequirements':
        return <StageRequirementsSection />;
      default:
        return <DocumentTypesSection />;
    }
  };

  return (
    <div className="settings-page">
      <div className="settings-content-full">
        {renderContent()}
      </div>
    </div>
  );
};

const DocumentTypesSection = () => {
  const queryClient = useQueryClient();
  const [modalOpen, setModalOpen] = useState(false);
  const [editingType, setEditingType] = useState<DocumentTypeDto | null>(null);
  const [form] = Form.useForm();

  const { data: documentTypes, isLoading, error } = useQuery({
    queryKey: ['documentTypes', false],
    queryFn: () => getDocumentTypes(false),
  });

  const createMutation = useMutation({
    mutationFn: createDocumentType,
    onSuccess: () => {
      message.success('Document type created');
      queryClient.invalidateQueries({ queryKey: ['documentTypes'] });
      setModalOpen(false);
      form.resetFields();
    },
    onError: (err) => message.error(getApiError(err).message),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: { displayName: string; description?: string } }) =>
      updateDocumentType(id, data),
    onSuccess: () => {
      message.success('Document type updated');
      queryClient.invalidateQueries({ queryKey: ['documentTypes'] });
      setModalOpen(false);
      setEditingType(null);
      form.resetFields();
    },
    onError: (err) => message.error(getApiError(err).message),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteDocumentType,
    onSuccess: () => {
      message.success('Document type deactivated');
      queryClient.invalidateQueries({ queryKey: ['documentTypes'] });
    },
    onError: (err) => message.error(getApiError(err).message),
  });

  const handleSubmit = (values: { name?: string; displayName: string; description?: string }) => {
    if (editingType) {
      updateMutation.mutate({
        id: editingType.id,
        data: { displayName: values.displayName, description: values.description },
      });
    } else {
      createMutation.mutate({
        name: values.name!,
        displayName: values.displayName,
        description: values.description,
      });
    }
  };

  const openEditModal = (record: DocumentTypeDto) => {
    setEditingType(record);
    form.setFieldsValue({
      displayName: record.displayName,
      description: record.description,
    });
    setModalOpen(true);
  };

  const columns = [
    {
      title: 'Display Name',
      dataIndex: 'displayName',
      key: 'displayName',
      render: (text: string) => <Text strong>{text}</Text>,
    },
    {
      title: 'Internal Name',
      dataIndex: 'name',
      key: 'name',
      render: (text: string) => <Text type="secondary" code>{text}</Text>,
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      render: (text: string | undefined) => text || <Text type="secondary">-</Text>,
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 100,
      render: (isActive: boolean) => (
        <Tag color={isActive ? 'success' : 'default'}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
    },
    {
      title: 'Type',
      dataIndex: 'isSystemType',
      key: 'isSystemType',
      width: 100,
      render: (isSystemType: boolean) => (
        <Tag color={isSystemType ? 'blue' : 'default'}>
          {isSystemType ? 'System' : 'Custom'}
        </Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_: unknown, record: DocumentTypeDto) => (
        <Space>
          <Button
            type="text"
            size="small"
            icon={<EditOutlined />}
            onClick={() => openEditModal(record)}
          />
          {!record.isSystemType && record.isActive && (
            <Popconfirm
              title="Deactivate this document type?"
              onConfirm={() => deleteMutation.mutate(record.id)}
            >
              <Button type="text" size="small" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  if (isLoading) {
    return (
      <Card title="Document Types">
        <div style={{ textAlign: 'center', padding: 40 }}><Spin /></div>
      </Card>
    );
  }

  if (error) {
    return (
      <Card title="Document Types">
        <Alert type="error" message="Failed to load document types" description={getApiError(error).message} showIcon />
      </Card>
    );
  }

  return (
    <Card
      title={<Title level={4} style={{ margin: 0 }}>Document Types</Title>}
      extra={
        <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditingType(null); form.resetFields(); setModalOpen(true); }}>
          Add Document Type
        </Button>
      }
    >
      <Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
        Document types that can be uploaded for applicants. System types cannot be deleted.
      </Text>
      {documentTypes && documentTypes.length > 0 ? (
        <Table dataSource={documentTypes} columns={columns} rowKey="id" pagination={false} size="small" />
      ) : (
        <Empty description="No document types configured" />
      )}

      <Modal
        title={editingType ? 'Edit Document Type' : 'Add Document Type'}
        open={modalOpen}
        onCancel={() => { setModalOpen(false); setEditingType(null); form.resetFields(); }}
        onOk={() => form.submit()}
        confirmLoading={createMutation.isPending || updateMutation.isPending}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          {!editingType && (
            <Form.Item
              name="name"
              label="Internal Name"
              rules={[{ required: true, message: 'Required' }, { pattern: /^[A-Za-z][A-Za-z0-9]*$/, message: 'Letters and numbers only, start with letter' }]}
              tooltip="Used internally (e.g., BrokerAgreement)"
            >
              <Input placeholder="e.g., BrokerAgreement" />
            </Form.Item>
          )}
          <Form.Item name="displayName" label="Display Name" rules={[{ required: true, message: 'Required' }]}>
            <Input placeholder="e.g., Broker Agreement" />
          </Form.Item>
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} placeholder="Optional description" />
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};

const StageRequirementsSection = () => {
  const queryClient = useQueryClient();
  const [modalOpen, setModalOpen] = useState(false);
  const [form] = Form.useForm();

  const { data: requirements, isLoading, error } = useQuery({
    queryKey: ['allStageRequirements'],
    queryFn: () => getAllStageRequirements(),
  });

  const { data: documentTypes } = useQuery({
    queryKey: ['documentTypes', true],
    queryFn: () => getDocumentTypes(true),
  });

  const createMutation = useMutation({
    mutationFn: createStageRequirement,
    onSuccess: () => {
      message.success('Requirement created');
      queryClient.invalidateQueries({ queryKey: ['allStageRequirements'] });
      setModalOpen(false);
      form.resetFields();
    },
    onError: (err) => message.error(getApiError(err).message),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteStageRequirement,
    onSuccess: () => {
      message.success('Requirement deleted');
      queryClient.invalidateQueries({ queryKey: ['allStageRequirements'] });
    },
    onError: (err) => message.error(getApiError(err).message),
  });

  const formatStageName = (stage: string) => {
    const found = STAGES.find((s) => s.value === stage);
    return found?.label || stage;
  };

  const columns = [
    {
      title: 'From Stage',
      dataIndex: 'fromStage',
      key: 'fromStage',
      render: (stage: string) => <Tag>{formatStageName(stage)}</Tag>,
    },
    {
      title: 'To Stage',
      dataIndex: 'toStage',
      key: 'toStage',
      render: (stage: string) => <Tag color="blue">{formatStageName(stage)}</Tag>,
    },
    {
      title: 'Required Document',
      dataIndex: 'documentTypeName',
      key: 'documentTypeName',
      render: (name: string) => <Text strong>{name}</Text>,
    },
    {
      title: 'Requirement',
      dataIndex: 'isRequired',
      key: 'isRequired',
      width: 120,
      render: (isRequired: boolean) => (
        <Tag color={isRequired ? 'red' : 'orange'}>
          {isRequired ? 'Required' : 'Optional'}
        </Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 80,
      render: (_: unknown, record: StageTransitionRequirementDto) => (
        <Popconfirm title="Delete this requirement?" onConfirm={() => deleteMutation.mutate(record.id)}>
          <Button type="text" size="small" danger icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ];

  if (isLoading) {
    return (
      <Card title="Stage Transition Requirements">
        <div style={{ textAlign: 'center', padding: 40 }}><Spin /></div>
      </Card>
    );
  }

  if (error) {
    return (
      <Card title="Stage Transition Requirements">
        <Alert type="error" message="Failed to load requirements" description={getApiError(error).message} showIcon />
      </Card>
    );
  }

  return (
    <Card
      title={<Title level={4} style={{ margin: 0 }}>Stage Transition Requirements</Title>}
      extra={
        <Button type="primary" icon={<PlusOutlined />} onClick={() => { form.resetFields(); setModalOpen(true); }}>
          Add Requirement
        </Button>
      }
    >
      <Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
        Documents required before an applicant can move between pipeline stages.
      </Text>
      {requirements && requirements.length > 0 ? (
        <Table dataSource={requirements} columns={columns} rowKey="id" pagination={false} size="small" />
      ) : (
        <Empty description="No stage requirements configured" />
      )}

      <Modal
        title="Add Stage Requirement"
        open={modalOpen}
        onCancel={() => { setModalOpen(false); form.resetFields(); }}
        onOk={() => form.submit()}
        confirmLoading={createMutation.isPending}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={(values) => createMutation.mutate(values)}
          initialValues={{ isRequired: true }}
        >
          <Form.Item name="fromStage" label="From Stage" rules={[{ required: true, message: 'Required' }]}>
            <Select options={STAGES} placeholder="Select stage" />
          </Form.Item>
          <Form.Item name="toStage" label="To Stage" rules={[{ required: true, message: 'Required' }]}>
            <Select options={STAGES} placeholder="Select stage" />
          </Form.Item>
          <Form.Item name="documentTypeId" label="Document Type" rules={[{ required: true, message: 'Required' }]}>
            <Select
              placeholder="Select document type"
              options={documentTypes?.map((dt) => ({ value: dt.id, label: dt.displayName }))}
            />
          </Form.Item>
          <Form.Item name="isRequired" label="Is Required">
            <Select
              options={[
                { value: true, label: 'Required' },
                { value: false, label: 'Optional' },
              ]}
            />
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};

export default SettingsPage;
