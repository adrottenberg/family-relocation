import { useState, useMemo } from 'react';
import {
  Card,
  Typography,
  Space,
  Segmented,
  Empty,
  Spin,
  Button,
  Collapse,
  Badge,
  Image,
} from 'antd';
import {
  CalendarOutlined,
  UserOutlined,
  HomeOutlined,
  ScheduleOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { propertyMatchesApi } from '../../api';
import type { PropertyMatchListDto } from '../../api/types';
import { ShowingSchedulerModal } from './scheduler';
import MatchScoreDisplay from '../propertyMatches/MatchScoreDisplay';

const { Title, Text } = Typography;

type GroupBy = 'applicant' | 'property';

interface SchedulerModalState {
  open: boolean;
  mode: 'applicant' | 'property';
  applicantId?: string;
  propertyId?: string;
  housingSearchId?: string;
}

const ScheduleShowingsPage = () => {
  const [groupBy, setGroupBy] = useState<GroupBy>('applicant');
  const [schedulerModal, setSchedulerModal] = useState<SchedulerModalState>({
    open: false,
    mode: 'applicant',
  });

  // Fetch pending property matches
  const { data: matches = [], isLoading, refetch } = useQuery({
    queryKey: ['propertyMatches', 'pending'],
    queryFn: () => propertyMatchesApi.getPending(),
    refetchInterval: 30000, // Refresh every 30 seconds
  });

  // Group matches by applicant or property
  const groupedMatches = useMemo(() => {
    if (groupBy === 'applicant') {
      const byApplicant = new Map<string, { name: string; housingSearchId: string; matches: PropertyMatchListDto[] }>();
      matches.forEach(match => {
        const key = match.applicantId;
        if (!byApplicant.has(key)) {
          byApplicant.set(key, {
            name: match.applicantName,
            housingSearchId: match.housingSearchId,
            matches: [],
          });
        }
        byApplicant.get(key)!.matches.push(match);
      });
      return Array.from(byApplicant.entries()).map(([id, data]) => ({
        id,
        label: data.name,
        housingSearchId: data.housingSearchId,
        matches: data.matches,
      }));
    } else {
      const byProperty = new Map<string, { street: string; city: string; price: number; photoUrl?: string; matches: PropertyMatchListDto[] }>();
      matches.forEach(match => {
        const key = match.propertyId;
        if (!byProperty.has(key)) {
          byProperty.set(key, {
            street: match.propertyStreet,
            city: match.propertyCity,
            price: match.propertyPrice,
            photoUrl: match.propertyPhotoUrl,
            matches: [],
          });
        }
        byProperty.get(key)!.matches.push(match);
      });
      return Array.from(byProperty.entries()).map(([id, data]) => ({
        id,
        label: `${data.street}, ${data.city}`,
        price: data.price,
        photoUrl: data.photoUrl,
        matches: data.matches,
      }));
    }
  }, [matches, groupBy]);

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 0,
    }).format(price);
  };

  const openScheduler = (match: PropertyMatchListDto) => {
    if (groupBy === 'applicant') {
      setSchedulerModal({
        open: true,
        mode: 'applicant',
        applicantId: match.applicantId,
        housingSearchId: match.housingSearchId,
      });
    } else {
      setSchedulerModal({
        open: true,
        mode: 'property',
        propertyId: match.propertyId,
      });
    }
  };

  const openSchedulerForGroup = (group: typeof groupedMatches[0]) => {
    if (groupBy === 'applicant' && 'housingSearchId' in group) {
      setSchedulerModal({
        open: true,
        mode: 'applicant',
        applicantId: group.id,
        housingSearchId: group.housingSearchId,
      });
    } else {
      setSchedulerModal({
        open: true,
        mode: 'property',
        propertyId: group.id,
      });
    }
  };

  const handleSchedulerClose = () => {
    setSchedulerModal({ open: false, mode: 'applicant' });
    refetch();
  };

  const renderMatchCard = (match: PropertyMatchListDto, showApplicant: boolean, showProperty: boolean) => (
    <Card
      key={match.id}
      size="small"
      styles={{ body: { padding: 12 } }}
      style={{ marginBottom: 8 }}
    >
      <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
        {/* Property Photo (when showing property) */}
        {showProperty && (
          <div style={{ flexShrink: 0 }}>
            {match.propertyPhotoUrl ? (
              <Image
                src={match.propertyPhotoUrl}
                alt="Property"
                width={80}
                height={60}
                style={{ objectFit: 'cover', borderRadius: 4 }}
                preview={false}
              />
            ) : (
              <div
                style={{
                  width: 80,
                  height: 60,
                  background: '#f0f0f0',
                  borderRadius: 4,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <Text type="secondary" style={{ fontSize: 10 }}>No photo</Text>
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
          {showProperty && (
            <div style={{ marginBottom: 4 }}>
              <Link
                to={`/listings/${match.propertyId}`}
                style={{ fontWeight: 600, color: '#1890ff' }}
              >
                {match.propertyStreet}
              </Link>
              <Text type="secondary" style={{ marginLeft: 8 }}>{match.propertyCity}</Text>
              <Text strong style={{ marginLeft: 12, color: '#52c41a' }}>
                {formatPrice(match.propertyPrice)}
              </Text>
            </div>
          )}

          {showApplicant && (
            <div style={{ marginBottom: 4 }}>
              <Link
                to={`/applicants/${match.applicantId}`}
                style={{ fontWeight: 600, color: '#1890ff' }}
              >
                {match.applicantName}
              </Link>
            </div>
          )}

          {showProperty && (
            <Text type="secondary" style={{ fontSize: 13 }}>
              {match.propertyBedrooms} bed / {match.propertyBathrooms} bath
            </Text>
          )}
        </div>

        {/* Schedule Button */}
        <Button
          type="primary"
          icon={<CalendarOutlined />}
          onClick={() => openScheduler(match)}
        >
          Schedule
        </Button>
      </div>
    </Card>
  );

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 100 }}>
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <div>
          <Title level={2} style={{ margin: 0 }}>
            <ScheduleOutlined style={{ marginRight: 12 }} />
            Schedule Showings
          </Title>
          <Text type="secondary" style={{ marginTop: 4, display: 'block' }}>
            {matches.length} {matches.length === 1 ? 'listing' : 'listings'} ready to be scheduled
          </Text>
        </div>

        <Space>
          <Text>Group by:</Text>
          <Segmented
            value={groupBy}
            onChange={(value) => setGroupBy(value as GroupBy)}
            options={[
              { label: <><UserOutlined /> Applicant</>, value: 'applicant' },
              { label: <><HomeOutlined /> Property</>, value: 'property' },
            ]}
          />
        </Space>
      </div>

      {matches.length === 0 ? (
        <Card>
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description="No showings pending. All caught up!"
          />
        </Card>
      ) : (
        <Collapse
          defaultActiveKey={groupedMatches.slice(0, 5).map(g => g.id)}
          items={groupedMatches.map(group => ({
            key: group.id,
            label: (
              <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                {groupBy === 'applicant' ? (
                  <>
                    <UserOutlined />
                    <Link
                      to={`/applicants/${group.id}`}
                      onClick={(e) => e.stopPropagation()}
                      style={{ fontWeight: 600 }}
                    >
                      {group.label}
                    </Link>
                  </>
                ) : (
                  <>
                    {'photoUrl' in group && group.photoUrl && (
                      <Image
                        src={group.photoUrl}
                        alt="Property"
                        width={40}
                        height={30}
                        style={{ objectFit: 'cover', borderRadius: 4 }}
                        preview={false}
                      />
                    )}
                    <Link
                      to={`/listings/${group.id}`}
                      onClick={(e) => e.stopPropagation()}
                      style={{ fontWeight: 600 }}
                    >
                      {group.label}
                    </Link>
                    {'price' in group && (
                      <Text strong style={{ color: '#52c41a' }}>
                        {formatPrice(group.price as number)}
                      </Text>
                    )}
                  </>
                )}
                <Badge count={group.matches.length} style={{ backgroundColor: '#1890ff' }} />
              </div>
            ),
            extra: (
              <Button
                size="small"
                icon={<CalendarOutlined />}
                onClick={(e) => {
                  e.stopPropagation();
                  openSchedulerForGroup(group);
                }}
              >
                Schedule All
              </Button>
            ),
            children: (
              <div>
                {group.matches.map(match =>
                  renderMatchCard(
                    match,
                    groupBy === 'property', // show applicant when grouped by property
                    groupBy === 'applicant'  // show property when grouped by applicant
                  )
                )}
              </div>
            ),
          }))}
        />
      )}

      {/* Scheduler Modal */}
      <ShowingSchedulerModal
        open={schedulerModal.open}
        onClose={handleSchedulerClose}
        mode={schedulerModal.mode}
        applicantId={schedulerModal.applicantId}
        propertyId={schedulerModal.propertyId}
        housingSearchId={schedulerModal.housingSearchId}
      />
    </div>
  );
};

export default ScheduleShowingsPage;
