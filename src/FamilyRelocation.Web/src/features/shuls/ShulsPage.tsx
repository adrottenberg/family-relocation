import { useState } from 'react';
import { Table, Button, Space, Tag, Input, message, Popconfirm, Card } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { ColumnsType } from 'antd/es/table';
import { shulsApi } from '../../api';
import type { ShulListDto } from '../../api/types';
import ShulFormModal from './ShulFormModal';

const { Search } = Input;

const ShulsPage = () => {
  const queryClient = useQueryClient();
  const [searchText, setSearchText] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editingShul, setEditingShul] = useState<ShulListDto | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ['shuls', searchText],
    queryFn: () => shulsApi.getAll({ search: searchText || undefined }),
  });

  const deleteMutation = useMutation({
    mutationFn: shulsApi.delete,
    onSuccess: () => {
      message.success('Shul deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['shuls'] });
    },
    onError: () => {
      message.error('Failed to delete shul');
    },
  });

  const handleAdd = () => {
    setEditingShul(null);
    setModalOpen(true);
  };

  const handleEdit = (shul: ShulListDto) => {
    setEditingShul(shul);
    setModalOpen(true);
  };

  const handleDelete = (id: string) => {
    deleteMutation.mutate(id);
  };

  const handleModalClose = () => {
    setModalOpen(false);
    setEditingShul(null);
  };

  const columns: ColumnsType<ShulListDto> = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a, b) => a.name.localeCompare(b.name),
    },
    {
      title: 'Address',
      key: 'address',
      render: (_, record) => (
        <span>
          {record.street}, {record.city}, {record.state} {record.zipCode}
        </span>
      ),
    },
    {
      title: 'Rabbi',
      dataIndex: 'rabbi',
      key: 'rabbi',
      render: (rabbi) => rabbi || '-',
    },
    {
      title: 'Denomination',
      dataIndex: 'denomination',
      key: 'denomination',
      render: (denomination) => denomination ? <Tag>{denomination}</Tag> : '-',
    },
    {
      title: 'Coordinates',
      key: 'coordinates',
      render: (_, record) => (
        record.latitude && record.longitude
          ? <Tag color="green">Geocoded</Tag>
          : <Tag color="orange">Pending</Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Button
            type="text"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
          />
          <Popconfirm
            title="Delete shul"
            description="Are you sure you want to delete this shul?"
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button type="text" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Card>
        <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ margin: 0 }}>Shuls</h2>
          <Space>
            <Search
              placeholder="Search shuls..."
              allowClear
              onSearch={setSearchText}
              style={{ width: 250 }}
              prefix={<SearchOutlined />}
            />
            <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
              Add Shul
            </Button>
          </Space>
        </div>

        <Table
          columns={columns}
          dataSource={data?.items || []}
          rowKey="id"
          loading={isLoading}
          pagination={{
            total: data?.totalCount,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} shuls`,
          }}
        />
      </Card>

      <ShulFormModal
        open={modalOpen}
        onClose={handleModalClose}
        shul={editingShul}
      />
    </div>
  );
};

export default ShulsPage;
