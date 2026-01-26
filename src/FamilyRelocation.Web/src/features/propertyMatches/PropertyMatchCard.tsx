import { Card, Tag, Button, Space, Typography, Checkbox, Image, message, Modal, InputNumber } from 'antd';
import { EyeOutlined, CalendarOutlined, FileProtectOutlined, CheckCircleOutlined, CloseCircleOutlined, HeartOutlined, DollarOutlined } from '@ant-design/icons';
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
  selectable?: boolean;
  selected?: boolean;
  onSelect?: (id: string, selected: boolean) => void;
  onRequestShowing?: (id: string) => void;
  onScheduleShowing?: (id: string) => void;
  onEnterContract?: (propertyId: string, offerAmount?: number) => void;
  showApplicant?: boolean;
  showProperty?: boolean;
}

const statusColors: Record<string, string> = {
  MatchIdentified: 'blue',
  ShowingRequested: 'orange',
  ApplicantInterested: 'green',
  OfferMade: 'purple',
  ApplicantRejected: 'default',
};

const statusLabels: Record<string, string> = {
  MatchIdentified: 'Match Identified',
  ShowingRequested: 'Showing Requested',
  ApplicantInterested: 'Interested',
  OfferMade: 'Offer Made',
  ApplicantRejected: 'Rejected',
};

