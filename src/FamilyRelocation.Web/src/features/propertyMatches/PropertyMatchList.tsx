import { Card, Button, Space, Empty, Spin, Collapse, Badge, Typography } from 'antd';
import { PlusOutlined, CalendarOutlined, HeartOutlined, QuestionCircleOutlined, DollarOutlined } from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { propertyMatchesApi } from '../../api';
import type { PropertyMatchListDto } from '../../api/types';
import PropertyMatchCard from './PropertyMatchCard';

const { Text } = Typography;

export interface MatchScheduleData {
  id: string;
  propertyStreet: string;
  propertyCity: string;
  applicantName: string;
}

interface PropertyMatchListProps {
  housingSearchId?: string;
  propertyId?: string;
  onCreateMatch?: () => void;
  onScheduleShowings?: (matches: MatchScheduleData[]) => void;
  onOpenScheduler?: () => void;
  onEnterContract?: (propertyId: string, offerAmount?: number) => void;
  showApplicant?: boolean;
  showProperty?: boolean;
}

const PropertyMatchList = ({
  housingSearchId,
  propertyId,
  onCreateMatch,
  onScheduleShowings,
  onOpenScheduler,
  onEnterContract,
  showApplicant = true,
  showProperty = true,
}: PropertyMatchListProps) => {
  // Context-aware title: from applicant view = "Suggested Listings", from property view = "Suggested Applicants"
  const title = propertyId ? 'Suggested Applicants' : 'Suggested Listings';
  const itemLabel = propertyId ? 'applicant' : 'listing';

  const queryKey = housingSearchId
    ? ['propertyMatches', 'housingSearch', housingSearchId]
    : ['propertyMatches', 'property', propertyId];

  const { data: allMatches, isLoading } = useQuery({
    queryKey,
    queryFn: () => {
      if (housingSearchId) {
        return propertyMatchesApi.getForHousingSearch(housingSearchId);
      }
      return propertyMatchesApi.getForProperty(propertyId!);
    },
    enabled: !!(housingSearchId || propertyId),
  });

  // Categorize matches into 3 groups: Offer Made, Interested, Pending Review
  const offerMadeMatches = (allMatches || []).filter(
    m => m.status === 'OfferMade'
  );
  const interestedMatches = (allMatches || []).filter(
    m => m.status === 'ApplicantInterested' || m.status === 'ShowingRequested'
  );
  const pendingMatches = (allMatches || []).filter(
    m => m.status === 'MatchIdentified'
  );
  // Note: ApplicantRejected matches are not shown at all per requirements

  // Count matches needing scheduling (interested but no scheduled showing)
  const needsScheduling = interestedMatches.filter(
    m => !m.showings?.some(s => s.status === 'Scheduled')
  );

  const renderMatchList = (matches: PropertyMatchListDto[]) => {
    if (matches.length === 0) {
      return <Empty description={`No ${itemLabel}s`} image={Empty.PRESENTED_IMAGE_SIMPLE} />;
    }

    return matches.map((match) => (
      <PropertyMatchCard
        key={match.id}
        match={match}
        onScheduleShowing={onScheduleShowings ? (id) => {
          const m = allMatches?.find(item => item.id === id);
          if (m) {
            onScheduleShowings([{
              id: m.id,
              propertyStreet: m.propertyStreet,
              propertyCity: m.propertyCity,
              applicantName: m.applicantName,
            }]);
          }
        } : undefined}
        onEnterContract={onEnterContract}
        showApplicant={showApplicant}
        showProperty={showProperty}
      />
    ));
  };

  // Accordion items in priority order: Offer Made → Interested → Pending Review
  const accordionItems = [
    {
      key: 'offerMade',
      label: (
        <Space>
          <DollarOutlined />
          <span>Offer Made</span>
          <Badge count={offerMadeMatches.length} style={{ backgroundColor: '#722ed1' }} />
        </Space>
      ),
      children: renderMatchList(offerMadeMatches),
    },
    {
      key: 'interested',
      label: (
        <Space>
          <HeartOutlined />
          <span>Interested</span>
          <Badge count={interestedMatches.length} style={{ backgroundColor: '#52c41a' }} />
          {needsScheduling.length > 0 && (
            <Text type="secondary" style={{ fontSize: 12 }}>
              ({needsScheduling.length} need scheduling)
            </Text>
          )}
        </Space>
      ),
      children: renderMatchList(interestedMatches),
    },
    {
      key: 'pending',
      label: (
        <Space>
          <QuestionCircleOutlined />
          <span>Pending Review</span>
          <Badge count={pendingMatches.length} style={{ backgroundColor: '#1890ff' }} />
        </Space>
      ),
      children: renderMatchList(pendingMatches),
    },
  ];

  // Default open panels based on content - open sections that have items
  const defaultActiveKeys: string[] = [];
  if (offerMadeMatches.length > 0) defaultActiveKeys.push('offerMade');
  if (interestedMatches.length > 0) defaultActiveKeys.push('interested');
  if (pendingMatches.length > 0) defaultActiveKeys.push('pending');
  // If nothing has content, open pending
  if (defaultActiveKeys.length === 0) defaultActiveKeys.push('pending');

  return (
    <Card
      title={title}
      extra={
        <Space>
          {needsScheduling.length > 0 && onOpenScheduler && (
            <Button
              type="primary"
              icon={<CalendarOutlined />}
              onClick={onOpenScheduler}
            >
              Schedule Showings ({needsScheduling.length})
            </Button>
          )}
          {onCreateMatch && (
            <Button icon={<PlusOutlined />} onClick={onCreateMatch}>
              {propertyId ? 'Add Applicant' : 'Add Listing'}
            </Button>
          )}
        </Space>
      }
    >
      {isLoading ? (
        <div style={{ textAlign: 'center', padding: 40 }}>
          <Spin />
        </div>
      ) : (
        <Collapse
          defaultActiveKey={defaultActiveKeys}
          items={accordionItems}
          bordered={false}
          style={{ background: 'transparent' }}
        />
      )}
    </Card>
  );
};

export default PropertyMatchList;
