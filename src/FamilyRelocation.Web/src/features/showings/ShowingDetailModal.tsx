import { Modal, Descriptions, Tag, Button, Space, Divider, message } from 'antd';
import {
  CalendarOutlined,
  ClockCircleOutlined,
  HomeOutlined,
  UserOutlined,
  CheckOutlined,
  CloseOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { showingsApi } from '../../api';
import type { ShowingStatus } from '../../api/types';
import { useState } from 'react';
import RescheduleShowingModal from './RescheduleShowingModal';

interface ShowingDetailModalProps {
  open: boolean;
  onClose: () => void;
  showingId: string;
}

const statusColors: Record<ShowingStatus, string> = {
  Scheduled: 'blue',
  Completed: 'green',
  Cancelled: 'default',
  NoShow: 'orange',
};

const ShowingDetailModal = ({ open, onClose, showingId }: ShowingDetailModalProps) => {
  const queryClient = useQueryClient();
  const [rescheduleModalOpen, setRescheduleModalOpen] = useState(false);

  const { data: showing, isLoading } = useQuery({
    queryKey: ['showings', showingId],
    queryFn: () => showingsApi.getById(showingId),
    enabled: open && !!showingId,
  });

  const updateStatusMutation = useMutation({
    mutationFn: (status: ShowingStatus) => showingsApi.updateStatus(showingId, { status }),
    onSuccess: () => {
      message.success('Showing status updated');
      queryClient.invalidateQueries({ queryKey: ['showings'] });
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to update status');
    },
  });

  const handleComplete = () => {
    updateStatusMutation.mutate('Completed');
  };

  const handleCancel = () => {
    updateStatusMutation.mutate('Cancelled');
  };

  const handleNoShow = () => {
    updateStatusMutation.mutate('NoShow');
  };

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const formatTime = (timeStr: string) => {
    const [hours, minutes] = timeStr.split(':');
    const date = new Date();
    date.setHours(parseInt(hours), parseInt(minutes));
    return date.toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true,
    });
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 0,
    }).format(price);
  };

  return (
    <>
      <Modal
        title="Showing Details"
        open={open}
        onCancel={onClose}
        width={600}
        footer={
          showing?.status === 'Scheduled' ? (
            <Space>
              <Button onClick={() => setRescheduleModalOpen(true)}>Reschedule</Button>
              <Button danger onClick={handleCancel} loading={updateStatusMutation.isPending}>
                <CloseOutlined /> Cancel
              </Button>
              <Button onClick={handleNoShow} loading={updateStatusMutation.isPending}>
                No Show
              </Button>
              <Button type="primary" onClick={handleComplete} loading={updateStatusMutation.isPending}>
                <CheckOutlined /> Mark Completed
              </Button>
            </Space>
          ) : (
            <Button onClick={onClose}>Close</Button>
          )
        }
      >
        {isLoading ? (
          <div>Loading...</div>
        ) : showing ? (
          <>
            <div style={{ marginBottom: 16, textAlign: 'center' }}>
              <Tag color={statusColors[showing.status]} style={{ fontSize: 14, padding: '4px 12px' }}>
                {showing.status}
              </Tag>
            </div>

            <Descriptions column={1} bordered size="small">
              <Descriptions.Item
                label={
                  <>
                    <CalendarOutlined /> Date
                  </>
                }
              >
                {formatDate(showing.scheduledDate)}
              </Descriptions.Item>
              <Descriptions.Item
                label={
                  <>
                    <ClockCircleOutlined /> Time
                  </>
                }
              >
                {formatTime(showing.scheduledTime)}
              </Descriptions.Item>
            </Descriptions>

            <Divider orientation="left">
              <HomeOutlined /> Property
            </Divider>

            <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
              {showing.propertyPhotoUrl && (
                <img
                  src={showing.propertyPhotoUrl}
                  alt={showing.propertyStreet}
                  style={{ width: 120, height: 90, objectFit: 'cover', borderRadius: 4 }}
                />
              )}
              <div>
                <div style={{ fontWeight: 600, fontSize: 16 }}>{showing.propertyStreet}</div>
                <div style={{ color: '#666' }}>{showing.propertyCity}</div>
                <div style={{ fontSize: 18, fontWeight: 600, color: '#1890ff', marginTop: 8 }}>
                  {formatPrice(showing.propertyPrice)}
                </div>
              </div>
            </div>

            <Divider orientation="left">
              <UserOutlined /> Family
            </Divider>

            <div style={{ marginBottom: 16 }}>
              <div style={{ fontWeight: 600, fontSize: 16 }}>{showing.applicantName} Family</div>
            </div>

            {showing.brokerUserName && (
              <>
                <Divider orientation="left">Broker</Divider>
                <div style={{ marginBottom: 16 }}>{showing.brokerUserName}</div>
              </>
            )}

            {showing.notes && (
              <>
                <Divider orientation="left">Notes</Divider>
                <div style={{ marginBottom: 16, whiteSpace: 'pre-wrap' }}>{showing.notes}</div>
              </>
            )}

            {showing.completedAt && (
              <div style={{ marginTop: 16, color: '#666', fontSize: 12 }}>
                Completed on: {new Date(showing.completedAt).toLocaleString()}
              </div>
            )}
          </>
        ) : (
          <div>Showing not found</div>
        )}
      </Modal>

      <RescheduleShowingModal
        open={rescheduleModalOpen}
        onClose={() => setRescheduleModalOpen(false)}
        showingId={showingId}
        currentDate={showing?.scheduledDate || ''}
        currentTime={showing?.scheduledTime || ''}
      />
    </>
  );
};

export default ShowingDetailModal;
