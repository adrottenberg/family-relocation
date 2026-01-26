import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { Table, Input, Select, Button, Tag, Space, Typography, Card, Empty, Badge } from 'antd';
import { SearchOutlined, PlusOutlined, FilterOutlined, BellOutlined } from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { applicantsApi, remindersApi } from '../../api';
import type { ApplicantListItemDto } from '../../api/types';
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table';
import { colors, statusTagStyles, stageTagStyles } from '../../theme/antd-theme';
import CreateApplicantModal from './CreateApplicantModal';
import './ApplicantListPage.css';

const { Title, Text } = Typography;
const { Option } = Select;

const ApplicantListPage = () => {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [boardDecision, setBoardDecision] = useState<string | undefined>();
  const [stage, setStage] = useState<string | undefined>();
  const [pagination, setPagination] = useState({ current: 1, pageSize: 10 });
  const [showCreateModal, setShowCreateModal] = useState(false);

  const { data, isLoading, error } = useQuery({
    queryKey: ['applicants', search, boardDecision, stage, pagination.current, pagination.pageSize],
    queryFn: () =>
      applicantsApi.getAll({
        page: pagination.current,
        pageSize: pagination.pageSize,
        search: search || undefined,
        boardDecision: boardDecision || undefined,
        stage: stage || undefined,
      }),
  });

  // Fetch due reminders report to show badges
  const { data: dueReport } = useQuery({
    queryKey: ['reminders', 'due-report'],
    queryFn: () => remindersApi.getDueReport(0), // Only overdue and due today
  });

  // Build a map of applicant IDs to their urgent reminder counts
  const applicantReminderCounts = useMemo(() => {
    const counts: Record<string, number> = {};
    if (!dueReport) return counts;

    // Combine overdue and due today, but deduplicate by reminder ID
    // (a reminder could theoretically appear in both arrays)
    const allDue = [...(dueReport.overdue || []), ...(dueReport.dueToday || [])];
    const uniqueReminders = new Map<string, typeof allDue[0]>();
    allDue.forEach(r => {
      if (!uniqueReminders.has(r.id)) {
        uniqueReminders.set(r.id, r);
      }
    });

    // Count unique reminders for applicants
    uniqueReminders.forEach(r => {
      if (r.entityType === 'Applicant') {
        counts[r.entityId] = (counts[r.entityId] || 0) + 1;
      }
    });

    return counts;
  }, [dueReport]);

  const handleTableChange = (newPagination: TablePaginationConfig) => {
    setPagination({
      current: newPagination.current || 1,
      pageSize: newPagination.pageSize || 10,
    });
  };

  const getStatusTagStyle = (decision: string) => {
    const key = decision.toLowerCase() as keyof typeof statusTagStyles;
    return statusTagStyles[key] || { backgroundColor: colors.neutral[100], color: colors.neutral[600] };
  };

  const getStageTagStyle = (stageName: string) => {
    const stageMap: Record<string, keyof typeof stageTagStyles> = {
      'Submitted': 'submitted',
      'AwaitingAgreements': 'submitted',
      'Searching': 'houseHunting',
      'UnderContract': 'underContract',
      'Closed': 'closed',
      'Rejected': 'rejected',
      'Paused': 'paused',
      'MovedIn': 'closed',
    };
    const key = stageMap[stageName];
    return key ? stageTagStyles[key] : { backgroundColor: colors.neutral[100], color: colors.neutral[600] };
  };

  const formatStageName = (stage: string) => {
    const names: Record<string, string> = {
      'Submitted': 'Submitted',
      'AwaitingAgreements': 'Awaiting Agreements',
      'Searching': 'Searching',
      'UnderContract': 'Under Contract',
      'Closed': 'Closed',
      'Paused': 'Paused',
      'MovedIn': 'Moved In',
      'Rejected': 'Rejected',
    };
    return names[stage] || stage;
  };

  const columns: ColumnsType<ApplicantListItemDto> = [
    {
      title: 'NAME',
      key: 'familyName',
      render: (_, record) => {
        const reminderCount = applicantReminderCounts[record.id] || 0;
        return (
          <Space>
            <Text strong>{record.husbandFullName}</Text>
            {reminderCount > 0 && (
              <Badge count={reminderCount} size="small" title={`${reminderCount} due reminder(s)`}>
                <BellOutlined style={{ color: '#ff4d4f', fontSize: 14 }} />
              </Badge>
            )}
          </Space>
        );
      },
    },
    {
      title: 'CONTACT',
      key: 'contact',
      render: (_, record) => (
        <div>
          <div>{record.husbandEmail || 'N/A'}</div>
          <Text type="secondary" style={{ fontSize: 13 }}>
            {record.husbandPhone || 'N/A'}
          </Text>
        </div>
      ),
    },
    {
      title: 'BOARD STATUS',
      key: 'boardDecision',
      width: 130,
      render: (_, record) => {
        const decision = record.boardDecision || 'Pending';
        const style = getStatusTagStyle(decision);
        return <Tag style={style}>{decision}</Tag>;
      },
    },
    {
      title: 'STAGE',
      key: 'stage',
      width: 140,
      render: (_, record) => {
        const stageName = record.stage || 'Submitted';
        const style = getStageTagStyle(stageName);
        return <Tag style={style}>{formatStageName(stageName)}</Tag>;
      },
    },
    {
      title: 'APPLIED',
      dataIndex: 'createdDate',
      key: 'createdDate',
      width: 110,
      render: (date: string) => new Date(date).toLocaleDateString(),
    },
  ];

  const clearFilters = () => {
    setSearch('');
    setBoardDecision(undefined);
    setStage(undefined);
  };

  const hasFilters = search || boardDecision || stage;

  if (error) {
    return (
      <Card>
        <Empty
          description="Failed to load applicants"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      </Card>
    );
  }

  return (
    <div className="applicant-list-page">
      {/* Page Header */}
      <div className="page-header">
        <div className="header-title-section">
          <Title level={3} style={{ margin: 0 }}>Applicants</Title>
          {data && (
            <Text type="secondary">{data.totalCount} total</Text>
          )}
        </div>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => setShowCreateModal(true)}
        >
          Add Applicant
        </Button>
      </div>

      {/* Filters */}
      <Card className="filters-card">
        <Space wrap size="middle">
          <Input
            placeholder="Search by name or email..."
            prefix={<SearchOutlined />}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            style={{ width: 280 }}
            allowClear
          />
          <Select
            placeholder="Board Status"
            value={boardDecision}
            onChange={setBoardDecision}
            style={{ width: 150 }}
            allowClear
          >
            <Option value="Pending">Pending</Option>
            <Option value="Approved">Approved</Option>
            <Option value="Rejected">Rejected</Option>
            <Option value="Deferred">Deferred</Option>
          </Select>
          <Select
            placeholder="Stage"
            value={stage}
            onChange={setStage}
            style={{ width: 180 }}
            allowClear
          >
            <Option value="Submitted">Submitted</Option>
            <Option value="AwaitingAgreements">Awaiting Agreements</Option>
            <Option value="Searching">Searching</Option>
            <Option value="UnderContract">Under Contract</Option>
            <Option value="Closed">Closed</Option>
            <Option value="MovedIn">Moved In</Option>
            <Option value="Paused">Paused</Option>
            <Option value="Rejected">Rejected</Option>
          </Select>
          {hasFilters && (
            <Button
              type="text"
              icon={<FilterOutlined />}
              onClick={clearFilters}
            >
              Clear filters
            </Button>
          )}
        </Space>
      </Card>

      {/* Table */}
      <Card className="table-card">
        <Table
          columns={columns}
          dataSource={data?.items}
          rowKey="id"
          loading={isLoading}
          pagination={{
            current: pagination.current,
            pageSize: pagination.pageSize,
            total: data?.totalCount,
            showSizeChanger: true,
            showTotal: (total, range) => `${range[0]}-${range[1]} of ${total}`,
          }}
          onChange={handleTableChange}
          onRow={(record) => ({
            onClick: () => navigate(`/applicants/${record.id}`),
            style: { cursor: 'pointer' },
          })}
          locale={{
            emptyText: (
              <Empty
                description={hasFilters ? 'No applicants match your filters' : 'No applicants yet'}
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            ),
          }}
        />
      </Card>

      <CreateApplicantModal
        open={showCreateModal}
        onClose={() => setShowCreateModal(false)}
      />
    </div>
  );
};

export default ApplicantListPage;
