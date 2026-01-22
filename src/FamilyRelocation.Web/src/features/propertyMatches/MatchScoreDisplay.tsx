import { Progress, Tooltip, Space, Typography } from 'antd';
import type { MatchScoreBreakdownDto } from '../../api/types';

const { Text } = Typography;

interface MatchScoreDisplayProps {
  score: number;
  details?: MatchScoreBreakdownDto;
  showBreakdown?: boolean;
  size?: 'small' | 'default';
}

const getScoreColor = (score: number): string => {
  if (score >= 80) return '#52c41a'; // green
  if (score >= 60) return '#1890ff'; // blue
  if (score >= 40) return '#faad14'; // yellow
  return '#ff4d4f'; // red
};

const MatchScoreDisplay = ({ score, details, showBreakdown = false, size = 'default' }: MatchScoreDisplayProps) => {
  const color = getScoreColor(score);
  const progressSize = size === 'small' ? 60 : 80;

  const scoreCircle = (
    <Progress
      type="circle"
      percent={score}
      size={progressSize}
      strokeColor={color}
      format={(percent) => (
        <span style={{ fontSize: size === 'small' ? 14 : 18, fontWeight: 600 }}>
          {percent}
        </span>
      )}
    />
  );

  if (!details) {
    return scoreCircle;
  }

  const tooltipContent = (
    <div style={{ minWidth: 200 }}>
      <div style={{ marginBottom: 8, fontWeight: 600 }}>Score Breakdown</div>
      <ScoreRow label="Budget" score={details.budgetScore} max={details.maxBudgetScore} notes={details.budgetNotes} />
      <ScoreRow label="Bedrooms" score={details.bedroomsScore} max={details.maxBedroomsScore} notes={details.bedroomsNotes} />
      <ScoreRow label="Bathrooms" score={details.bathroomsScore} max={details.maxBathroomsScore} notes={details.bathroomsNotes} />
      <ScoreRow label="City" score={details.cityScore} max={details.maxCityScore} notes={details.cityNotes} />
      <ScoreRow label="Features" score={details.featuresScore} max={details.maxFeaturesScore} notes={details.featuresNotes} />
      <div style={{ marginTop: 8, paddingTop: 8, borderTop: '1px solid rgba(255,255,255,0.2)', fontWeight: 600 }}>
        Total: {details.totalScore}/{details.maxTotalScore}
      </div>
    </div>
  );

  if (!showBreakdown) {
    return (
      <Tooltip title={tooltipContent} placement="right">
        {scoreCircle}
      </Tooltip>
    );
  }

  return (
    <Space direction="vertical" size="small" style={{ width: '100%' }}>
      <div style={{ textAlign: 'center' }}>{scoreCircle}</div>
      <div>
        <ScoreBar label="Budget" score={details.budgetScore} max={details.maxBudgetScore} notes={details.budgetNotes} />
        <ScoreBar label="Bedrooms" score={details.bedroomsScore} max={details.maxBedroomsScore} notes={details.bedroomsNotes} />
        <ScoreBar label="Bathrooms" score={details.bathroomsScore} max={details.maxBathroomsScore} notes={details.bathroomsNotes} />
        <ScoreBar label="City" score={details.cityScore} max={details.maxCityScore} notes={details.cityNotes} />
        <ScoreBar label="Features" score={details.featuresScore} max={details.maxFeaturesScore} notes={details.featuresNotes} />
      </div>
    </Space>
  );
};

const ScoreRow = ({ label, score, max, notes }: { label: string; score: number; max: number; notes?: string }) => (
  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
    <span>{label}:</span>
    <Tooltip title={notes}>
      <span>{score}/{max}</span>
    </Tooltip>
  </div>
);

const ScoreBar = ({ label, score, max, notes }: { label: string; score: number; max: number; notes?: string }) => {
  const percent = max > 0 ? (score / max) * 100 : 0;
  return (
    <div style={{ marginBottom: 8 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 2 }}>
        <Text type="secondary" style={{ fontSize: 12 }}>{label}</Text>
        <Text style={{ fontSize: 12 }}>{score}/{max}</Text>
      </div>
      <Tooltip title={notes}>
        <Progress percent={percent} size="small" showInfo={false} strokeColor={getScoreColor(percent)} />
      </Tooltip>
    </div>
  );
};

export default MatchScoreDisplay;
