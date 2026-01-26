import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Typography,
  Table,
  Button,
  Space,
  Tag,
  Input,
  Select,
  Row,
  Col,
  Card,
  message,
} from 'antd';
import {
  PlusOutlined,
  SearchOutlined,
  HomeOutlined,
} from '@ant-design/icons';
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table';
import { propertiesApi, PropertyListDto, PaginatedList } from '../../api';

const { Title } = Typography;
const { Option } = Select;

const statusColors: Record<string, string> = {
  Active: 'green',
  UnderContract: 'orange',
  Sold: 'blue',
  OffMarket: 'default',
};

const PropertiesListPage = () => {
  const navigate = useNavigate();
  const [properties, setProperties] = useState<PropertyListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [pagination, setPagination] = useState<TablePaginationConfig>({
    current: 1,
    pageSize: 20,
    total: 0,
  });
  const [filters, setFilters] = useState({
    search: '',
    status: '',
    city: '',
  });

  const fetchProperties = async (page = 1, pageSize = 20) => {
    setLoading(true);
    try {
      const response: PaginatedList<PropertyListDto> = await propertiesApi.getAll({
        page,
        pageSize,
        search: filters.search || undefined,
        status: filters.status || undefined,
        city: filters.city || undefined,
      });
      setProperties(response.items);
      setPagination({
        current: response.page,
        pageSize: response.pageSize,
        total: response.totalCount,
      });
    } catch (error) {
      console.error('Failed to fetch properties:', error);
      message.error('Failed to load properties');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProperties();
  }, []);

  const handleSearch = () => {
    fetchProperties(1, pagination.pageSize);
  };

  const handleTableChange = (newPagination: TablePaginationConfig) => {
    fetchProperties(newPagination.current ?? 1, newPagination.pageSize ?? 20);
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 0,
    }).format(price);
  };

  const columns: ColumnsType<PropertyListDto> = [
    {
      title: 'Address',
      key: 'address',
      render: (_, record) => (
        <Space>
          <HomeOutlined />
          <span>
            <strong>{record.street}</strong>
            <br />
            <span style={{ color: '#888' }}>{record.city}</span>
          </span>
        </Space>
      ),
    },
    {
      title: 'Price',
      dataIndex: 'price',
      key: 'price',
      render: (price: number) => (
        <span style={{ fontWeight: 600, color: '#1890ff' }}>{formatPrice(price)}</span>
      ),
      sorter: true,
    },
    {
      title: 'Beds',
      dataIndex: 'bedrooms',
      key: 'bedrooms',
      width: 80,
      align: 'center',
    },
    {
      title: 'Baths',
      dataIndex: 'bathrooms',
      key: 'bathrooms',
      width: 80,
      align: 'center',
    },
    {
      title: 'Sq Ft',
      dataIndex: 'squareFeet',
      key: 'squareFeet',
      render: (sqft?: number) => sqft?.toLocaleString() ?? '-',
      width: 100,
    },
    {
      title: 'MLS #',
      dataIndex: 'mlsNumber',
      key: 'mlsNumber',
      render: (mls?: string) => mls ?? '-',
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Tag color={statusColors[status] || 'default'}>{status}</Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, record: PropertyListDto) => (
        <Button type="link" size="small" onClick={() => navigate(`/listings/${record.id}`)}>
          View
        </Button>
      ),
    },
  ];

  return (
    <div>
      <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
        <Col>
          <Title level={2} style={{ margin: 0 }}>
            Properties
          </Title>
        </Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />}>
            Add Property
          </Button>
        </Col>
      </Row>

      {/* Filters */}
      <Card style={{ marginBottom: 16 }}>
        <Row gutter={16}>
          <Col xs={24} sm={8}>
            <Input
              placeholder="Search by address or MLS #"
              prefix={<SearchOutlined />}
              value={filters.search}
              onChange={(e) => setFilters({ ...filters, search: e.target.value })}
              onPressEnter={handleSearch}
            />
          </Col>
          <Col xs={24} sm={6}>
            <Select
              placeholder="Status"
              allowClear
              style={{ width: '100%' }}
              value={filters.status || undefined}
              onChange={(value) => setFilters({ ...filters, status: value || '' })}
            >
              <Option value="Active">Active</Option>
              <Option value="UnderContract">Under Contract</Option>
              <Option value="Sold">Sold</Option>
              <Option value="OffMarket">Off Market</Option>
            </Select>
          </Col>
          <Col xs={24} sm={6}>
            <Select
              placeholder="City"
              allowClear
              style={{ width: '100%' }}
              value={filters.city || undefined}
              onChange={(value) => setFilters({ ...filters, city: value || '' })}
            >
              <Option value="Union">Union</Option>
              <Option value="Roselle Park">Roselle Park</Option>
            </Select>
          </Col>
          <Col xs={24} sm={4}>
            <Button type="primary" onClick={handleSearch} block>
              Search
            </Button>
          </Col>
        </Row>
      </Card>

      {/* Properties Table */}
      <Table
        columns={columns}
        dataSource={properties}
        rowKey="id"
        loading={loading}
        pagination={pagination}
        onChange={handleTableChange}
        onRow={(record) => ({
          onClick: () => navigate(`/listings/${record.id}`),
          style: { cursor: 'pointer' },
        })}
      />
    </div>
  );
};

export default PropertiesListPage;
