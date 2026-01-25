import { useState, useMemo } from 'react';
import { Modal, message, Spin, Card, Typography } from 'antd';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import { DndContext, DragEndEvent, DragOverlay, DragStartEvent } from '@dnd-kit/core';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import dayjs, { Dayjs } from 'dayjs';
import { propertyMatchesApi } from '../../../api/endpoints/propertyMatches';
import { showingsApi, ScheduleShowingRequest } from '../../../api/endpoints/showings';
import type { PropertyMatchListDto, ShowingListDto } from '../../../api/types';
import PendingMatchesSidebar from './PendingMatchesSidebar';
import SchedulerCalendar from './SchedulerCalendar';
import DayScheduleView from './DayScheduleView';
import DraggableMatchCard from './DraggableMatchCard';
import { RescheduleShowingModal } from '../index';

const { Text } = Typography;

interface ShowingSchedulerModalProps {
  open: boolean;
  onClose: () => void;
  mode: 'applicant' | 'property';
  applicantId?: string;
  propertyId?: string;
  housingSearchId?: string;
}

// Types for drag data
type DragData =
  | { type: 'match'; match: PropertyMatchListDto }
  | { type: 'showing'; showing: ShowingListDto };

const ShowingSchedulerModal = ({
  open,
  onClose,
  mode,
  applicantId,
  propertyId,
  housingSearchId,
}: ShowingSchedulerModalProps) => {
  const [selectedDate, setSelectedDate] = useState<Dayjs>(dayjs().add(1, 'day'));
  const [activeDragData, setActiveDragData] = useState<DragData | null>(null);
  const [rescheduleShowingId, setRescheduleShowingId] = useState<string | null>(null);
  const queryClient = useQueryClient();

  // Query key for property matches - used for both fetching and optimistic updates
  const matchesQueryKey = mode === 'applicant'
    ? ['propertyMatches', 'housingSearch', housingSearchId]
    : ['propertyMatches', 'property', propertyId];

  // Fetch property matches based on mode
  const { data: matches = [], isLoading: matchesLoading } = useQuery({
    queryKey: matchesQueryKey,
    queryFn: async () => {
      if (mode === 'applicant' && housingSearchId) {
        return propertyMatchesApi.getForHousingSearch(housingSearchId);
      } else if (mode === 'property' && propertyId) {
        return propertyMatchesApi.getForProperty(propertyId);
      }
      return [];
    },
    enabled: open && ((mode === 'applicant' && !!housingSearchId) || (mode === 'property' && !!propertyId)),
  });

  // Calculate date range for fetching showings (current month view)
  const dateRange = useMemo(() => {
    const start = selectedDate.startOf('month').subtract(7, 'days');
    const end = selectedDate.endOf('month').add(7, 'days');
    return {
      fromDate: start.format('YYYY-MM-DD'),
      toDate: end.format('YYYY-MM-DD'),
    };
  }, [selectedDate]);

  // Fetch all showings for the visible date range
  const { data: showings = [], isLoading: showingsLoading } = useQuery({
    queryKey: ['showings', 'scheduler', dateRange.fromDate, dateRange.toDate],
    queryFn: () => showingsApi.getAll({
      fromDate: dateRange.fromDate,
      toDate: dateRange.toDate,
    }),
    enabled: open,
  });

  // Schedule showing mutation with optimistic update
  const scheduleMutation = useMutation({
    mutationFn: (request: ScheduleShowingRequest) => showingsApi.schedule(request),
    onMutate: async (request) => {
      // Cancel outgoing refetches to avoid overwriting optimistic update
      await queryClient.cancelQueries({ queryKey: matchesQueryKey });

      // Snapshot current state for rollback
      const previousMatches = queryClient.getQueryData<PropertyMatchListDto[]>(matchesQueryKey);

      // Optimistically update: add a temporary showing to the match so it leaves the pending sidebar
      queryClient.setQueryData<PropertyMatchListDto[]>(matchesQueryKey, (old) =>
        old?.map((match) =>
          match.id === request.propertyMatchId
            ? {
                ...match,
                showings: [
                  ...(match.showings || []),
                  {
                    id: 'temp-' + Date.now(),
                    status: 'Scheduled',
                    scheduledDate: request.scheduledDate,
                    scheduledTime: request.scheduledTime,
                  },
                ],
              }
            : match
        )
      );

      return { previousMatches };
    },
    onSuccess: () => {
      message.success('Showing scheduled successfully');
    },
    onError: (error: Error, _variables, context) => {
      // Roll back to previous state on error
      if (context?.previousMatches) {
        queryClient.setQueryData(matchesQueryKey, context.previousMatches);
      }
      message.error(`Failed to schedule showing: ${error.message}`);
    },
    onSettled: () => {
      // Always refetch to get actual server state
      queryClient.invalidateQueries({ queryKey: ['showings'] });
      queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
    },
  });

  // Reschedule showing mutation
  const rescheduleMutation = useMutation({
    mutationFn: ({ showingId, newDate, newTime }: { showingId: string; newDate: string; newTime: string }) =>
      showingsApi.reschedule(showingId, { newDate, newTime }),
    onSuccess: () => {
      message.success('Showing rescheduled successfully');
      queryClient.invalidateQueries({ queryKey: ['showings'] });
      queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
    },
    onError: (error: Error) => {
      message.error(`Failed to reschedule showing: ${error.message}`);
    },
  });

  // Cancel showing mutation
  const cancelMutation = useMutation({
    mutationFn: (showingId: string) => showingsApi.updateStatus(showingId, { status: 'Cancelled' }),
    onSuccess: () => {
      message.success('Showing cancelled');
      queryClient.invalidateQueries({ queryKey: ['showings'] });
      queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
    },
    onError: (error: Error) => {
      message.error(`Failed to cancel showing: ${error.message}`);
    },
  });

  // Calculate showing counts per day for calendar (only Scheduled showings)
  const showingCounts = useMemo(() => {
    const counts: Record<string, number> = {};
    showings
      .filter((showing) => showing.status === 'Scheduled')
      .forEach((showing) => {
        const dateStr = showing.scheduledDate;
        counts[dateStr] = (counts[dateStr] || 0) + 1;
      });
    return counts;
  }, [showings]);

  // Get showings for the selected date (only Scheduled showings)
  const selectedDateShowings = useMemo(() => {
    const dateStr = selectedDate.format('YYYY-MM-DD');
    return showings.filter((s) => s.scheduledDate === dateStr && s.status === 'Scheduled');
  }, [showings, selectedDate]);

  // Handle drag start
  const handleDragStart = (event: DragStartEvent) => {
    const data = event.active.data.current;
    if (data?.type === 'match' && data.match) {
      setActiveDragData({ type: 'match', match: data.match });
    } else if (data?.type === 'showing' && data.showing) {
      setActiveDragData({ type: 'showing', showing: data.showing });
    }
  };

  // Handle drag end
  const handleDragEnd = async (event: DragEndEvent) => {
    const dragData = activeDragData;
    setActiveDragData(null);

    const { over } = event;
    if (!over || !dragData) return;

    const dropId = over.id as string;

    // Determine action based on drag type and drop target
    if (dragData.type === 'match') {
      // Dragging a match card to schedule
      if (dropId === 'cancel-zone') return; // Can't cancel something not scheduled

      const matchId = dragData.match.id;
      const timeSlotId = dropId;

      // Check if slot is already occupied
      const existingShowing = showings.find((s) => {
        const showingSlot = `${s.scheduledDate}T${s.scheduledTime}`;
        return showingSlot === timeSlotId && s.status === 'Scheduled';
      });

      if (existingShowing) {
        Modal.confirm({
          title: 'Time Slot Conflict',
          content: `There's already a showing scheduled at this time for ${existingShowing.propertyStreet}. Do you want to schedule anyway?`,
          okText: 'Schedule Anyway',
          cancelText: 'Cancel',
          onOk: () => scheduleShowing(matchId, timeSlotId),
        });
      } else {
        await scheduleShowing(matchId, timeSlotId);
      }
    } else if (dragData.type === 'showing') {
      // Dragging a showing
      const showing = dragData.showing;

      if (dropId === 'cancel-zone') {
        // Drop on cancel zone - cancel the showing
        Modal.confirm({
          title: 'Cancel Showing',
          icon: <ExclamationCircleOutlined />,
          content: `Are you sure you want to cancel the showing for ${showing.propertyStreet}?`,
          okText: 'Cancel Showing',
          okButtonProps: { danger: true },
          cancelText: 'Keep',
          onOk: () => cancelMutation.mutate(showing.id),
        });
      } else {
        // Drop on time slot - reschedule
        const [newDate, newTime] = dropId.split('T');

        // Don't reschedule if dropped on same slot
        const currentSlot = `${showing.scheduledDate}T${showing.scheduledTime.substring(0, 8)}`;
        if (currentSlot === dropId) return;

        await rescheduleMutation.mutateAsync({
          showingId: showing.id,
          newDate,
          newTime,
        });
      }
    }
  };

  const scheduleShowing = async (matchId: string, timeSlotId: string) => {
    const [dateStr, timeStr] = timeSlotId.split('T');
    await scheduleMutation.mutateAsync({
      propertyMatchId: matchId,
      scheduledDate: dateStr,
      scheduledTime: timeStr,
    });
  };

  const handleDragCancel = () => {
    setActiveDragData(null);
  };

  // Handle reschedule via menu - open the reschedule modal
  const handleReschedule = (showingId: string) => {
    setRescheduleShowingId(showingId);
  };

  // Handle cancel via menu - show confirmation and cancel
  const handleCancelShowing = (showingId: string) => {
    const showing = showings.find((s) => s.id === showingId);
    Modal.confirm({
      title: 'Cancel Showing',
      icon: <ExclamationCircleOutlined />,
      content: `Are you sure you want to cancel the showing for ${showing?.propertyStreet || 'this property'}?`,
      okText: 'Cancel Showing',
      okButtonProps: { danger: true },
      cancelText: 'Keep',
      onOk: () => cancelMutation.mutate(showingId),
    });
  };

  // Handle reschedule modal close
  const handleRescheduleClose = () => {
    setRescheduleShowingId(null);
    queryClient.invalidateQueries({ queryKey: ['showings'] });
    queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
  };

  // Get showing details for reschedule modal
  const rescheduleShowing = rescheduleShowingId
    ? showings.find((s) => s.id === rescheduleShowingId)
    : null;

  const isLoading = matchesLoading || showingsLoading;

  // Render drag overlay based on what's being dragged
  const renderDragOverlay = () => {
    if (!activeDragData) return null;

    if (activeDragData.type === 'match') {
      return (
        <div style={{ opacity: 0.8, transform: 'rotate(3deg)' }}>
          <DraggableMatchCard match={activeDragData.match} />
        </div>
      );
    } else {
      const showing = activeDragData.showing;
      return (
        <Card
          size="small"
          style={{
            width: 200,
            opacity: 0.9,
            transform: 'rotate(3deg)',
            boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
          }}
        >
          <Text strong style={{ fontSize: 12 }}>{showing.propertyStreet}</Text>
          <br />
          <Text type="secondary" style={{ fontSize: 11 }}>{showing.applicantName}</Text>
        </Card>
      );
    }
  };

  return (
    <Modal
      open={open}
      onCancel={onClose}
      title="Schedule Showings"
      width={900}
      footer={null}
      styles={{
        body: { padding: 0, height: 600 },
      }}
    >
      <DndContext
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
        onDragCancel={handleDragCancel}
      >
        <div style={{ display: 'flex', height: '100%' }}>
          {/* Left sidebar with pending matches */}
          <PendingMatchesSidebar matches={matches} loading={matchesLoading} />

          {/* Right side - Calendar and Day view */}
          <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
            {isLoading ? (
              <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
                <Spin size="large" />
              </div>
            ) : (
              <>
                {/* Month calendar */}
                <div style={{ borderBottom: '1px solid #f0f0f0' }}>
                  <SchedulerCalendar
                    selectedDate={selectedDate}
                    onDateSelect={setSelectedDate}
                    showingCounts={showingCounts}
                  />
                </div>

                {/* Day schedule */}
                <div style={{ flex: 1, overflow: 'hidden' }}>
                  <DayScheduleView
                    date={selectedDate}
                    showings={selectedDateShowings}
                    contextApplicantId={applicantId}
                    contextPropertyId={propertyId}
                    onReschedule={handleReschedule}
                    onCancel={handleCancelShowing}
                  />
                </div>
              </>
            )}
          </div>
        </div>

        {/* Drag overlay */}
        <DragOverlay>
          {renderDragOverlay()}
        </DragOverlay>
      </DndContext>

      {/* Reschedule modal */}
      {rescheduleShowingId && rescheduleShowing && (
        <RescheduleShowingModal
          showingId={rescheduleShowingId}
          open={true}
          onClose={handleRescheduleClose}
          currentDate={rescheduleShowing.scheduledDate}
          currentTime={rescheduleShowing.scheduledTime}
          propertyInfo={{
            street: rescheduleShowing.propertyStreet,
            city: rescheduleShowing.propertyCity,
          }}
        />
      )}
    </Modal>
  );
};

export default ShowingSchedulerModal;
