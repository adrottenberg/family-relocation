import { useState } from 'react';
import {
  Card,
  Table,
  Button,
  Input,
  Space,
  Tag,
  Typography,
  message,
  Popconfirm,
  Tooltip,
  Alert,
} from 'antd';
import {
  SearchOutlined,
  UserOutlined,
  CheckCircleOutlined,
  EditOutlined,
  StopOutlined,
  PlayCircleOutlined,
  ReloadOutlined,
  PlusOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import { usersApi, UserDto } from '../../api';
import EditUserRolesModal from './EditUserRolesModal';
import CreateUserModal from './CreateUserModal';

const { Title } = Typography;

const getRoleColor = (role: string) => {
  switch (role) {
    case 'Admin':
      return 'red';
    case 'Coordinator':
      return 'blue';
    case 'BoardMember':
      return 'green';
    case 'Broker':
      return 'purple';
    default:
      return 'default';
  }
};

const getStatusColor = (status: string) => {
  switch (status.toUpperCase()) {
    case 'CONFIRMED':
      return 'success';
    case 'FORCE_CHANGE_PASSWORD':
      return 'warning';
    case 'DISABLED':
      return 'error';
    default:
      return 'default';
  }
};

const UsersPage = () => {
  const queryClient = useQueryClient();
  const [searchText, setSearchText] = useState('');
  const [editingUser, setEditingUser] = useState<UserDto | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);

  const {
    data: usersData,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ['users', searchText],
    queryFn: () => usersApi.list({ search: searchText || undefined }),
  });

  const deactivateMutation = useMutation({
    mutationFn: (userId: string) => usersApi.deactivate(userId),
    onSuccess: (data) => {
      message.success(data.message);
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to deactivate user');
    },
  });

  const reactivateMutation = useMutation({
    mutationFn: (userId: string) => usersApi.reactivate(userId),
    onSuccess: (data) => {
      message.success(data.message);
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to reactivate user');
    },
  });

  const columns = [
    {
      title: 'User',
      key: 'user',
      render: (_: unknown, record: UserDto) => (
        <Space direction="vertical" size={0}>
          <Space>
            <UserOutlined />
            <span style={{ fontWeight: 500 }}>{record.name || record.email}</span>
          </Space>
          {record.name && (
            <Typography.Text type="secondary" style={{ fontSize: 12 }}>
              {record.email}
            </Typography.Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Roles',
      key: 'roles',
      render: (_: unknown, record: UserDto) => (
        <Space wrap>
          {record.roles.length > 0 ? (
            record.roles.map((role) => (
              <Tag key={role} color={getRoleColor(role)}>
                {role}
              </Tag>
            ))
          ) : (
            <Typography.Text type="secondary">No roles</Typography.Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Status',
      key: 'status',
      render: (_: unknown, record: UserDto) => (
        <Space>
          <Tag color={getStatusColor(record.status)}>{record.status}</Tag>
          {record.emailVerified && (
            <Tooltip title="Email Verified">
              <CheckCircleOutlined style={{ color: '#52c41a' }} />
            </Tooltip>
          )}
          {record.mfaEnabled && (
            <Tooltip title="MFA Enabled">
              <Tag color="cyan" style={{ fontSize: 10 }}>
                MFA
              </Tag>
            </Tooltip>
          )}
        </Space>
      ),
    },
    {
      title: 'Created',
      key: 'createdAt',
      render: (_: unknown, record: UserDto) => (
        <Typography.Text type="secondary">
          {dayjs(record.createdAt).format('MMM D, YYYY')}
        </Typography.Text>
      ),
    },
    {
      title: 'Last Login',
      key: 'lastLogin',
      render: (_: unknown, record: UserDto) => (
        <Typography.Text type="secondary">
          {record.lastLogin ? dayjs(record.lastLogin).format('MMM D, YYYY h:mm A') : 'Never'}
        </Typography.Text>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (_: unknown, record: UserDto) => (
        <Space>
          <Tooltip title="Edit Roles">
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => setEditingUser(record)}
            />
          </Tooltip>
          {record.status.toUpperCase() === 'DISABLED' ? (
            <Popconfirm
              title="Reactivate User"
              description="Are you sure you want to reactivate this user?"
              onConfirm={() => reactivateMutation.mutate(record.email)}
              okText="Yes"
              cancelText="No"
            >
              <Tooltip title="Reactivate">
                <Button
                  type="text"
                  icon={<PlayCircleOutlined style={{ color: '#52c41a' }} />}
                  loading={reactivateMutation.isPending}
                />
              </Tooltip>
            </Popconfirm>
          ) : (
            <Popconfirm
              title="Deactivate User"
              description="This will prevent the user from logging in. Continue?"
              onConfirm={() => deactivateMutation.mutate(record.email)}
              okText="Yes"
              cancelText="No"
            >
              <Tooltip title="Deactivate">
                <Button
                  type="text"
                  danger
                  icon={<StopOutlined />}
                  loading={deactivateMutation.isPending}
                />
              </Tooltip>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Card>
        <Space direction="vertical" style={{ width: '100%' }} size="large">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Title level={4} style={{ margin: 0 }}>
              <UserOutlined style={{ marginRight: 8 }} />
              User Management
            </Title>
            <Space>
              <Input
                placeholder="Search by email..."
                prefix={<SearchOutlined />}
                value={searchText}
                onChange={(e) => setSearchText(e.target.value)}
                style={{ width: 250 }}
                allowClear
              />
              <Tooltip title="Refresh">
                <Button icon={<ReloadOutlined />} onClick={() => refetch()} />
              </Tooltip>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => setShowCreateModal(true)}
              >
                Add User
              </Button>
            </Space>
          </div>

          <Table
            columns={columns}
            dataSource={usersData?.users || []}
            rowKey="id"
            loading={isLoading}
            pagination={{
              pageSize: 20,
              showSizeChanger: false,
              showTotal: (total) => `${total} users`,
            }}
          />
        </Space>
      </Card>

      <EditUserRolesModal
        open={!!editingUser}
        user={editingUser}
        onClose={() => setEditingUser(null)}
        onSuccess={() => {
          setEditingUser(null);
          queryClient.invalidateQueries({ queryKey: ['users'] });
        }}
      />

      <CreateUserModal
        open={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        onSuccess={() => {
          queryClient.invalidateQueries({ queryKey: ['users'] });
        }}
      />
    </div>
  );
};

export default UsersPage;
