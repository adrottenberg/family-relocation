import { useState } from 'react';
import { Modal, Form, Input, Checkbox, Space, Typography, message, Alert, Result, Button } from 'antd';
import { UserAddOutlined, CopyOutlined, CheckOutlined } from '@ant-design/icons';
import { useMutation } from '@tanstack/react-query';
import { usersApi, CreateUserResponse } from '../../api';

const { Text } = Typography;

interface CreateUserModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

const AVAILABLE_ROLES = [
  {
    value: 'Admin',
    label: 'Admin',
    description: 'Full system access including user management',
  },
  {
    value: 'Coordinator',
    label: 'Coordinator',
    description: 'Manage applicants, housing searches, and properties',
  },
  {
    value: 'BoardMember',
    label: 'Board Member',
    description: 'View applicants and approve board reviews',
  },
];

const CreateUserModal = ({ open, onClose, onSuccess }: CreateUserModalProps) => {
  const [form] = Form.useForm();
  const [createdUser, setCreatedUser] = useState<CreateUserResponse | null>(null);
  const [copied, setCopied] = useState(false);

  const createUserMutation = useMutation({
    mutationFn: (values: { email: string; roles: string[] }) =>
      usersApi.create({ email: values.email, roles: values.roles }),
    onSuccess: (data) => {
      setCreatedUser(data);
      message.success('User created successfully');
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to create user');
    },
  });

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      createUserMutation.mutate(values);
    } catch {
      // Validation failed
    }
  };

  const handleClose = () => {
    form.resetFields();
    setCreatedUser(null);
    setCopied(false);
    onClose();
    if (createdUser) {
      onSuccess();
    }
  };

  const copyCredentials = () => {
    if (createdUser) {
      const text = `Email: ${createdUser.email}\nTemporary Password: ${createdUser.temporaryPassword}\n\nPlease log in and change your password.`;
      navigator.clipboard.writeText(text);
      setCopied(true);
      message.success('Credentials copied to clipboard');
      setTimeout(() => setCopied(false), 3000);
    }
  };

  return (
    <Modal
      title={
        <Space>
          <UserAddOutlined />
          Create New User
        </Space>
      }
      open={open}
      onCancel={handleClose}
      onOk={createdUser ? handleClose : handleSubmit}
      okText={createdUser ? 'Done' : 'Create User'}
      okButtonProps={{
        loading: createUserMutation.isPending,
      }}
      cancelButtonProps={{
        style: createdUser ? { display: 'none' } : undefined,
      }}
      width={550}
    >
      {createdUser ? (
        <Result
          status="success"
          title="User Created Successfully"
          subTitle="Share these credentials with the new user. They will be required to change their password on first login."
          extra={
            <Space direction="vertical" style={{ width: '100%' }}>
              <Alert
                type="info"
                message={
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div>
                      <Text strong>Email: </Text>
                      <Text copyable>{createdUser.email}</Text>
                    </div>
                    <div>
                      <Text strong>Temporary Password: </Text>
                      <Text copyable code>
                        {createdUser.temporaryPassword}
                      </Text>
                    </div>
                    {createdUser.roles.length > 0 && (
                      <div>
                        <Text strong>Roles: </Text>
                        <Text>{createdUser.roles.join(', ')}</Text>
                      </div>
                    )}
                  </Space>
                }
              />
              <Button
                icon={copied ? <CheckOutlined /> : <CopyOutlined />}
                onClick={copyCredentials}
                type={copied ? 'default' : 'primary'}
              >
                {copied ? 'Copied!' : 'Copy Credentials'}
              </Button>
            </Space>
          }
        />
      ) : (
        <Form form={form} layout="vertical" initialValues={{ roles: [] }}>
          <Form.Item
            name="email"
            label="Email Address"
            rules={[
              { required: true, message: 'Email is required' },
              { type: 'email', message: 'Please enter a valid email address' },
            ]}
          >
            <Input placeholder="user@example.com" autoFocus />
          </Form.Item>

          <Form.Item name="roles" label="Roles (optional)">
            <Checkbox.Group style={{ width: '100%' }}>
              <Space direction="vertical" style={{ width: '100%' }}>
                {AVAILABLE_ROLES.map((role) => (
                  <div
                    key={role.value}
                    style={{
                      padding: '12px 16px',
                      border: '1px solid #d9d9d9',
                      borderRadius: 6,
                    }}
                  >
                    <Checkbox value={role.value} style={{ width: '100%' }}>
                      <Space direction="vertical" size={0}>
                        <Text strong>{role.label}</Text>
                        <Text type="secondary" style={{ fontSize: 12 }}>
                          {role.description}
                        </Text>
                      </Space>
                    </Checkbox>
                  </div>
                ))}
              </Space>
            </Checkbox.Group>
          </Form.Item>

          <Alert
            type="info"
            showIcon
            message="A temporary password will be generated automatically"
            description="The new user will be required to change their password on first login."
          />
        </Form>
      )}
    </Modal>
  );
};

export default CreateUserModal;
