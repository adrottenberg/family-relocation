import { useState } from 'react';
import { Card, Tabs, Button, Space, Empty, Spin, message } from 'antd';
import { PlusOutlined, CalendarOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { propertyMatchesApi } from '../../api';
import type { PropertyMatchListDto } from '../../api/types';
import PropertyMatchCard from './PropertyMatchCard';

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
  onOpenScheduler,
  showApplicant = true,
  showProperty = true,
}: PropertyMatchListProps) => {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState('all');
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Context-aware title: from applicant view = "Suggested Listings", from property view = "Suggested Applicants"
  const title = propertyId ? 'Suggested Applicants' : 'Suggested Listings';

  // Always fetch all matches - filter client-side for display, keeps tab counts stable
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
    const identified = (allMatches || []).filter(m => m.status === 'MatchIdentified');
    if (selectedIds.size === identified.length) {
      setSelectedIds(new Set());
    } else {
      setSelectedIds(new Set(identified.map(m => m.id)));
    }
  };

  const handleRequestShowings = () => {
    if (selectedIds.size === 0) return;
    requestShowingsMutation.mutate(Array.from(selectedIds));
  };

  const handleSingleRequestShowing = (matchId: string) => {
    requestShowingsMutation.mutate([matchId]);
  };

  const identifiedMatches = (allMatches || []).filter(m => m.status === 'MatchIdentified');
  const showingRequestedMatches = (allMatches || []).filter(m => m.status === 'ShowingRequested');
  const hasSelectable = identifiedMatches.length > 0;

  // Filter for display based on active tab
  const filteredMatches = activeTab === 'all'
    ? allMatches
    : allMatches?.filter(m => m.status === activeTab);

  return (
    <Card
      title={title}
      extra={
        <Space>
          {showingRequestedMatches.length > 0 && onOpenScheduler && (
            <Button
              type="primary"
              icon={<CalendarOutlined />}
              onClick={onOpenScheduler}
            >
              Schedule Showings ({showingRequestedMatches.length})
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
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={statusTabs.map(tab => ({
          key: tab.key,
          label: (
            <span>
              {tab.label}
              {allMatches && (
                <span style={{ marginLeft: 4, color: '#999' }}>
                  ({tab.key === 'all'
                    ? allMatches.length
                    : allMatches.filter(m => m.status === tab.key).length})
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
            onScheduleShowing={onScheduleShowings ? (id) => {
              const match = allMatches?.find(m => m.id === id);
              if (match) {
                onScheduleShowings([{
                  id: match.id,
                  propertyStreet: match.propertyStreet,
                  propertyCity: match.propertyCity,
                  applicantName: match.applicantName,
                }]);
              }
            } : undefined}
            showApplicant={showApplicant}
            showProperty={showProperty}
          />
        ))
      )}
    </Card>
  );
};

export default PropertyMatchList;
