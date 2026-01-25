import { useDraggable } from '@dnd-kit/core';
import { CSS } from '@dnd-kit/utilities';
import { Card, Typography } from 'antd';
import { HolderOutlined } from '@ant-design/icons';
import type { PropertyMatchListDto } from '../../../api/types';
import MatchScoreDisplay from '../../propertyMatches/MatchScoreDisplay';

const { Text } = Typography;

interface DraggableMatchCardProps {
  match: PropertyMatchListDto;
}

const DraggableMatchCard = ({ match }: DraggableMatchCardProps) => {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: match.id,
    data: { type: 'match', match },
  });

  const style = {
    transform: CSS.Translate.toString(transform),
    opacity: isDragging ? 0.5 : 1,
    cursor: isDragging ? 'grabbing' : 'grab',
  };

  return (
    <Card
      ref={setNodeRef}
      style={style}
      size="small"
      {...attributes}
      {...listeners}
      styles={{
        body: { padding: '8px 12px' },
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
        <HolderOutlined style={{ color: '#999', fontSize: 14 }} />
        <div style={{ flex: 1, minWidth: 0 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Text strong style={{ fontSize: 13 }} ellipsis>
              {match.propertyStreet}
            </Text>
            <MatchScoreDisplay score={match.matchScore} size="small" />
          </div>
          <Text type="secondary" style={{ fontSize: 12 }} ellipsis>
            {match.applicantName}
          </Text>
        </div>
      </div>
    </Card>
  );
};

export default DraggableMatchCard;
