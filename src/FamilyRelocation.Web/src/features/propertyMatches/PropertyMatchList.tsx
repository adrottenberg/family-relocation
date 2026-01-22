import { useState } from 'react';
import { Card, Tabs, Button, Space, Empty, Spin, message } from 'antd';
import { PlusOutlined, CalendarOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { propertyMatchesApi } from '../../api';
import type { PropertyMatchListDto } from '../../api/types';
import PropertyMatchCard from './PropertyMatchCard';

interface PropertyMatchListProps {
  housingSearchId?: string;
  propertyId?: string;
  onCreateMatch?: () => void;
  onScheduleShowings?: (matchIds: string[]) => void;
  showApplicant?: boolean;
  showProperty?: boolean;
}

const statusTabs = [
  { key: 'all', label: 'All' },
  { key: 'MatchIdentified', label: 'Identified' },
  { key: 'ShowingRequested', label: 'Showing Requested' },
  { key: 'ApplicantInterested', label: 'Interested' },
  { key: 'OfferMade', label: 'Offer Made' },
  { key: 'ApplicantRejected', label: 'Rejected' },
];

const PropertyMatchList = ({
  housingSearchId,
  propertyId,
  onCreateMatch,
  onScheduleShowings,
  showApplicant = true,
  showProperty = true,
}: PropertyMatchListProps) => {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState('all');
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  const queryKey = housingSearchId
    ? ['propertyMatches', 'housingSearch', housingSearchId, activeTab]
    : ['propertyMatches', 'property', propertyId, activeTab];

  const { data: matches, isLoading } = useQuery({
    queryKey,
    queryFn: () => {
      const status = activeTab === 'all' ? undefined : activeTab;
      if (housingSearchId) {
        return propertyMatchesApi.getForHousingSearch(housingSearchId, status);
      }
      return propertyMatchesApi.getForProperty(propertyId!, status);
    },
    enabled: !!(housingSearchId || propertyId),
  });

  const requestShowingsMutation = useMutation({
    mutationFn: (matchIds: string[]) => propertyMatchesApi.requestShowings(matchIds),
    onSuccess: (data) => {
      message.success(`${data.updatedCount} match(es) updated to Showing Requested`);
      queryClient.invalidateQueries({ queryKey: ['propertyMatches'] });
      setSelectedIds(new Set());
    },
    onError: () => {
      message.error('Failed to request showings');
    },
  });

  const handleSelect = (id: string, selected: boolean) => {
    const newSet = new Set(selectedIds);
    if (selected) {
      newSet.add(id);
    } else {
      newSet.delete(id);
    }
    setSelectedIds(newSet);
  };

  const handleSelectAll = () => {
    const identifiedMatches = (matches || []).filter(m => m.status === 'MatchIdentified');
    if (selectedIds.size === identifiedMatches.length) {
      setSelectedIds(new Set());
    } else {
      setSelectedIds(new Set(identifiedMatches.map(m => m.id)));
    }
  };

  const handleRequestShowings = () => {
    if (selectedIds.size === 0) return;
    requestShowingsMutation.mutate(Array.from(selectedIds));
  };

  const handleSingleRequestShowing = (matchId: string) => {
    requestShowingsMutation.mutate([matchId]);
  };

  const identifiedMatches = (matches || []).filter(m => m.status === 'MatchIdentified');
  const showingRequestedMatches = (matches || []).filter(m => m.status === 'ShowingRequested');
  const hasSelectable = identifiedMatches.length > 0;

  const filteredMatches = activeTab === 'all'
    ? matches
    : matches?.filter(m => m.status === activeTab);

  return (
    <Card
      title="Property Matches"
      extra={
        <Space>
          {showingRequestedMatches.length > 0 && onScheduleShowings && (
            <Button
              icon={<CalendarOutlined />}
              onClick={() => onScheduleShowings(showingRequestedMatches.map(m => m.id))}
            >
              Schedule Showings ({showingRequestedMatches.length})
            </Button>
          )}
          {onCreateMatch && (
            <Button type="primary" icon={<PlusOutlined />} onClick={onCreateMatch}>
              Add Match
            </Button>
          )}
        </Space>
      }
    >
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={statusTabs.map(tab => ({
          key: tab.key,
          label: (
            <span>
              {tab.label}
              {matches && (
                <span style={{ marginLeft: 4, color: '#999' }}>
                  ({tab.key === 'all'
                    ? matches.length
                    : matches.filter(m => m.status === tab.key).length})
                </span>
              )}
            </span>
          ),
        }))}
      />

      {/* Batch actions for MatchIdentified */}
      {hasSelectable && activeTab !== 'ApplicantRejected' && (
        <div style={{ marginBottom: 12, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Button size="small" onClick={handleSelectAll}>
            {selectedIds.size === identifiedMatches.length ? 'Deselect All' : 'Select All Identified'}
          </Button>
          {selectedIds.size > 0 && (
            <Button
              type="primary"
              size="small"
              loading={requestShowingsMutation.isPending}
              onClick={handleRequestShowings}
            >
              Request Showing ({selectedIds.size})
            </Button>
          )}
        </div>
      )}

      {isLoading ? (
        <div style={{ textAlign: 'center', padding: 40 }}>
          <Spin />
        </div>
      ) : !filteredMatches || filteredMatches.length === 0 ? (
        <Empty description="No matches found" />
      ) : (
        filteredMatches.map((match: PropertyMatchListDto) => (
          <PropertyMatchCard
            key={match.id}
            match={match}
            selectable={match.status === 'MatchIdentified'}
            selected={selectedIds.has(match.id)}
            onSelect={handleSelect}
            onRequestShowing={handleSingleRequestShowing}
            onScheduleShowing={onScheduleShowings ? (id) => onScheduleShowings([id]) : undefined}
            showApplicant={showApplicant}
            showProperty={showProperty}
          />
        ))
      )}
    </Card>
  );
};

export default PropertyMatchList;
