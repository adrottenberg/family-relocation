import { Table, Tag, Button, Space, Dropdown, MenuProps } from 'antd';
import {
  MoreOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ExclamationCircleOutlined,
  EditOutlined,
} from '@ant-design/icons';
import { Link } from 'react-router-dom';
import type { ColumnsType } from 'antd/es/table';
import type { ShowingListDto } from '../../api/types';
import dayjs from 'dayjs';

interface ShowingsListProps {
  showings: ShowingListDto[];
  loading?: boolean;
  onReschedule?: (id: string) => void;
  onComplete?: (id: string) => void;
  onCancel?: (id: string) => void;
  onNoShow?: (id: string) => void;
}

const statusColors: Record<string, string> = {
  Scheduled: 'blue',
  Completed: 'green',
  Cancelled: 'default',
  NoShow: 'red',
};

const statusLabels: Record<string, string> = {
  Scheduled: 'Scheduled',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  NoShow: 'No Show',
};

const ShowingsList = ({
  showings,
  loading = false,
  onReschedule,
  onComplete,
  onCancel,
  onNoShow,
}: ShowingsListProps) => {
  const getActionMenu = (showing: ShowingListDto): MenuProps['items'] => {
    if (showing.status !== 'Scheduled') return [];

    return [
      {
        key: 'reschedule',
        icon: <EditOutlined />,
        label: 'Reschedule',
        onClick: () => onReschedule?.(showing.id),
      },
      {
        key: 'complete',
        icon: <CheckCircleOutlined />,
        label: 'Mark Completed',
        onClick: () => onComplete?.(showing.id),
      },
      {
        key: 'noshow',
        icon: <ExclamationCircleOutlined />,
        label: 'Mark No Show',
        onClick: () => onNoShow?.(showing.id),
      },
      { type: 'divider' },
      {
        key: 'cancel',
        icon: <CloseCircleOutlined />,
        label: 'Cancel',
        danger: true,
        onClick: () => onCancel?.(showing.id),
      },
    ];
  };

  const columns: ColumnsType<ShowingListDto> = [
    {
      title: 'Date',
      dataIndex: 'scheduledDate',
      key: 'date',
      width: 120,
      sorter: (a, b) => dayjs(a.scheduledDate).unix() - dayjs(b.scheduledDate).unix(),
      render: (date: string) => {
        const d = dayjs(date);
        const isToday = d.isSame(dayjs(), 'day');
        return (
          <span style={{ fontWeight: isToday ? 600 : 400, color: isToday ? '#1890ff' : undefined }}>
            {isToday ? 'Today' : d.format('MMM D, YYYY')}
          </span>
        );
      },
    },
    {
      title: 'Time',
      dataIndex: 'scheduledTime',
      key: 'time',
      width: 100,
      render: (time: string) => dayjs(`2000-01-01T${time}`).format('h:mm A'),
    },
    {
      title: 'Property',
      key: 'property',
      render: (_, record) => (
        <Link to={`/properties/${record.propertyId}`}>
          {record.propertyStreet}, {record.propertyCity}
        </Link>
      ),
    },
    {
      title: 'Family',
      key: 'applicant',
      render: (_, record) => (
        <Link to={`/applicants/${record.applicantId}`}>
          {record.applicantName}
        </Link>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      filters: [
        { text: 'Scheduled', value: 'Scheduled' },
        { text: 'Completed', value: 'Completed' },
        { text: 'Cancelled', value: 'Cancelled' },
        { text: 'No Show', value: 'NoShow' },
      ],
      onFilter: (value, record) => record.status === value,
      render: (status: string) => (
        <Tag color={statusColors[status] || 'default'}>
          {statusLabels[status] || status}
        </Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      render: (_, record) => {
        const menuItems = getActionMenu(record);
        if (!menuItems || menuItems.length === 0) return null;

        return (
          <Space>
            <Dropdown menu={{ items: menuItems }} trigger={['click']}>
              <Button size="small" icon={<MoreOutlined />} />
            </Dropdown>
          </Space>
        );
      },
    },
  ];

  return (
    <Table
      columns={columns}
      dataSource={showings}
      rowKey="id"
      loading={loading}
      pagination={{ pageSize: 10 }}
      size="middle"
    />
  );
};

export default ShowingsList;
