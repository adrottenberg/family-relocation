import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Table, Input, Select, Button, Tag, Space, Typography, Card, Empty, Tooltip } from 'antd';
import { SearchOutlined, PlusOutlined, FilterOutlined } from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { applicantsApi } from '../../api';
import type { ApplicantListItemDto } from '../../api/types';
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table';
import { colors, statusTagStyles, stageTagStyles } from '../../theme/antd-theme';
import './ApplicantListPage.css';

const { Title, Text } = Typography;
const { Option } = Select;

const ApplicantListPage = () => {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [boardDecision, setBoardDecision] = useState<string | undefined>();
  const [stage, setStage] = useState<string | undefined>();
  const [pagination, setPagination] = useState({ current: 1, pageSize: 10 });

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
      'Searching': 'houseHunting', // Reuse houseHunting style for Searching
      'UnderContract': 'underContract',
      'Closed': 'closed',
    };
    const key = stageMap[stageName];
    return key ? stageTagStyles[key] : { backgroundColor: colors.neutral[100], color: colors.neutral[600] };
  };

  const formatStageName = (stage: string) => {
    const names: Record<string, string> = {
      'Searching': 'Searching',
      'UnderContract': 'Under Contract',
      'Closed': 'Closed',
      'Paused': 'Paused',
      'MovedIn': 'Moved In',
    };
    return names[stage] || stage;
  };

  const columns: ColumnsType<ApplicantListItemDto> = [
    {
      title: 'FAMILY NAME',
      key: 'familyName',
      render: (_, record) => {
        const nameParts = record.husbandFullName.split(' ');
        const lastName = nameParts.pop() || record.husbandFullName;
        const firstName = nameParts.join(' ') || '';
        return (
          <div>
            <Text strong>{lastName}</Text>
            <div>
              <Text type="secondary" style={{ fontSize: 13 }}>
                {firstName}
                {record.wifeMaidenName && ` & ${record.wifeMaidenName}`}
              </Text>
            </div>
          </div>
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
        const stageName = record.stage || 'N/A';
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
        <Tooltip title="Coming soon">
          <Button
            type="primary"
            icon={<PlusOutlined />}
            disabled
          >
            Add Applicant
          </Button>
        </Tooltip>
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
            style={{ width: 160 }}
            allowClear
          >
            <Option value="Searching">Searching</Option>
            <Option value="UnderContract">Under Contract</Option>
            <Option value="Closed">Closed</Option>
            <Option value="MovedIn">Moved In</Option>
            <Option value="Paused">Paused</Option>
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
    </div>
  );
};

export default ApplicantListPage;
