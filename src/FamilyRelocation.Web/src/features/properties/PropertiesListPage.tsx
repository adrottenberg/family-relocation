import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Typography,
  Table,
  Button,
  Tag,
  Input,
  Select,
  Row,
  Col,
  Card,
  message,
  Image,
  Modal,
  Form,
  InputNumber,
} from 'antd';
import {
  PlusOutlined,
  SearchOutlined,
  HomeOutlined,
  DollarOutlined,
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
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [createLoading, setCreateLoading] = useState(false);
  const [form] = Form.useForm();

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

  const handleCreateListing = async () => {
    try {
      const values = await form.validateFields();
      setCreateLoading(true);
      const newProperty = await propertiesApi.create({
        street: values.street,
        city: values.city,
        state: 'NJ',
        zipCode: values.zipCode || '',
        price: values.price,
        bedrooms: values.bedrooms,
        bathrooms: values.bathrooms,
        squareFeet: values.squareFeet,
        mlsNumber: values.mlsNumber,
      });
      message.success('Listing created successfully');
      setCreateModalOpen(false);
      form.resetFields();
      // Navigate to the new listing
      navigate(`/listings/${newProperty.id}`);
    } catch (error) {
      if (error instanceof Error) {
        message.error(error.message || 'Failed to create listing');
      }
    } finally {
      setCreateLoading(false);
    }
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
      title: '',
      key: 'photo',
      width: 80,
      render: (_, record) => (
        record.primaryPhotoUrl ? (
          <Image
            src={record.primaryPhotoUrl}
            alt={record.street}
            width={60}
            height={45}
            style={{ objectFit: 'cover', borderRadius: 4 }}
            preview={false}
          />
        ) : (
          <div
            style={{
              width: 60,
              height: 45,
              background: '#f0f0f0',
              borderRadius: 4,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <HomeOutlined style={{ color: '#999' }} />
          </div>
        )
      ),
    },
    {
      title: 'Address',
      key: 'address',
      render: (_, record) => (
        <span>
          <strong>{record.street}</strong>
          <br />
          <span style={{ color: '#888' }}>{record.city}</span>
        </span>
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
            Listings
          </Title>
        </Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateModalOpen(true)}>
            Add Listing
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

      {/* Listings Table */}
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

      {/* Create Listing Modal */}
      <Modal
        title="Add New Listing"
        open={createModalOpen}
        onCancel={() => {
          setCreateModalOpen(false);
          form.resetFields();
        }}
        onOk={handleCreateListing}
        confirmLoading={createLoading}
        width={600}
      >
        <Form form={form} layout="vertical">
          <Row gutter={16}>
            <Col span={16}>
              <Form.Item name="street" label="Street Address" rules={[{ required: true, message: 'Street address is required' }]}>
                <Input placeholder="123 Main St" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="city" label="City" rules={[{ required: true, message: 'City is required' }]}>
                <Select placeholder="Select city">
                  <Option value="Union">Union</Option>
                  <Option value="Roselle Park">Roselle Park</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="zipCode" label="Zip Code">
                <Input placeholder="07083" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="mlsNumber" label="MLS #">
                <Input placeholder="MLS Number" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="price" label="Price" rules={[{ required: true, message: 'Price is required' }]}>
                <InputNumber
                  prefix={<DollarOutlined />}
                  style={{ width: '100%' }}
                  min={0}
                  formatter={(value) => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={(value) => Number(value?.replace(/\$\s?|(,*)/g, '') || 0) as unknown as 0}
                  placeholder="500,000"
                />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="bedrooms" label="Bedrooms" rules={[{ required: true, message: 'Bedrooms is required' }]}>
                <InputNumber style={{ width: '100%' }} min={0} placeholder="4" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="bathrooms" label="Bathrooms" rules={[{ required: true, message: 'Bathrooms is required' }]}>
                <InputNumber style={{ width: '100%' }} min={0} step={0.5} placeholder="2.5" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="squareFeet" label="Square Feet">
                <InputNumber style={{ width: '100%' }} min={0} placeholder="2,000" />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>
    </div>
  );
};

export default PropertiesListPage;
