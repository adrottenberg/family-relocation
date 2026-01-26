import { Modal, Descriptions, Tag, Button, Space, Divider, message, InputNumber, Typography } from 'antd';
import {
  CalendarOutlined,
  ClockCircleOutlined,
  HomeOutlined,
  UserOutlined,
  CheckOutlined,
  CloseOutlined,
  HeartOutlined,
  DollarOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { showingsApi, propertyMatchesApi } from '../../api';
import type { ShowingStatus, PropertyMatchStatus } from '../../api/types';
import { useState } from 'react';
import RescheduleShowingModal from './RescheduleShowingModal';
import { formatDate, formatTime } from '../../utils/datetime';

const { Text } = Typography;

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

const matchStatusColors: Record<string, string> = {
  MatchIdentified: 'blue',
  ShowingRequested: 'orange',
  ApplicantInterested: 'green',
  OfferMade: 'purple',
  ApplicantRejected: 'default',
};

const matchStatusLabels: Record<string, string> = {
  MatchIdentified: 'Match Identified',
  ShowingRequested: 'Showing Requested',
  ApplicantInterested: 'Interested',
  OfferMade: 'Offer Made',
  ApplicantRejected: 'Not Interested',
};

const ShowingDetailModal = ({ open, onClose, showingId }: ShowingDetailModalProps) => {
  const queryClient = useQueryClient();
  const [rescheduleModalOpen, setRescheduleModalOpen] = useState(false);
  const [offerModalOpen, setOfferModalOpen] = useState(false);
  const [offerAmount, setOfferAmount] = useState<number | null>(null);

  const { data: showing, isLoading } = useQuery({
    queryKey: ['showings', showingId],
    queryFn: () => showingsApi.getById(showingId),
    enabled: open && !!showingId,
  });

  // Fetch property match to check its current status
  const { data: propertyMatch } = useQuery({
    queryKey: ['propertyMatches', showing?.propertyMatchId],
    queryFn: () => propertyMatchesApi.getById(showing!.propertyMatchId),
    enabled: open && !!showing?.propertyMatchId,
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

  const updateMatchStatusMutation = useMutation({
    mutationFn: ({ status, offerAmount }: { status: PropertyMatchStatus; offerAmount?: number }) =>
      propertyMatchesApi.updateStatus(showing!.propertyMatchId, { status, offerAmount }),
    onSuccess: () => {
      message.success('Match status updated');
      queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
      queryClient.invalidateQueries({ queryKey: ['showings'] });
      setOfferModalOpen(false);
      setOfferAmount(null);
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to update match status');
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

  const handleMarkInterested = () => {
    updateMatchStatusMutation.mutate({ status: 'ApplicantInterested' });
  };

  const handleOpenOfferModal = () => {
    setOfferAmount(showing?.propertyPrice || null);
    setOfferModalOpen(true);
  };

  const handleMakeOffer = () => {
    if (!offerAmount || offerAmount <= 0) {
      message.error('Please enter a valid offer amount');
      return;
    }
    updateMatchStatusMutation.mutate({ status: 'OfferMade', offerAmount });
  };

  const handleNotInterested = () => {
    updateMatchStatusMutation.mutate({ status: 'ApplicantRejected' });
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
          ) : showing?.status === 'Completed' && propertyMatch?.status !== 'ApplicantRejected' ? (
            <Space>
              {propertyMatch?.status !== 'ApplicantInterested' && propertyMatch?.status !== 'OfferMade' && (
                <Button
                  icon={<HeartOutlined />}
                  onClick={handleMarkInterested}
                  loading={updateMatchStatusMutation.isPending}
                >
                  Interested
                </Button>
              )}
              {propertyMatch?.status !== 'OfferMade' && (
                <Button
                  type="primary"
                  ghost
                  icon={<DollarOutlined />}
                  onClick={handleOpenOfferModal}
                  loading={updateMatchStatusMutation.isPending}
                >
                  Make Offer
                </Button>
              )}
              <Button
                danger
                icon={<CloseOutlined />}
                onClick={handleNotInterested}
                loading={updateMatchStatusMutation.isPending}
              >
                Not Interested
              </Button>
              <Button onClick={onClose}>Close</Button>
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
              <Space>
                <Tag color={statusColors[showing.status]} style={{ fontSize: 14, padding: '4px 12px' }}>
                  Showing: {showing.status}
                </Tag>
                {propertyMatch && (
                  <Tag
                    color={matchStatusColors[propertyMatch.status] || 'default'}
                    style={{ fontSize: 14, padding: '4px 12px' }}
                  >
                    {propertyMatch.status === 'OfferMade' && propertyMatch.offerAmount
                      ? `Offer: ${formatPrice(propertyMatch.offerAmount)}`
                      : matchStatusLabels[propertyMatch.status] || propertyMatch.status}
                  </Tag>
                )}
              </Space>
            </div>

            <Descriptions column={1} bordered size="small">
              <Descriptions.Item
                label={
                  <>
                    <CalendarOutlined /> Date
                  </>
                }
              >
                {formatDate(showing.scheduledDateTime, 'dddd, MMMM D, YYYY')}
              </Descriptions.Item>
              <Descriptions.Item
                label={
                  <>
                    <ClockCircleOutlined /> Time
                  </>
                }
              >
                {formatTime(showing.scheduledDateTime)}
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
              <UserOutlined /> Applicant
            </Divider>

            <div style={{ marginBottom: 16 }}>
              <div style={{ fontWeight: 600, fontSize: 16 }}>{showing.applicantName}</div>
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
        currentDateTime={showing?.scheduledDateTime}
      />

      {/* Offer Amount Modal */}
      <Modal
        title="Enter Offer Amount"
        open={offerModalOpen}
        onOk={handleMakeOffer}
        onCancel={() => {
          setOfferModalOpen(false);
          setOfferAmount(null);
        }}
        okText="Submit Offer"
        confirmLoading={updateMatchStatusMutation.isPending}
      >
        <div style={{ marginBottom: 16 }}>
          <Text>Enter the offer amount for {showing?.propertyStreet}:</Text>
          <InputNumber
            style={{ width: '100%', marginTop: 8 }}
            size="large"
            value={offerAmount}
            onChange={(value) => setOfferAmount(value)}
            formatter={(value) => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
            parser={(value) => Number(value?.replace(/\$\s?|(,*)/g, '') || 0)}
            min={0}
            placeholder="Enter offer amount"
          />
        </div>
      </Modal>
    </>
  );
};

export default ShowingDetailModal;
