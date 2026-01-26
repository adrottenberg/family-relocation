import { useState, useMemo } from 'react';
import {
  Card,
  Calendar,
  Select,
  Space,
  Badge,
  Empty,
  Spin,
  message,
  Modal,
  Table,
  Tag,
  Button,
  Segmented,
  Dropdown,
} from 'antd';
import {
  CalendarOutlined,
  LeftOutlined,
  RightOutlined,
  MoreOutlined,
  CheckOutlined,
  CloseOutlined,
  SwapOutlined,
  UserOutlined,
  EnvironmentOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { showingsApi, usersApi } from '../../api';
import type { ShowingListDto, ShowingStatus } from '../../api/types';
import type { UserDto } from '../../api/endpoints/users';
import RescheduleShowingModal from './RescheduleShowingModal';
import ShowingDetailModal from './ShowingDetailModal';
import dayjs, { Dayjs } from 'dayjs';
import type { ColumnsType } from 'antd/es/table';
import type { MenuProps } from 'antd';
import { Link } from 'react-router-dom';
import { formatDate, formatTime, toUtcString } from '../../utils/datetime';
import './ShowingsCalendarPage.css';

type ViewMode = 'month' | 'week' | 'day';

const statusColors: Record<ShowingStatus, string> = {
  Scheduled: 'processing',
  Completed: 'success',
  Cancelled: 'default',
  NoShow: 'error',
};

const statusBadgeColors: Record<ShowingStatus, 'processing' | 'success' | 'default' | 'error'> = {
  Scheduled: 'processing',
  Completed: 'success',
  Cancelled: 'default',
  NoShow: 'error',
};

const ShowingsCalendarPage = () => {
  const queryClient = useQueryClient();

  const [viewMode, setViewMode] = useState<ViewMode>('month');
  const [selectedDate, setSelectedDate] = useState<Dayjs>(dayjs());
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [brokerFilter, setBrokerFilter] = useState<string>('all');
  const [rescheduleModalData, setRescheduleModalData] = useState<{
    showingId: string;
    scheduledDateTime?: string;
    propertyInfo?: { street: string; city: string };
  } | null>(null);
  const [detailModalShowingId, setDetailModalShowingId] = useState<string | null>(null);

  // Calculate date range based on view mode
  const dateRange = useMemo(() => {
    if (viewMode === 'month') {
      return {
        start: selectedDate.startOf('month').startOf('week'),
        end: selectedDate.endOf('month').endOf('week'),
      };
    } else if (viewMode === 'week') {
      return {
        start: selectedDate.startOf('week'),
        end: selectedDate.endOf('week'),
      };
    } else {
      return {
        start: selectedDate.startOf('day'),
        end: selectedDate.endOf('day'),
      };
    }
  }, [viewMode, selectedDate]);

  // Fetch users for broker dropdown
  const { data: usersData, isLoading: usersLoading } = useQuery({
    queryKey: ['users'],
    queryFn: () => usersApi.list({ limit: 100 }),
  });

  // Filter users who could be brokers (all active users for now)
  const brokers = useMemo(() => {
    return usersData?.users?.filter((u: UserDto) => u.status === 'CONFIRMED') || [];
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
    { value: 'NoShow', label: 'No Show' },
  ];

  // Fetch showings
  const { data: showings, isLoading } = useQuery({
    queryKey: ['showings', dateRange.start.format('YYYY-MM-DD'), dateRange.end.format('YYYY-MM-DD'), statusFilter],
    queryFn: () =>
      showingsApi.getAll({
        fromDateTime: toUtcString(dateRange.start.startOf('day')),
        toDateTime: toUtcString(dateRange.end.endOf('day')),
        status: statusFilter || undefined,
      }),
  });

  // Filter by broker and exclude cancelled showings on client side
  const filteredShowings = useMemo(() => {
    if (!showings) return [];
    let filtered = showings;
    // Always exclude cancelled showings from calendar view
    filtered = filtered.filter(s => s.status !== 'Cancelled');
    if (brokerFilter !== 'all') {
      if (brokerFilter === 'unassigned') {
        filtered = filtered.filter(s => !s.brokerUserId);
      } else {
        filtered = filtered.filter(s => s.brokerUserId === brokerFilter);
      }
    }
    return filtered;
  }, [showings, brokerFilter]);

  // Group showings by date (in user's timezone)
  const showingsByDate = useMemo(() => {
    const grouped: Record<string, ShowingListDto[]> = {};
    filteredShowings.forEach(showing => {
      const dateKey = formatDate(showing.scheduledDateTime, 'YYYY-MM-DD');
      if (!grouped[dateKey]) {
        grouped[dateKey] = [];
      }
      grouped[dateKey].push(showing);
    });
    // Sort times within each date
    Object.keys(grouped).forEach(date => {
      grouped[date].sort((a, b) =>
        new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime()
      );
    });
    return grouped;
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
      scheduledDateTime: showing.scheduledDateTime,
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

  // Navigation helpers
  const navigatePrev = () => {
    if (viewMode === 'month') {
      setSelectedDate(prev => prev.subtract(1, 'month'));
    } else if (viewMode === 'week') {
      setSelectedDate(prev => prev.subtract(1, 'week'));
    } else {
      setSelectedDate(prev => prev.subtract(1, 'day'));
    }
  };

  const navigateNext = () => {
    if (viewMode === 'month') {
      setSelectedDate(prev => prev.add(1, 'month'));
    } else if (viewMode === 'week') {
      setSelectedDate(prev => prev.add(1, 'week'));
    } else {
      setSelectedDate(prev => prev.add(1, 'day'));
    }
  };

  const navigateToday = () => {
    setSelectedDate(dayjs());
  };

  const getHeaderTitle = () => {
    if (viewMode === 'month') {
      return selectedDate.format('MMMM YYYY');
    } else if (viewMode === 'week') {
      const start = selectedDate.startOf('week');
      const end = selectedDate.endOf('week');
      if (start.month() === end.month()) {
        return `${start.format('MMMM D')} - ${end.format('D, YYYY')}`;
      }
      return `${start.format('MMM D')} - ${end.format('MMM D, YYYY')}`;
    } else {
      return selectedDate.format('dddd, MMMM D, YYYY');
    }
  };

  // Calculate summary stats
  const stats = useMemo(() => {
    const scheduled = filteredShowings.filter(s => s.status === 'Scheduled').length;
    const completed = filteredShowings.filter(s => s.status === 'Completed').length;
    const cancelled = filteredShowings.filter(s => s.status === 'Cancelled').length;
    const total = filteredShowings.length;
    return { scheduled, completed, cancelled, total };
  }, [filteredShowings]);

  // Render calendar cell for month view
  const dateCellRender = (date: Dayjs) => {
    const dateKey = date.format('YYYY-MM-DD');
    const dayShowings = showingsByDate[dateKey] || [];

    if (dayShowings.length === 0) return null;

    return (
      <ul className="showings-calendar-events">
        {dayShowings.slice(0, 3).map(showing => (
          <li key={showing.id} onClick={(e) => { e.stopPropagation(); setDetailModalShowingId(showing.id); }}>
            <Badge
              status={statusBadgeColors[showing.status]}
              text={
                <span className="showing-event-text">
                  {formatTime(showing.scheduledDateTime)} - {showing.applicantName}
                </span>
              }
            />
          </li>
        ))}
        {dayShowings.length > 3 && (
          <li className="more-showings">+{dayShowings.length - 3} more</li>
        )}
      </ul>
    );
  };

  // Handle calendar date select
  const onCalendarSelect = (date: Dayjs) => {
    if (viewMode === 'month') {
      // If clicking on a date in month view, zoom to that day
      setSelectedDate(date);
      setViewMode('day');
    }
  };

  // Table columns for week/day view
  const columns: ColumnsType<ShowingListDto> = [
    {
      title: 'Time',
      dataIndex: 'scheduledDateTime',
      key: 'time',
      width: 100,
      render: (dateTime: string) => formatTime(dateTime),
    },
    {
      title: 'Property',
      key: 'property',
      render: (_, record) => (
        <Link to={`/listings/${record.propertyId}`}>
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

  // Render week view
  const renderWeekView = () => {
    const weekStart = selectedDate.startOf('week');
    const days: Dayjs[] = [];
    for (let i = 0; i < 7; i++) {
      days.push(weekStart.add(i, 'day'));
    }

    return (
      <div className="showings-week-view">
        {days.map(day => {
          const dateKey = day.format('YYYY-MM-DD');
          const dayShowings = showingsByDate[dateKey] || [];
          const isToday = day.isSame(dayjs(), 'day');

          return (
            <div key={dateKey} className={`week-day-column ${isToday ? 'today' : ''}`}>
              <div className="week-day-header" onClick={() => { setSelectedDate(day); setViewMode('day'); }}>
                <div className="week-day-name">{day.format('ddd')}</div>
                <div className={`week-day-number ${isToday ? 'today-number' : ''}`}>
                  {day.format('D')}
                </div>
                {dayShowings.length > 0 && (
                  <Badge count={dayShowings.length} size="small" />
                )}
              </div>
              <div className="week-day-content">
                {dayShowings.map(showing => (
                  <div
                    key={showing.id}
                    className={`week-showing-item status-${showing.status.toLowerCase()}`}
                    onClick={() => setDetailModalShowingId(showing.id)}
                  >
                    <div className="showing-time">{formatTime(showing.scheduledDateTime)}</div>
                    <div className="showing-applicant">{showing.applicantName}</div>
                    <div className="showing-property">{showing.propertyStreet}</div>
                  </div>
                ))}
                {dayShowings.length === 0 && (
                  <div className="no-showings">-</div>
                )}
              </div>
            </div>
          );
        })}
      </div>
    );
  };

  // Render day view
  const renderDayView = () => {
    const dateKey = selectedDate.format('YYYY-MM-DD');
    const dayShowings = showingsByDate[dateKey] || [];

    return (
      <div className="showings-day-view">
        <div className="day-header">
          <CalendarOutlined />
          <span>{selectedDate.format('dddd, MMMM D, YYYY')}</span>
          <Tag>{dayShowings.length} showing{dayShowings.length !== 1 ? 's' : ''}</Tag>
        </div>
        {dayShowings.length > 0 ? (
          <Table
            dataSource={dayShowings}
            columns={columns}
            rowKey="id"
            pagination={false}
            size="middle"
            onRow={(record) => ({
              onClick: () => setDetailModalShowingId(record.id),
              style: { cursor: 'pointer' },
            })}
          />
        ) : (
          <Empty description="No showings scheduled for this day" />
        )}
      </div>
    );
  };

  return (
    <div className="showings-calendar-page">
      <Card
        title={
          <Space>
            <CalendarOutlined />
            <span>Showings Calendar</span>
          </Space>
        }
        extra={
          <Space wrap>
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
        <div className="calendar-stats">
          <Badge status="processing" text={`${stats.scheduled} Scheduled`} />
          <Badge status="success" text={`${stats.completed} Completed`} />
          <Badge status="default" text={`${stats.cancelled} Cancelled`} />
          <span style={{ color: '#999' }}>Total: {stats.total}</span>
        </div>

        {/* Calendar navigation and view toggle */}
        <div className="calendar-toolbar">
          <div className="calendar-nav">
            <Button onClick={navigateToday}>Today</Button>
            <Button icon={<LeftOutlined />} onClick={navigatePrev} />
            <Button icon={<RightOutlined />} onClick={navigateNext} />
            <span className="calendar-title">{getHeaderTitle()}</span>
          </div>
          <Segmented
            value={viewMode}
            onChange={(value) => setViewMode(value as ViewMode)}
            options={[
              { label: 'Month', value: 'month' },
              { label: 'Week', value: 'week' },
              { label: 'Day', value: 'day' },
            ]}
          />
        </div>

        {isLoading ? (
          <div style={{ textAlign: 'center', padding: 40 }}>
            <Spin size="large" />
          </div>
        ) : (
          <div className="calendar-content">
            {viewMode === 'month' && (
              <Calendar
                value={selectedDate}
                onSelect={onCalendarSelect}
                cellRender={(current, info) => {
                  if (info.type === 'date') {
                    return dateCellRender(current);
                  }
                  return null;
                }}
                headerRender={() => null}
              />
            )}
            {viewMode === 'week' && renderWeekView()}
            {viewMode === 'day' && renderDayView()}
          </div>
        )}
      </Card>

      {/* Reschedule Modal */}
      {rescheduleModalData && (
        <RescheduleShowingModal
          open={true}
          onClose={() => setRescheduleModalData(null)}
          showingId={rescheduleModalData.showingId}
          currentDateTime={rescheduleModalData.scheduledDateTime}
          propertyInfo={rescheduleModalData.propertyInfo}
        />
      )}

      {/* Detail Modal */}
      {detailModalShowingId && (
        <ShowingDetailModal
          open={true}
          onClose={() => setDetailModalShowingId(null)}
          showingId={detailModalShowingId}
        />
      )}
    </div>
  );
};

export default ShowingsCalendarPage;
