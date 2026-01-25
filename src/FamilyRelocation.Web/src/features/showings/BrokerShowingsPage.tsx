import { useState, useMemo } from 'react';
import { Card, Select, Space, Badge, Empty, Spin, message, Modal, Table, Tag, Button, DatePicker, Dropdown } from 'antd';
import { CalendarOutlined, UserOutlined, MoreOutlined, CheckOutlined, CloseOutlined, SwapOutlined, EnvironmentOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { showingsApi, usersApi } from '../../api';
import type { ShowingListDto, ShowingStatus } from '../../api/types';
import type { UserDto } from '../../api/endpoints/users';
import RescheduleShowingModal from './RescheduleShowingModal';
import dayjs, { Dayjs } from 'dayjs';
import type { ColumnsType } from 'antd/es/table';
import type { MenuProps } from 'antd';
import { Link } from 'react-router-dom';

const { RangePicker } = DatePicker;

const statusColors: Record<ShowingStatus, string> = {
  Scheduled: 'processing',
  Completed: 'success',
  Cancelled: 'default',
  NoShow: 'error',
};

const BrokerShowingsPage = () => {
  const queryClient = useQueryClient();

  const [dateRange, setDateRange] = useState<[Dayjs | null, Dayjs | null]>([
    dayjs().startOf('month'),
    dayjs().endOf('month'),
  ]);
  const [statusFilter, setStatusFilter] = useState<string>('Scheduled');
  const [brokerFilter, setBrokerFilter] = useState<string>('all');
  const [rescheduleModalData, setRescheduleModalData] = useState<{
    showingId: string;
    date?: string;
    time?: string;
    propertyInfo?: { street: string; city: string };
  } | null>(null);

  // Fetch users for broker dropdown
  const { data: usersData, isLoading: usersLoading } = useQuery({
    queryKey: ['users'],
    queryFn: () => usersApi.list({ limit: 100 }),
  });

  // Filter users who could be brokers (all active users for now)
  const brokers = useMemo(() => {
    return usersData?.users.filter((u: UserDto) => u.status === 'CONFIRMED') || [];
  }, [usersData]);

  const brokerOptions = [
    { value: 'all', label: 'All Brokers' },
    { value: 'unassigned', label: 'Unassigned' },
    ...brokers.map((b: UserDto) => ({
      value: b.id,
      label: b.name || b.email,
    })),
  ];

  const statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'Scheduled', label: 'Scheduled' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Cancelled', label: 'Cancelled' },
    { value: 'NoShow', label: 'No Show' },
  ];

  // Fetch showings
  const { data: showings, isLoading } = useQuery({
    queryKey: ['showings', dateRange[0]?.format('YYYY-MM-DD'), dateRange[1]?.format('YYYY-MM-DD'), statusFilter],
    queryFn: () =>
      showingsApi.getAll({
        fromDate: dateRange[0]?.format('YYYY-MM-DD'),
        toDate: dateRange[1]?.format('YYYY-MM-DD'),
        status: statusFilter || undefined,
      }),
  });

  // Filter by broker on client side
  const filteredShowings = useMemo(() => {
    if (!showings) return [];
    if (brokerFilter === 'all') return showings;
    if (brokerFilter === 'unassigned') return showings.filter(s => !s.brokerUserId);
    return showings.filter(s => s.brokerUserId === brokerFilter);
  }, [showings, brokerFilter]);

  // Group showings by date
  const showingsByDate = useMemo(() => {
    const grouped: Record<string, ShowingListDto[]> = {};
    filteredShowings.forEach(showing => {
      if (!grouped[showing.scheduledDate]) {
        grouped[showing.scheduledDate] = [];
      }
      grouped[showing.scheduledDate].push(showing);
    });
    // Sort dates
    return Object.entries(grouped)
      .sort(([a], [b]) => a.localeCompare(b))
      .reduce((acc, [date, items]) => {
        acc[date] = items.sort((a, b) => a.scheduledTime.localeCompare(b.scheduledTime));
        return acc;
      }, {} as Record<string, ShowingListDto[]>);
  }, [filteredShowings]);

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status, notes }: { id: string; status: ShowingStatus; notes?: string }) =>
      showingsApi.updateStatus(id, { status, notes }),
    onSuccess: () => {
      message.success('Showing status updated');
      queryClient.invalidateQueries({ queryKey: ['showings'] });
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to update showing');
    },
  });

  const handleComplete = (id: string) => {
    Modal.confirm({
      title: 'Mark as Completed',
      content: 'Are you sure you want to mark this showing as completed?',
      onOk: () => updateStatusMutation.mutate({ id, status: 'Completed' }),
    });
  };

  const handleCancel = (id: string) => {
    Modal.confirm({
      title: 'Cancel Showing',
      content: 'Are you sure you want to cancel this showing?',
      okText: 'Yes, Cancel',
      okButtonProps: { danger: true },
      onOk: () => updateStatusMutation.mutate({ id, status: 'Cancelled' }),
    });
  };

  const handleNoShow = (id: string) => {
    Modal.confirm({
      title: 'Mark as No Show',
      content: 'Are you sure you want to mark this showing as no show?',
      onOk: () => updateStatusMutation.mutate({ id, status: 'NoShow' }),
    });
  };

  const handleReschedule = (showing: ShowingListDto) => {
    setRescheduleModalData({
      showingId: showing.id,
      date: showing.scheduledDate,
      time: showing.scheduledTime,
      propertyInfo: {
        street: showing.propertyStreet,
        city: showing.propertyCity,
      },
    });
  };

  const getActionMenuItems = (showing: ShowingListDto): MenuProps['items'] => {
    const items: MenuProps['items'] = [];

    if (showing.status === 'Scheduled') {
      items.push(
        {
          key: 'complete',
          icon: <CheckOutlined />,
          label: 'Mark Completed',
          onClick: () => handleComplete(showing.id),
        },
        {
          key: 'reschedule',
          icon: <SwapOutlined />,
          label: 'Reschedule',
          onClick: () => handleReschedule(showing),
        },
        {
          key: 'noshow',
          icon: <UserOutlined />,
          label: 'No Show',
          onClick: () => handleNoShow(showing.id),
        },
        { type: 'divider' },
        {
          key: 'cancel',
          icon: <CloseOutlined />,
          label: 'Cancel',
          danger: true,
          onClick: () => handleCancel(showing.id),
        }
      );
    }

    return items;
  };

  const columns: ColumnsType<ShowingListDto> = [
    {
      title: 'Time',
      dataIndex: 'scheduledTime',
      key: 'time',
      width: 100,
      render: (time: string) => time?.substring(0, 5),
    },
    {
      title: 'Property',
      key: 'property',
      render: (_, record) => (
        <Link to={`/properties/${record.propertyId}`}>
          <Space>
            <EnvironmentOutlined />
            <span>{record.propertyStreet}, {record.propertyCity}</span>
          </Space>
        </Link>
      ),
    },
    {
      title: 'Applicant',
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
      render: (status: ShowingStatus) => (
        <Tag color={statusColors[status]}>{status}</Tag>
      ),
    },
    {
      title: '',
      key: 'actions',
      width: 50,
      render: (_, record) => {
        const menuItems = getActionMenuItems(record);
        if (!menuItems || menuItems.length === 0) return null;
        return (
          <Dropdown menu={{ items: menuItems }} trigger={['click']}>
            <Button type="text" icon={<MoreOutlined />} />
          </Dropdown>
        );
      },
    },
  ];

  // Calculate summary stats
  const stats = useMemo(() => {
    const scheduled = filteredShowings.filter(s => s.status === 'Scheduled').length;
    const completed = filteredShowings.filter(s => s.status === 'Completed').length;
    const cancelled = filteredShowings.filter(s => s.status === 'Cancelled').length;
    const total = filteredShowings.length;
    return { scheduled, completed, cancelled, total };
  }, [filteredShowings]);

  return (
    <div style={{ padding: 24 }}>
      <Card
        title={
          <Space>
            <CalendarOutlined />
            <span>Broker Showings</span>
          </Space>
        }
        extra={
          <Space wrap>
            <RangePicker
              value={dateRange}
              onChange={(dates) => setDateRange(dates as [Dayjs | null, Dayjs | null])}
              allowClear={false}
            />
            <Select
              value={brokerFilter}
              onChange={setBrokerFilter}
              options={brokerOptions}
              style={{ width: 180 }}
              loading={usersLoading}
              placeholder="Select broker"
            />
            <Select
              value={statusFilter}
              onChange={setStatusFilter}
              options={statusOptions}
              style={{ width: 140 }}
            />
          </Space>
        }
      >
        {/* Stats row */}
        <div style={{ marginBottom: 24, display: 'flex', gap: 24 }}>
          <Badge status="processing" text={`${stats.scheduled} Scheduled`} />
          <Badge status="success" text={`${stats.completed} Completed`} />
          <Badge status="default" text={`${stats.cancelled} Cancelled`} />
          <span style={{ color: '#999' }}>Total: {stats.total}</span>
        </div>

        {isLoading ? (
          <div style={{ textAlign: 'center', padding: 40 }}>
            <Spin size="large" />
          </div>
        ) : Object.keys(showingsByDate).length > 0 ? (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
            {Object.entries(showingsByDate).map(([date, dayShowings]) => (
              <Card
                key={date}
                size="small"
                title={
                  <Space>
                    <CalendarOutlined />
                    <span>{dayjs(date).format('dddd, MMMM D, YYYY')}</span>
                    <Tag>{dayShowings.length} showing{dayShowings.length !== 1 ? 's' : ''}</Tag>
                  </Space>
                }
                styles={{ body: { padding: 0 } }}
              >
                <Table
                  dataSource={dayShowings}
                  columns={columns}
                  rowKey="id"
                  pagination={false}
                  size="small"
                />
              </Card>
            ))}
          </div>
        ) : (
          <Empty description="No showings found" />
        )}
      </Card>

      {/* Reschedule Modal */}
      {rescheduleModalData && (
        <RescheduleShowingModal
          open={true}
          onClose={() => setRescheduleModalData(null)}
          showingId={rescheduleModalData.showingId}
          currentDate={rescheduleModalData.date}
          currentTime={rescheduleModalData.time}
          propertyInfo={rescheduleModalData.propertyInfo}
        />
      )}
    </div>
  );
};

export default BrokerShowingsPage;
