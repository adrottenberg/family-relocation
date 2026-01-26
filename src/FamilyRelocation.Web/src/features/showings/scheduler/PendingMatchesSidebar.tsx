import { useDroppable } from '@dnd-kit/core';
import { Typography, Empty, Skeleton } from 'antd';
import { CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import type { PropertyMatchListDto } from '../../../api/types';
import DraggableMatchCard from './DraggableMatchCard';
import { parseUtcToLocal, today } from '../../../utils/datetime';

const { Text } = Typography;

interface PendingMatchesSidebarProps {
  matches: PropertyMatchListDto[];
  loading?: boolean;
}

const PendingMatchesSidebar = ({ matches, loading }: PendingMatchesSidebarProps) => {
  const todayStart = today();

  // Make sidebar a droppable zone for cancellation
  const { isOver, setNodeRef } = useDroppable({
    id: 'cancel-zone',
    data: {
      type: 'cancel-zone',
    },
  });

  // Filter to only show matches that need scheduling:
  // - ShowingRequested status AND no FUTURE scheduled showings
  // (Past scheduled showings that weren't marked complete shouldn't block scheduling)
  const pendingMatches = matches.filter(m =>
    m.status === 'ShowingRequested' &&
    !m.showings?.some(s =>
      s.status === 'Scheduled' &&
      !parseUtcToLocal(s.scheduledDateTime).isBefore(todayStart, 'day')
    )
  );

  return (
    <div
      style={{
        width: 260,
        borderRight: '1px solid #f0f0f0',
        display: 'flex',
        flexDirection: 'column',
        height: '100%',
      }}
    >
      <div
        style={{
          padding: '12px 16px',
          borderBottom: '1px solid #f0f0f0',
          background: '#fafafa',
        }}
      >
        <Text strong>Pending Matches ({pendingMatches.length})</Text>
      </div>

      <div
        ref={setNodeRef}
        style={{
          flex: 1,
          overflow: 'auto',
          padding: 12,
          display: 'flex',
          flexDirection: 'column',
          gap: 8,
          background: isOver ? '#fff1f0' : 'transparent',
          transition: 'background 0.2s',
        }}
      >
        {isOver ? (
          <div
            style={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              height: '100%',
              color: '#ff4d4f',
            }}
          >
            <CloseCircleOutlined style={{ fontSize: 48, marginBottom: 8 }} />
            <Text type="danger" strong>Drop to Cancel Showing</Text>
          </div>
        ) : loading ? (
          <>
            <Skeleton.Input active block style={{ height: 60 }} />
            <Skeleton.Input active block style={{ height: 60 }} />
            <Skeleton.Input active block style={{ height: 60 }} />
          </>
        ) : pendingMatches.length === 0 ? (
          <Empty
            image={<CheckCircleOutlined style={{ fontSize: 48, color: '#52c41a' }} />}
            description={
              <Text type="secondary">All showings scheduled!</Text>
            }
            style={{ marginTop: 40 }}
          />
        ) : (
          pendingMatches.map((match) => (
            <DraggableMatchCard key={match.id} match={match} />
          ))
        )}
      </div>

      {!loading && !isOver && pendingMatches.length > 0 && (
        <div
          style={{
            padding: '8px 16px',
            borderTop: '1px solid #f0f0f0',
            background: '#fafafa',
          }}
        >
          <Text type="secondary" style={{ fontSize: 12 }}>
            Drag a match to the calendar to schedule
          </Text>
        </div>
      )}
    </div>
  );
};

export default PendingMatchesSidebar;
