import { Card, Tag, Button, Space, Typography, Checkbox, Image } from 'antd';
import { EyeOutlined, CalendarOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import type { PropertyMatchListDto } from '../../api/types';
import MatchScoreDisplay from './MatchScoreDisplay';

const { Text } = Typography;

interface PropertyMatchCardProps {
  match: PropertyMatchListDto;
  selectable?: boolean;
  selected?: boolean;
  onSelect?: (id: string, selected: boolean) => void;
  onRequestShowing?: (id: string) => void;
  onScheduleShowing?: (id: string) => void;
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
  showApplicant = true,
  showProperty = true,
}: PropertyMatchCardProps) => {
  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 0,
    }).format(price);
  };

  return (
    <Card
      size="small"
      style={{
        marginBottom: 12,
        border: selected ? '2px solid #1890ff' : undefined,
      }}
    >
      <div style={{ display: 'flex', gap: 12 }}>
        {/* Checkbox for selection */}
        {selectable && (
          <div style={{ display: 'flex', alignItems: 'flex-start', paddingTop: 4 }}>
            <Checkbox
              checked={selected}
              onChange={(e) => onSelect?.(match.id, e.target.checked)}
            />
          </div>
        )}

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
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 4 }}>
            <div>
              {showProperty && (
                <div>
                  <Link to={`/properties/${match.propertyId}`} style={{ fontWeight: 500 }}>
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
            <Space size="small">
              <Tag color={statusColors[match.status] || 'default'}>
                {statusLabels[match.status] || match.status}
              </Tag>
              {match.isAutoMatched ? (
                <Tag>Auto</Tag>
              ) : (
                <Tag color="cyan">Manual</Tag>
              )}
            </Space>
          </div>

          {showProperty && (
            <div style={{ marginBottom: 8 }}>
              <Text strong style={{ color: '#1890ff' }}>{formatPrice(match.propertyPrice)}</Text>
              <Text type="secondary" style={{ marginLeft: 12 }}>
                {match.propertyBedrooms} bed · {match.propertyBathrooms} bath
              </Text>
            </div>
          )}

          {/* Action Buttons */}
          <Space size="small">
            <Link to={`/properties/${match.propertyId}`}>
              <Button size="small" icon={<EyeOutlined />}>
                View Property
              </Button>
            </Link>
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
            {match.status === 'ShowingRequested' && onScheduleShowing && (
              <Button
                size="small"
                type="primary"
                icon={<CalendarOutlined />}
                onClick={() => onScheduleShowing(match.id)}
              >
                Schedule Showing
              </Button>
            )}
          </Space>
        </div>
      </div>
    </Card>
  );
};

export default PropertyMatchCard;
