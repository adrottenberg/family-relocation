import { useState } from 'react';
import { Modal, Space, Button, DatePicker, Typography } from 'antd';
import dayjs from 'dayjs';
import { toUtcString } from '../../utils/datetime';

const { Text } = Typography;

interface SnoozeModalProps {
  open: boolean;
  onClose: () => void;
  onSnooze: (snoozeUntil: string) => Promise<void>;
}

const SnoozeModal = ({ open, onClose, onSnooze }: SnoozeModalProps) => {
  const [customDate, setCustomDate] = useState<dayjs.Dayjs | null>(null);
  const [loading, setLoading] = useState(false);

  const handleQuickSnooze = async (days: number) => {
    setLoading(true);
    try {
      // Set snooze time to 9 AM in user's timezone, then convert to UTC
      const snoozeDateTime = dayjs().add(days, 'day').hour(9).minute(0).second(0);
      await onSnooze(toUtcString(snoozeDateTime));
    } finally {
      setLoading(false);
    }
  };

  const handleCustomSnooze = async () => {
    if (!customDate) return;
    setLoading(true);
    try {
      // Set snooze time to 9 AM in user's timezone, then convert to UTC
      const snoozeDateTime = customDate.hour(9).minute(0).second(0);
      await onSnooze(toUtcString(snoozeDateTime));
      setCustomDate(null);
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setCustomDate(null);
    onClose();
  };

  return (
    <Modal
      title="Snooze Reminder"
      open={open}
      onCancel={handleClose}
      footer={null}
      width={400}
    >
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        <Text type="secondary">Choose when to be reminded again:</Text>

        <Space direction="vertical" style={{ width: '100%' }}>
          <Button
            block
            onClick={() => handleQuickSnooze(1)}
            loading={loading}
          >
            Tomorrow ({dayjs().add(1, 'day').format('ddd, MMM D')})
          </Button>

          <Button
            block
            onClick={() => handleQuickSnooze(2)}
            loading={loading}
          >
            In 2 days ({dayjs().add(2, 'day').format('ddd, MMM D')})
          </Button>

          <Button
            block
            onClick={() => handleQuickSnooze(7)}
            loading={loading}
          >
            Next week ({dayjs().add(7, 'day').format('ddd, MMM D')})
          </Button>

          <Button
            block
            onClick={() => handleQuickSnooze(14)}
            loading={loading}
          >
            In 2 weeks ({dayjs().add(14, 'day').format('ddd, MMM D')})
          </Button>
        </Space>

        <div>
          <Text style={{ display: 'block', marginBottom: 8 }}>Or pick a custom date:</Text>
          <Space.Compact style={{ width: '100%' }}>
            <DatePicker
              style={{ flex: 1 }}
              value={customDate}
              onChange={setCustomDate}
              disabledDate={(current) => current && current <= dayjs().startOf('day')}
              format="MMMM D, YYYY"
            />
            <Button
              type="primary"
              onClick={handleCustomSnooze}
              disabled={!customDate}
              loading={loading}
            >
              Snooze
            </Button>
          </Space.Compact>
        </div>
      </Space>
    </Modal>
  );
};

export default SnoozeModal;