const PropertyMatchCard = ({
  match,
  selectable = false,
  selected = false,
  onSelect,
  onRequestShowing,
  onScheduleShowing,
  onEnterContract,
  showApplicant = true,
  showProperty = true,
}: PropertyMatchCardProps) => {
  const queryClient = useQueryClient();
  const [offerModalOpen, setOfferModalOpen] = useState(false);
  const [offerAmount, setOfferAmount] = useState<number | null>(null);

  const updateStatusMutation = useMutation({
    mutationFn: ({ status, offerAmount }: { status: PropertyMatchStatus; offerAmount?: number }) =>
      propertyMatchesApi.updateStatus(match.id, { status, offerAmount }),
    onSuccess: () => {
      message.success('Status updated');
      queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
      setOfferModalOpen(false);
      setOfferAmount(null);
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

  const handleOfferSubmit = () => {
    if (!offerAmount || offerAmount <= 0) {
      message.error('Please enter a valid offer amount');
      return;
    }
    updateStatusMutation.mutate({ status: 'OfferMade', offerAmount });
  };

  const openOfferModal = () => {
    setOfferAmount(match.propertyPrice); // Default to property price
    setOfferModalOpen(true);
  };

  return (
    <>
      <Card
        size="small"
        style={{
          marginBottom: 12,
          border: selected ? '2px solid #1890ff' : undefined,
        }}
      >
      <div style={{ display: 'flex', gap: 12, alignItems: 'flex-start', paddingTop: 4 }}>
        {/* Checkbox area - always reserve space for alignment */}
        <div style={{ width: 24, flexShrink: 0, paddingTop: 4 }}>
          {selectable && (
            <Checkbox
              checked={selected}
              onChange={(e) => onSelect?.(match.id, e.target.checked)}
            />
          )}
        </div>

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
          {/* Header row with name and status tags */}
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 8 }}>
            <div>
              {showProperty && (
                <div>
                  <Link to={`/listings/${match.propertyId}`} style={{ fontWeight: 500 }}>
                    {match.propertyStreet}
                  </Link>
                  <Text type="secondary" style={{ marginLeft: 8 }}>{match.propertyCity}</Text>
                </div>
              )}
              {showApplicant && (
                <div style={{ marginTop: showProperty ? 2 : 0 }}>
                  <Link to={`/applicants/${match.applicantId}`}>
                    {match.applicantName}
                  </Link>
                </div>
              )}
            </div>
            <Space size="small" style={{ flexShrink: 0 }}>
              {/* Show match status - offer amount takes priority */}
              {match.status === 'OfferMade' && match.offerAmount ? (
                <Tag color={statusColors[match.status]}>
                  Offer: {formatPrice(match.offerAmount)}
                </Tag>
              ) : hasScheduledShowing ? (
                <Tag color="green">Scheduled</Tag>
              ) : (
                <Tag color={statusColors[match.status] || 'default'}>
                  {statusLabels[match.status] || match.status}
                </Tag>
              )}
              {match.isAutoMatched ? (
                <Tag>Auto</Tag>
              ) : (
                <Tag color="cyan">Manual</Tag>
              )}
            </Space>
          </div>

          {/* Price and details */}
          {showProperty && (
            <div style={{ marginBottom: 8 }}>
              <Text strong style={{ color: '#1890ff' }}>{formatPrice(match.propertyPrice)}</Text>
              <Text type="secondary" style={{ marginLeft: 12 }}>
                {match.propertyBedrooms} bed · {match.propertyBathrooms} bath
              </Text>
            </div>
          )}

          {/* Showing info tags */}
          {match.showings && match.showings.length > 0 && (
            <div style={{ marginBottom: 8 }}>
              <Space size="small" wrap>
                {match.showings.filter(s => s.status === 'Scheduled').map((showing) => (
                  <Tag key={showing.id} icon={<CalendarOutlined />} color="processing">
                    Showing: {formatDateTime(showing.scheduledDateTime, 'MMM D [at] h:mm A')}
                  </Tag>
                ))}
                {match.showings.filter(s => s.status === 'Completed').length > 0 && (
                  <Tag color="success" icon={<CheckCircleOutlined />}>
                    {match.showings.filter(s => s.status === 'Completed').length} completed
                  </Tag>
                )}
                {match.showings.filter(s => s.status === 'Cancelled').length > 0 && (
                  <Tag color="default">
                    {match.showings.filter(s => s.status === 'Cancelled').length} cancelled
                  </Tag>
                )}
              </Space>
            </div>
          )}

          {/* Action Buttons */}
          <Space size="small" wrap>
            <Link to={`/listings/${match.propertyId}`}>
              <Button size="small" icon={<EyeOutlined />}>
                View Listing
              </Button>
            </Link>

            {/* Request Showing - only for MatchIdentified */}
            {match.status === 'MatchIdentified' && onRequestShowing && (
              <Button
                size="small"
                type="primary"
                ghost
                onClick={() => onRequestShowing(match.id)}
              >
                Request Showing
              </Button>
            )}

            {/* Schedule Showing - for ShowingRequested without scheduled showing */}
            {match.status === 'ShowingRequested' && onScheduleShowing && !hasScheduledShowing && (
              <Button
                size="small"
                type="primary"
                icon={<CalendarOutlined />}
                onClick={() => onScheduleShowing(match.id)}
              >
                Schedule Showing
              </Button>
            )}

            {/* Status action buttons - show for non-rejected statuses */}
            {match.status !== 'ApplicantRejected' && match.status !== 'ApplicantInterested' && (
              <Button
                size="small"
                icon={<HeartOutlined />}
                onClick={() => updateStatusMutation.mutate({ status: 'ApplicantInterested' })}
                loading={updateStatusMutation.isPending}
              >
                Interested
              </Button>
            )}

            {match.status !== 'ApplicantRejected' && match.status !== 'OfferMade' && (
              <Button
                size="small"
                type="primary"
                ghost
                icon={<DollarOutlined />}
                onClick={openOfferModal}
                loading={updateStatusMutation.isPending}
              >
                Make Offer
              </Button>
            )}

            {match.status !== 'ApplicantRejected' && (
              <Button
                size="small"
                danger
                icon={<CloseCircleOutlined />}
                onClick={() => updateStatusMutation.mutate({ status: 'ApplicantRejected' })}
                loading={updateStatusMutation.isPending}
              >
                Not Interested
              </Button>
            )}

            {/* Enter Contract button - only show after offer is made */}
            {match.status === 'OfferMade' && onEnterContract && (
              <Button
                size="small"
                type="primary"
                icon={<FileProtectOutlined />}
                onClick={() => onEnterContract(match.propertyId, match.offerAmount)}
              >
                Enter Contract
              </Button>
            )}
          </Space>
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
