import { useEffect, useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import {
  Typography,
  Table,
  Button,
  Space,
  Tag,
  Select,
  Row,
  Col,
  Card,
  message,
  Tabs,
  Tooltip,
  Dropdown,
  DatePicker,
} from 'antd';
import {
  PlusOutlined,
  SearchOutlined,
  BellOutlined,
  CheckOutlined,
  ClockCircleOutlined,
  CloseOutlined,
  PrinterOutlined,
  MoreOutlined,
  ReloadOutlined,
  ExclamationCircleOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table';
import type { MenuProps } from 'antd';
import dayjs from 'dayjs';
import { formatDateTime, toUtcString } from '../../utils/datetime';
import {
  remindersApi,
  ReminderListDto,
  PaginatedList,
  ReminderPriority,
  ReminderStatus,
} from '../../api';
import CreateReminderModal from './CreateReminderModal';
import EditReminderModal from './EditReminderModal';
import SnoozeModal from './SnoozeModal';
import RemindersPrintView from './RemindersPrintView';
import ReminderDetailModal from './ReminderDetailModal';

const { Title, Text } = Typography;
const { Option } = Select;
const { RangePicker } = DatePicker;

const priorityColors: Record<ReminderPriority, string> = {
  Urgent: 'red',
  High: 'orange',
  Normal: 'blue',
  Low: 'default',
};

const statusColors: Record<ReminderStatus, string> = {
  Open: 'processing',
  Completed: 'success',
  Snoozed: 'warning',
  Dismissed: 'default',
};

const RemindersPage = () => {
  const queryClient = useQueryClient();
  const [reminders, setReminders] = useState<ReminderListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [pagination, setPagination] = useState<TablePaginationConfig>({
    current: 1,
    pageSize: 20,
    total: 0,
  });
  const [activeTab, setActiveTab] = useState<string>('Open');
  const [filters, setFilters] = useState({
    search: '',
    priority: '',
    entityType: '',
    dateRange: null as [dayjs.Dayjs, dayjs.Dayjs] | null,
  });

  // Modal states
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [snoozeModalOpen, setSnoozeModalOpen] = useState(false);
  const [detailModalOpen, setDetailModalOpen] = useState(false);
  const [selectedReminderId, setSelectedReminderId] = useState<string | null>(null);
  const [printViewOpen, setPrintViewOpen] = useState(false);

  const openDetailModal = (id: string) => {
    setSelectedReminderId(id);
    setDetailModalOpen(true);
  };

  const fetchReminders = useCallback(async (page = 1, pageSize = 20) => {
    setLoading(true);
    try {
      const params: Record<string, unknown> = {
        skip: (page - 1) * pageSize,
        take: pageSize,
        status: activeTab !== 'All' ? activeTab : undefined,
        priority: filters.priority || undefined,
        entityType: filters.entityType || undefined,
      };

      if (filters.dateRange) {
        params.dueDateTimeFrom = toUtcString(filters.dateRange[0].startOf('day'));
        params.dueDateTimeTo = toUtcString(filters.dateRange[1].endOf('day'));
      }

      const response: PaginatedList<ReminderListDto> = await remindersApi.getAll(params);
      setReminders(response.items);
      setPagination({
        current: response.page,
        pageSize: response.pageSize,
        total: response.totalCount,
      });
    } catch (error) {
      console.error('Failed to fetch reminders:', error);
      message.error('Failed to load reminders');
    } finally {
      setLoading(false);
    }
  }, [activeTab, filters]);

  useEffect(() => {
    fetchReminders();
  }, [fetchReminders]);

  const handleSearch = () => {
    fetchReminders(1, pagination.pageSize);
  };

  const handleTableChange = (newPagination: TablePaginationConfig) => {
    fetchReminders(newPagination.current ?? 1, newPagination.pageSize ?? 20);
  };

  const handleComplete = async (id: string) => {
    try {
      await remindersApi.complete(id);
      message.success('Reminder completed');
      fetchReminders(pagination.current, pagination.pageSize);
      queryClient.invalidateQueries({ queryKey: ['reminderCounts'] });
    } catch (error) {
      console.error('Failed to complete reminder:', error);
      message.error('Failed to complete reminder');
    }
  };

  const handleDismiss = async (id: string) => {
    try {
      await remindersApi.dismiss(id);
      message.success('Reminder dismissed');
      fetchReminders(pagination.current, pagination.pageSize);
      queryClient.invalidateQueries({ queryKey: ['reminderCounts'] });
    } catch (error) {
      console.error('Failed to dismiss reminder:', error);
      message.error('Failed to dismiss reminder');
    }
  };

  const handleReopen = async (id: string) => {
    try {
      await remindersApi.reopen(id);
      message.success('Reminder reopened');
      fetchReminders(pagination.current, pagination.pageSize);
      queryClient.invalidateQueries({ queryKey: ['reminderCounts'] });
    } catch (error) {
      console.error('Failed to reopen reminder:', error);
      message.error('Failed to reopen reminder');
    }
  };

  const openSnoozeModal = (id: string) => {
    setSelectedReminderId(id);
    setSnoozeModalOpen(true);
  };

  const handleSnooze = async (snoozeUntil: string) => {
    if (!selectedReminderId) return;
    try {
      await remindersApi.snooze(selectedReminderId, { snoozeUntil });
      message.success('Reminder snoozed');
      setSnoozeModalOpen(false);
      setSelectedReminderId(null);
      fetchReminders(pagination.current, pagination.pageSize);
      queryClient.invalidateQueries({ queryKey: ['reminderCounts'] });
    } catch (error) {
      console.error('Failed to snooze reminder:', error);
      message.error('Failed to snooze reminder');
    }
  };

  const getActionMenu = (record: ReminderListDto): MenuProps['items'] => {
    const items: MenuProps['items'] = [];

    if (record.status === 'Open' || record.status === 'Snoozed') {
      items.push(
        {
          key: 'complete',
          icon: <CheckOutlined />,
          label: 'Complete',
          onClick: () => handleComplete(record.id),
        },
        {
          key: 'snooze',
          icon: <ClockCircleOutlined />,
          label: 'Snooze',
          onClick: () => openSnoozeModal(record.id),
        },
        {
          key: 'dismiss',
          icon: <CloseOutlined />,
          label: 'Dismiss',
          onClick: () => handleDismiss(record.id),
        }
      );
    }

    if (record.status === 'Completed' || record.status === 'Dismissed') {
      items.push({
        key: 'reopen',
        icon: <ReloadOutlined />,
        label: 'Reopen',
        onClick: () => handleReopen(record.id),
      });
    }

    return items;
  };

  // formatDateTime from utils handles timezone conversion

  const columns: ColumnsType<ReminderListDto> = [
    {
      title: 'Title',
      dataIndex: 'title',
      key: 'title',
      render: (title: string, record) => (
        <Space>
          {record.isOverdue && (
            <Tooltip title="Overdue">
              <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
            </Tooltip>
          )}
          {record.isDueToday && !record.isOverdue && (
            <Tooltip title="Due Today">
              <ClockCircleOutlined style={{ color: '#faad14' }} />
            </Tooltip>
          )}
          <span style={{ fontWeight: record.isOverdue || record.isDueToday ? 600 : 400 }}>{title}</span>
          {record.snoozeCount > 0 && (
            <Tag>Snoozed {record.snoozeCount}x</Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Due Date',
      key: 'dueDateTime',
      render: (_, record) => {
        const isOverdue = record.isOverdue;
        const isDueToday = record.isDueToday;
        return (
          <Text
            style={{
              color: isOverdue ? '#ff4d4f' : isDueToday ? '#faad14' : undefined,
              fontWeight: isOverdue || isDueToday ? 600 : 400,
            }}
          >
            {formatDateTime(record.dueDateTime)}
            {isDueToday && !isOverdue && <Tag color="warning" style={{ marginLeft: 8 }}>Today</Tag>}
          </Text>
        );
      },
      sorter: true,
    },
    {
      title: 'Priority',
      dataIndex: 'priority',
      key: 'priority',
      width: 100,
      render: (priority: ReminderPriority) => (
        <Tag color={priorityColors[priority]}>{priority}</Tag>
      ),
    },
    {
      title: 'Related To',
      key: 'entity',
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Text type="secondary" style={{ fontSize: 12 }}>{record.entityType}</Text>
          {record.entityType === 'Applicant' && record.entityId ? (
            <Link
              to={`/applicants/${record.entityId}`}
              onClick={(e) => e.stopPropagation()}
            >
              {record.entityDisplayName || 'View Applicant'}
            </Link>
          ) : (
            <Text>{record.entityDisplayName || '-'}</Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (status: ReminderStatus) => (
        <Tag color={statusColors[status]}>{status}</Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (_, record) => (
        <Space>
          <Tooltip title="View Details">
            <Button
              type="text"
              size="small"
              icon={<EyeOutlined />}
              onClick={(e) => {
                e.stopPropagation();
                openDetailModal(record.id);
              }}
            />
          </Tooltip>
          {(record.status === 'Open' || record.status === 'Snoozed') && (
            <Tooltip title="Complete">
              <Button
                type="text"
                size="small"
                icon={<CheckOutlined />}
                onClick={(e) => {
                  e.stopPropagation();
                  handleComplete(record.id);
                }}
              />
            </Tooltip>
          )}
          <Dropdown
            menu={{ items: getActionMenu(record) }}
            trigger={['click']}
          >
            <Button
              type="text"
              size="small"
              icon={<MoreOutlined />}
              onClick={(e) => e.stopPropagation()}
            />
          </Dropdown>
        </Space>
      ),
    },
  ];

  const tabItems = [
    { key: 'All', label: 'All' },
    { key: 'Open', label: 'Open' },
    { key: 'Snoozed', label: 'Snoozed' },
    { key: 'Completed', label: 'Completed' },
    { key: 'Dismissed', label: 'Dismissed' },
  ];

  return (
    <div>
      <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
        <Col>
          <Space>
            <BellOutlined style={{ fontSize: 24 }} />
            <Title level={2} style={{ margin: 0 }}>
              Reminders
            </Title>
          </Space>
        </Col>
        <Col>
          <Space>
            <Button icon={<PrinterOutlined />} onClick={() => setPrintViewOpen(true)}>
              Print Due
            </Button>
            <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateModalOpen(true)}>
              Create Reminder
            </Button>
          </Space>
        </Col>
      </Row>

      {/* Status Tabs */}
      <Tabs
        activeKey={activeTab}
        onChange={(key) => {
          setActiveTab(key);
          setPagination({ ...pagination, current: 1 });
        }}
        items={tabItems}
        style={{ marginBottom: 16 }}
      />

      {/* Filters */}
      <Card style={{ marginBottom: 16 }}>
        <Row gutter={16}>
          <Col xs={24} sm={6}>
            <Select
              placeholder="Priority"
              allowClear
              style={{ width: '100%' }}
              value={filters.priority || undefined}
              onChange={(value) => setFilters({ ...filters, priority: value || '' })}
            >
              <Option value="Urgent">Urgent</Option>
              <Option value="High">High</Option>
              <Option value="Normal">Normal</Option>
              <Option value="Low">Low</Option>
            </Select>
          </Col>
          <Col xs={24} sm={6}>
            <Select
              placeholder="Entity Type"
              allowClear
              style={{ width: '100%' }}
              value={filters.entityType || undefined}
              onChange={(value) => setFilters({ ...filters, entityType: value || '' })}
            >
              <Option value="Applicant">Applicant</Option>
              <Option value="HousingSearch">Housing Search</Option>
              <Option value="Property">Property</Option>
              <Option value="General">General</Option>
            </Select>
          </Col>
          <Col xs={24} sm={8}>
            <RangePicker
              style={{ width: '100%' }}
              value={filters.dateRange}
              onChange={(dates) => setFilters({ ...filters, dateRange: dates as [dayjs.Dayjs, dayjs.Dayjs] | null })}
              placeholder={['Due From', 'Due To']}
            />
          </Col>
          <Col xs={24} sm={4}>
            <Button type="primary" onClick={handleSearch} block icon={<SearchOutlined />}>
              Search
            </Button>
          </Col>
        </Row>
      </Card>

      {/* Reminders Table */}
      <Table
        columns={columns}
        dataSource={reminders}
        rowKey="id"
        loading={loading}
        pagination={pagination}
        onChange={handleTableChange}
        onRow={(record) => ({
          onClick: () => openDetailModal(record.id),
          style: { cursor: 'pointer' },
        })}
        rowClassName={(record) => {
          if (record.isOverdue) return 'reminder-row-overdue';
          if (record.isDueToday) return 'reminder-row-today';
          return '';
        }}
      />

      {/* Create Reminder Modal */}
      <CreateReminderModal
        open={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        onSuccess={() => {
          setCreateModalOpen(false);
          fetchReminders(1, pagination.pageSize);
        }}
      />

      {/* Snooze Modal */}
      <SnoozeModal
        open={snoozeModalOpen}
        onClose={() => {
          setSnoozeModalOpen(false);
          setSelectedReminderId(null);
        }}
        onSnooze={handleSnooze}
      />

      {/* Print View */}
      <RemindersPrintView
        open={printViewOpen}
        onClose={() => setPrintViewOpen(false)}
      />

      {/* Reminder Detail Modal */}
      <ReminderDetailModal
        open={detailModalOpen}
        reminderId={selectedReminderId}
        onClose={() => {
          setDetailModalOpen(false);
          setSelectedReminderId(null);
        }}
        onComplete={(id) => {
          handleComplete(id);
          setDetailModalOpen(false);
        }}
        onSnooze={(id) => {
          setDetailModalOpen(false);
          openSnoozeModal(id);
        }}
        onDismiss={(id) => {
          handleDismiss(id);
          setDetailModalOpen(false);
        }}
        onReopen={(id) => {
          handleReopen(id);
          setDetailModalOpen(false);
        }}
        onEdit={(id) => {
          setDetailModalOpen(false);
          setSelectedReminderId(id);
          setEditModalOpen(true);
        }}
      />

      {/* Edit Reminder Modal */}
      <EditReminderModal
        open={editModalOpen}
        reminderId={selectedReminderId}
        onClose={() => {
          setEditModalOpen(false);
          setSelectedReminderId(null);
        }}
        onSuccess={() => {
          fetchReminders(pagination.current, pagination.pageSize);
          queryClient.invalidateQueries({ queryKey: ['reminderCounts'] });
          queryClient.invalidateQueries({ queryKey: ['reminders', 'due-report'] });
        }}
      />
    </div>
  );
};

export default RemindersPage;
