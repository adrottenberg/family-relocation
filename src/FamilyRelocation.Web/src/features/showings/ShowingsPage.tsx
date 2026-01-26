import { useState } from 'react';
import { Card, Segmented, DatePicker, Select, Space, Badge, Empty, Spin, message, Modal, Calendar } from 'antd';
import { UnorderedListOutlined, CalendarOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { showingsApi } from '../../api';
import type { ShowingListDto, ShowingStatus } from '../../api/types';
import ShowingsList from './ShowingsList';
import ShowingCard from './ShowingCard';
import RescheduleShowingModal from './RescheduleShowingModal';
import dayjs, { Dayjs } from 'dayjs';
import { formatDate as formatDateUtil, toUtcString } from '../../utils/datetime';

const { RangePicker } = DatePicker;

type ViewMode = 'list' | 'calendar';

const statusOptions = [
  { value: '', label: 'All Statuses' },
  { value: 'Scheduled', label: 'Scheduled' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Cancelled', label: 'Cancelled' },
  { value: 'NoShow', label: 'No Show' },
];

const ShowingsPage = () => {
  const queryClient = useQueryClient();
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [dateRange, setDateRange] = useState<[Dayjs | null, Dayjs | null]>([
    dayjs().startOf('month'),
    dayjs().endOf('month'),
  ]);
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [rescheduleModalData, setRescheduleModalData] = useState<{
    showingId: string;
    scheduledDateTime?: string;
    propertyInfo?: { street: string; city: string };
  } | null>(null);

  const { data: showings, isLoading } = useQuery({
    queryKey: ['showings', dateRange[0]?.format('YYYY-MM-DD'), dateRange[1]?.format('YYYY-MM-DD'), statusFilter],
    queryFn: () =>
      showingsApi.getAll({
        fromDateTime: dateRange[0] ? toUtcString(dateRange[0].startOf('day')) : undefined,
        toDateTime: dateRange[1] ? toUtcString(dateRange[1].endOf('day')) : undefined,
        status: statusFilter || undefined,
      }),
  });

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

  const handleRescheduleById = (id: string) => {
    const showing = showings?.find((s) => s.id === id);
    if (showing) {
      handleReschedule(showing);
    }
  };

  // Group showings by date for calendar view
  const showingsByDate = (showings || []).reduce<Record<string, ShowingListDto[]>>((acc, showing) => {
    const date = formatDateUtil(showing.scheduledDateTime, 'YYYY-MM-DD');
    if (!acc[date]) acc[date] = [];
    acc[date].push(showing);
    return acc;
  }, {});

  const dateCellRender = (value: Dayjs) => {
    const dateStr = value.format('YYYY-MM-DD');
    const dayShowings = showingsByDate[dateStr] || [];

    if (dayShowings.length === 0) return null;

    const scheduledCount = dayShowings.filter((s) => s.status === 'Scheduled').length;
    const completedCount = dayShowings.filter((s) => s.status === 'Completed').length;

    return (
      <div style={{ padding: '2px 0' }}>
        {scheduledCount > 0 && (
          <Badge status="processing" text={`${scheduledCount} scheduled`} style={{ fontSize: 11, display: 'block' }} />
        )}
        {completedCount > 0 && (
          <Badge status="success" text={`${completedCount} completed`} style={{ fontSize: 11, display: 'block' }} />
        )}
        {dayShowings.length - scheduledCount - completedCount > 0 && (
          <Badge
            status="default"
            text={`${dayShowings.length - scheduledCount - completedCount} other`}
            style={{ fontSize: 11, display: 'block' }}
          />
        )}
      </div>
    );
  };

  const [selectedDate, setSelectedDate] = useState<Dayjs | null>(null);
  const selectedDateShowings = selectedDate ? showingsByDate[selectedDate.format('YYYY-MM-DD')] || [] : [];

  return (
    <div style={{ padding: 24 }}>
      <Card
        title="Showings"
        extra={
          <Space>
            <RangePicker
              value={dateRange}
              onChange={(dates) => setDateRange(dates as [Dayjs | null, Dayjs | null])}
              allowClear={false}
            />
            <Select
              value={statusFilter}
              onChange={setStatusFilter}
              options={statusOptions}
              style={{ width: 140 }}
            />
            <Segmented
              value={viewMode}
              onChange={(value) => setViewMode(value as ViewMode)}
              options={[
                { value: 'list', icon: <UnorderedListOutlined /> },
                { value: 'calendar', icon: <CalendarOutlined /> },
              ]}
            />
          </Space>
        }
      >
        {isLoading ? (
          <div style={{ textAlign: 'center', padding: 40 }}>
            <Spin size="large" />
          </div>
        ) : viewMode === 'list' ? (
          showings && showings.length > 0 ? (
            <ShowingsList
              showings={showings}
              onReschedule={handleRescheduleById}
              onComplete={handleComplete}
              onCancel={handleCancel}
              onNoShow={handleNoShow}
            />
          ) : (
            <Empty description="No showings found" />
          )
        ) : (
          <div style={{ display: 'flex', gap: 24 }}>
            <div style={{ flex: 1 }}>
              <Calendar
                fullscreen={false}
                cellRender={(current) => dateCellRender(current)}
                onSelect={(date) => setSelectedDate(date)}
                value={selectedDate || dayjs()}
              />
            </div>
            <div style={{ width: 400 }}>
              <h4 style={{ marginBottom: 16 }}>
                {selectedDate ? selectedDate.format('MMMM D, YYYY') : 'Select a date'}
              </h4>
              {selectedDateShowings.length > 0 ? (
                selectedDateShowings.map((showing) => (
                  <ShowingCard
                    key={showing.id}
                    showing={showing}
                    onReschedule={() => handleReschedule(showing)}
                    onComplete={handleComplete}
                    onCancel={handleCancel}
                    onNoShow={handleNoShow}
                  />
                ))
              ) : (
                <Empty description="No showings on this date" />
              )}
            </div>
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
    </div>
  );
};

export default ShowingsPage;
