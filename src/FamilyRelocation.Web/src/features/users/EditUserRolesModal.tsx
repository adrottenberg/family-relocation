import { useEffect, useState } from 'react';
import { Modal, Checkbox, Space, Typography, message, Alert } from 'antd';
import { useMutation } from '@tanstack/react-query';
import { usersApi, UserDto } from '../../api';

const { Text } = Typography;

interface EditUserRolesModalProps {
  open: boolean;
  user: UserDto | null;
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

const EditUserRolesModal = ({ open, user, onClose, onSuccess }: EditUserRolesModalProps) => {
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);

  useEffect(() => {
    if (user) {
      setSelectedRoles(user.roles);
    }
  }, [user]);

  const updateRolesMutation = useMutation({
    mutationFn: () => usersApi.updateRoles(user!.email, selectedRoles),
    onSuccess: (data) => {
      message.success(data.message);
      onSuccess();
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to update roles');
    },
  });

  const handleRoleChange = (role: string, checked: boolean) => {
    if (checked) {
      setSelectedRoles([...selectedRoles, role]);
    } else {
      setSelectedRoles(selectedRoles.filter((r) => r !== role));
    }
  };

  const hasChanges = user && JSON.stringify([...selectedRoles].sort()) !== JSON.stringify([...user.roles].sort());

  return (
    <Modal
      title={`Edit Roles - ${user?.name || user?.email}`}
      open={open}
      onCancel={onClose}
      onOk={() => updateRolesMutation.mutate()}
      okText="Save Changes"
      okButtonProps={{
        disabled: !hasChanges,
        loading: updateRolesMutation.isPending,
      }}
      width={500}
    >
      {user && (
        <Space direction="vertical" style={{ width: '100%' }} size="middle">
          <Text type="secondary">
            Select the roles for this user. Users can have multiple roles.
          </Text>

          {selectedRoles.length === 0 && (
            <Alert
              type="warning"
              message="User has no roles"
              description="Users without roles will have limited access to the system."
              showIcon
            />
          )}

          <Space direction="vertical" style={{ width: '100%' }}>
            {AVAILABLE_ROLES.map((role) => (
              <div
                key={role.value}
                style={{
                  padding: '12px 16px',
                  border: '1px solid #d9d9d9',
                  borderRadius: 6,
                  backgroundColor: selectedRoles.includes(role.value) ? '#f0f5ff' : '#fff',
                }}
              >
                <Checkbox
                  checked={selectedRoles.includes(role.value)}
                  onChange={(e) => handleRoleChange(role.value, e.target.checked)}
                  style={{ width: '100%' }}
                >
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
        </Space>
      )}
    </Modal>
  );
};

export default EditUserRolesModal;
