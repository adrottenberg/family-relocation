import { useEffect, useState, useRef } from 'react';
import { Modal, Spin, Typography, Divider, Empty, Button, Space } from 'antd';
import { PrinterOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { remindersApi, DueRemindersReportDto, ReminderListDto } from '../../api';

const { Title, Text } = Typography;

interface RemindersPrintViewProps {
  open: boolean;
  onClose: () => void;
}

const priorityOrder = ['Urgent', 'High', 'Normal', 'Low'];

const RemindersPrintView = ({ open, onClose }: RemindersPrintViewProps) => {
  const [loading, setLoading] = useState(true);
  const [report, setReport] = useState<DueRemindersReportDto | null>(null);
  const printRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (open) {
      fetchReport();
    }
  }, [open]);

  const fetchReport = async () => {
    setLoading(true);
    try {
      const data = await remindersApi.getDueReport(7);
      setReport(data);
    } catch (error) {
      console.error('Failed to fetch due report:', error);
    } finally {
      setLoading(false);
    }
  };

  const handlePrint = () => {
    const printContent = printRef.current;
    if (!printContent) return;

    const printWindow = window.open('', '_blank');
    if (!printWindow) return;

    printWindow.document.write(`
      <!DOCTYPE html>
      <html>
        <head>
          <title>Due Reminders Report</title>
          <style>
            body {
              font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
              padding: 20px;
              max-width: 800px;
              margin: 0 auto;
            }
            h1 { font-size: 24px; margin-bottom: 5px; }
            h2 { font-size: 18px; margin: 20px 0 10px; border-bottom: 2px solid #333; padding-bottom: 5px; }
            .header-date { color: #666; font-size: 14px; margin-bottom: 20px; }
            .summary { background: #f5f5f5; padding: 15px; border-radius: 4px; margin-bottom: 20px; }
            .summary-item { display: inline-block; margin-right: 30px; }
            .summary-count { font-size: 24px; font-weight: bold; }
            .summary-label { font-size: 12px; color: #666; }
            .reminder-item {
              padding: 10px 0;
              border-bottom: 1px solid #eee;
            }
            .reminder-title { font-weight: 600; margin-bottom: 5px; }
            .reminder-meta { font-size: 12px; color: #666; }
            .reminder-notes { font-size: 12px; color: #888; margin-top: 5px; font-style: italic; }
            .priority-urgent { color: #ff4d4f; }
            .priority-high { color: #fa8c16; }
            .priority-normal { color: #1890ff; }
            .priority-low { color: #8c8c8c; }
            .overdue { color: #ff4d4f; font-weight: bold; }
            .section-empty { color: #999; font-style: italic; padding: 10px 0; }
            @media print {
              body { padding: 0; }
              .no-print { display: none; }
            }
          </style>
        </head>
        <body>
          ${printContent.innerHTML}
        </body>
      </html>
    `);
    printWindow.document.close();
    printWindow.print();
  };

  const formatDate = (dateStr: string) => {
    return dayjs(dateStr).format('ddd, MMM D, YYYY');
  };

  const renderReminderList = (reminders: ReminderListDto[], emptyMessage: string) => {
    if (reminders.length === 0) {
      return <div className="section-empty">{emptyMessage}</div>;
    }

    return reminders.map((reminder) => (
      <div key={reminder.id} className="reminder-item">
        <div className="reminder-title">
          <input type="checkbox" style={{ marginRight: 8 }} />
          {reminder.title}
        </div>
        <div className="reminder-meta">
          <span className={reminder.isOverdue ? 'overdue' : ''}>
            Due: {formatDate(reminder.dueDate)}
            {reminder.dueTime && ` at ${reminder.dueTime.substring(0, 5)}`}
          </span>
          {' | '}
          <span className={`priority-${reminder.priority.toLowerCase()}`}>
            {reminder.priority}
          </span>
          {reminder.entityDisplayName && (
            <>
              {' | '}
              <span>{reminder.entityType}: {reminder.entityDisplayName}</span>
            </>
          )}
        </div>
      </div>
    ));
  };

  const groupByPriority = (reminders: ReminderListDto[]) => {
    const grouped: Record<string, ReminderListDto[]> = {};
    priorityOrder.forEach((priority) => {
      grouped[priority] = reminders.filter((r) => r.priority === priority);
    });
    return grouped;
  };

  return (
    <Modal
      title="Due Reminders Report"
      open={open}
      onCancel={onClose}
      width={800}
      footer={[
        <Button key="close" onClick={onClose}>
          Close
        </Button>,
        <Button key="print" type="primary" icon={<PrinterOutlined />} onClick={handlePrint}>
          Print
        </Button>,
      ]}
    >
      {loading ? (
        <div style={{ textAlign: 'center', padding: 40 }}>
          <Spin size="large" />
        </div>
      ) : !report ? (
        <Empty description="Failed to load report" />
      ) : (
        <div ref={printRef}>
          <Title level={3}>Due Reminders Report</Title>
          <div className="header-date">
            Generated: {dayjs().format('dddd, MMMM D, YYYY [at] h:mm A')}
          </div>

          <div className="summary">
            <Space size="large">
              <div className="summary-item">
                <div className="summary-count" style={{ color: '#ff4d4f' }}>
                  {report.overdueCount}
                </div>
                <div className="summary-label">Overdue</div>
              </div>
              <div className="summary-item">
                <div className="summary-count" style={{ color: '#faad14' }}>
                  {report.dueTodayCount}
                </div>
                <div className="summary-label">Due Today</div>
              </div>
              <div className="summary-item">
                <div className="summary-count" style={{ color: '#1890ff' }}>
                  {report.upcomingCount}
                </div>
                <div className="summary-label">Upcoming (7 days)</div>
              </div>
              <div className="summary-item">
                <div className="summary-count">{report.totalOpenCount}</div>
                <div className="summary-label">Total Open</div>
              </div>
            </Space>
          </div>

          {report.overdueCount > 0 && (
            <>
              <Title level={4} style={{ color: '#ff4d4f' }}>
                Overdue ({report.overdueCount})
              </Title>
              {renderReminderList(report.overdue, 'No overdue reminders')}
              <Divider />
            </>
          )}

          <Title level={4} style={{ color: '#faad14' }}>
            Due Today ({report.dueTodayCount})
          </Title>
          {renderReminderList(report.dueToday, 'No reminders due today')}
          <Divider />

          <Title level={4}>Upcoming (Next 7 Days)</Title>
          {report.upcomingCount === 0 ? (
            <Text type="secondary">No upcoming reminders</Text>
          ) : (
            Object.entries(groupByPriority(report.upcoming)).map(
              ([priority, reminders]) =>
                reminders.length > 0 && (
                  <div key={priority} style={{ marginBottom: 16 }}>
                    <Text strong className={`priority-${priority.toLowerCase()}`}>
                      {priority} Priority
                    </Text>
                    {renderReminderList(reminders, '')}
                  </div>
                )
            )
          )}
        </div>
      )}
    </Modal>
  );
};

export default RemindersPrintView;
