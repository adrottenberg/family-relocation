import { Card, Tag, Button, Space, Typography, Image, message, Modal, InputNumber } from 'antd';
import { CalendarOutlined, CheckCircleOutlined, HeartOutlined, DollarOutlined, CloseCircleOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import type { PropertyMatchListDto, PropertyMatchStatus } from '../../api/types';
import { propertyMatchesApi } from '../../api';
import MatchScoreDisplay from './MatchScoreDisplay';
import { formatDateTime } from '../../utils/datetime';

const { Text } = Typography;

interface PropertyMatchCardProps {
  match: PropertyMatchListDto;
  onScheduleShowing?: (id: string) => void;
  onEnterContract?: (propertyId: string, offerAmount?: number) => void;
  onRemove?: (id: string) => void; // Callback when match is removed (rejected/no longer interested)
  showApplicant?: boolean;
  showProperty?: boolean;
}

const statusColors: Record<string, string> = {
  MatchIdentified: 'blue',
  ShowingRequested: 'processing',
  ApplicantInterested: 'green',
  OfferMade: 'purple',
  ApplicantRejected: 'default',
};

const PropertyMatchCard = ({
  match,
  onScheduleShowing,
  onEnterContract,
  onRemove,
  showApplicant = true,
  showProperty = true,
}: PropertyMatchCardProps) => {
  const queryClient = useQueryClient();
  const [offerModalOpen, setOfferModalOpen] = useState(false);
  const [offerAmount, setOfferAmount] = useState<number | null>(null);

  const updateStatusMutation = useMutation({
    mutationFn: ({ status, offerAmount }: { status: PropertyMatchStatus; offerAmount?: number }) =>
      propertyMatchesApi.updateStatus(match.id, { status, offerAmount }),
    onSuccess: (_, variables) => {
      message.success('Status updated');
      queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
      setOfferModalOpen(false);
      setOfferAmount(null);
      // Notify parent if match was removed
      if (variables.status === 'ApplicantRejected' && onRemove) {
        onRemove(match.id);
      }
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to update status');
    },
  });

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 0,
    }).format(price);
  };

  const hasScheduledShowing = match.showings?.some(s => s.status === 'Scheduled');
  const hasCompletedShowing = match.showings?.some(s => s.status === 'Completed');
  const isNewMatch = match.status === 'MatchIdentified';
  const isInterested = match.status === 'ApplicantInterested' || match.status === 'ShowingRequested';
  const hasOffer = match.status === 'OfferMade';

  const handleOfferSubmit = () => {
    if (!offerAmount || offerAmount <= 0) {
      message.error('Please enter a valid offer amount');
      return;
    }
    updateStatusMutation.mutate({ status: 'OfferMade', offerAmount });
  };

  const openOfferModal = () => {
    setOfferAmount(match.propertyPrice);
    setOfferModalOpen(true);
  };

  const handleInterested = () => {
    updateStatusMutation.mutate({ status: 'ApplicantInterested' });
  };

  const handleNotInterested = () => {
    Modal.confirm({
      title: 'Remove this listing?',
      icon: <ExclamationCircleOutlined />,
      content: 'This listing will be removed from the suggestions. This action cannot be undone.',
      okText: 'Yes, Remove',
      okButtonProps: { danger: true },
      cancelText: 'Cancel',
      onOk: () => {
        updateStatusMutation.mutate({ status: 'ApplicantRejected' });
      },
    });
  };

  const handleNoLongerInterested = () => {
    const hasUpcomingShowings = match.showings?.some(s => s.status === 'Scheduled');

    Modal.confirm({
      title: 'No longer interested?',
      icon: <ExclamationCircleOutlined />,
      content: hasUpcomingShowings
        ? 'This will remove the listing from suggestions and cancel any scheduled showings for this applicant on this property.'
        : 'This listing will be removed from the suggestions. This action cannot be undone.',
      okText: 'Yes, Remove',
      okButtonProps: { danger: true },
      cancelText: 'Cancel',
      onOk: () => {
        updateStatusMutation.mutate({ status: 'ApplicantRejected' });
      },
    });
  };

  // Render status tag
  const renderStatusTag = () => {
    if (hasOffer && match.offerAmount) {
      return (
        <Tag color={statusColors.OfferMade}>
          Offer: {formatPrice(match.offerAmount)}
        </Tag>
      );
    }
    if (hasScheduledShowing) {
      return <Tag color="green">Showing Scheduled</Tag>;
    }
    if (isInterested) {
      return <Tag color={statusColors.ApplicantInterested}>Interested</Tag>;
    }
    return null;
  };

  // Render action buttons based on state
  const renderActionButtons = () => {
    const buttons: React.ReactNode[] = [];

    // New match: Interested / Not Interested
    if (isNewMatch) {
      buttons.push(
        <Button
          key="interested"
          type="primary"
          icon={<HeartOutlined />}
          onClick={handleInterested}
          loading={updateStatusMutation.isPending}
          block
        >
          Interested
        </Button>,
        <Button
          key="not-interested"
          danger
          icon={<CloseCircleOutlined />}
          onClick={handleNotInterested}
          loading={updateStatusMutation.isPending}
          block
        >
          Not Interested
        </Button>
      );
    }
    // Interested but no offer yet: Make Offer / No Longer Interested
    // (Schedule Showing is handled by the top-level calendar button)
    else if (isInterested) {
      buttons.push(
        <Button
          key="offer"
          type="primary"
          ghost
          icon={<DollarOutlined />}
          onClick={openOfferModal}
          loading={updateStatusMutation.isPending}
          block
        >
          Make Offer
        </Button>,
        <Button
          key="no-longer"
          danger
          icon={<CloseCircleOutlined />}
          onClick={handleNoLongerInterested}
          loading={updateStatusMutation.isPending}
          block
        >
          No Longer Interested
        </Button>
      );
    }
    // Has offer: Enter Contract / No Longer Interested
    else if (hasOffer) {
      if (onEnterContract) {
        buttons.push(
          <Button
            key="contract"
            type="primary"
            onClick={() => onEnterContract(match.propertyId, match.offerAmount)}
            block
          >
            Enter Contract
          </Button>
        );
      }
      buttons.push(
        <Button
          key="no-longer"
          danger
          icon={<CloseCircleOutlined />}
          onClick={handleNoLongerInterested}
          loading={updateStatusMutation.isPending}
          block
        >
          No Longer Interested
        </Button>
      );
    }

    return buttons;
  };

  return (
    <>
      <Card size="small" styles={{ body: { padding: 12 } }} style={{ marginBottom: 12 }}>
        <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
          {/* Property Photo */}
          {showProperty && (
            <div style={{ flexShrink: 0 }}>
              {match.propertyPhotoUrl ? (
                <Image
                  src={match.propertyPhotoUrl}
                  alt="Property"
                  width={100}
                  height={75}
                  style={{ objectFit: 'cover', borderRadius: 4 }}
                  preview={false}
                />
              ) : (
                <div
                  style={{
                    width: 100,
                    height: 75,
                    background: '#f0f0f0',
                    borderRadius: 4,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  <Text type="secondary" style={{ fontSize: 11 }}>No photo</Text>
                </div>
              )}
            </div>
          )}

          {/* Match Score */}
          <div style={{ flexShrink: 0 }}>
            <MatchScoreDisplay score={match.matchScore} size="small" />
          </div>

          {/* Main Content */}
          <div style={{ flex: 1, minWidth: 0 }}>
            {/* Property Info */}
            {showProperty && (
              <div style={{ marginBottom: 4 }}>
                <Link
                  to={`/listings/${match.propertyId}`}
                  style={{ fontWeight: 600, fontSize: 16, color: '#1890ff' }}
                >
                  {match.propertyStreet}
                </Link>
                <Text type="secondary" style={{ marginLeft: 8 }}>{match.propertyCity}</Text>
              </div>
            )}

            {/* Applicant Info */}
            {showApplicant && (
              <div style={{ marginBottom: 4 }}>
                <Link
                  to={`/applicants/${match.applicantId}`}
                  style={{ fontWeight: 600, fontSize: 16, color: '#1890ff' }}
                >
                  {match.applicantName}
                </Link>
              </div>
            )}

            {/* Price and details */}
            {showProperty && (
              <div style={{ marginBottom: 4 }}>
                <Text strong style={{ color: '#52c41a', fontSize: 15 }}>{formatPrice(match.propertyPrice)}</Text>
                <Text type="secondary" style={{ marginLeft: 12 }}>
                  {match.propertyBedrooms} bed · {match.propertyBathrooms} bath
                </Text>
              </div>
            )}

            {/* Status and showing info */}
            <Space size="small" wrap>
              {renderStatusTag()}
              {match.isAutoMatched ? (
                <Tag>Auto</Tag>
              ) : (
                <Tag color="cyan">Manual</Tag>
              )}
              {/* Scheduled showings */}
              {match.showings?.filter(s => s.status === 'Scheduled').map((showing) => (
                <Tag key={showing.id} icon={<CalendarOutlined />} color="processing">
                  {formatDateTime(showing.scheduledDateTime, 'MMM D [at] h:mm A')}
                </Tag>
              ))}
              {/* Completed showings count */}
              {hasCompletedShowing && (
                <Tag color="success" icon={<CheckCircleOutlined />}>
                  {match.showings?.filter(s => s.status === 'Completed').length} shown
                </Tag>
              )}
            </Space>
          </div>

          {/* Action Buttons - Vertical stack on right */}
          <div style={{ flexShrink: 0, width: 160, display: 'flex', flexDirection: 'column', gap: 8 }}>
            {renderActionButtons()}
          </div>
        </div>
      </Card>

      {/* Offer Amount Modal */}
      <Modal
        title="Enter Offer Amount"
        open={offerModalOpen}
        onOk={handleOfferSubmit}
        onCancel={() => {
          setOfferModalOpen(false);
          setOfferAmount(null);
        }}
        okText="Submit Offer"
        confirmLoading={updateStatusMutation.isPending}
      >
        <div style={{ marginBottom: 16 }}>
          <p>Enter the offer amount for {match.propertyStreet}:</p>
          <InputNumber
            style={{ width: '100%' }}
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

export default PropertyMatchCard;
