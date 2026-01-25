import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Card,
  Typography,
  Tag,
  Button,
  Descriptions,
  Space,
  Spin,
  Empty,
  Row,
  Col,
  Image,
  Upload,
  message,
  Popconfirm,
  Modal,
  Form,
  Input,
  InputNumber,
  Select,
} from 'antd';
import {
  ArrowLeftOutlined,
  EditOutlined,
  HomeOutlined,
  DollarOutlined,
  UploadOutlined,
  DeleteOutlined,
  StarOutlined,
  StarFilled,
  EnvironmentOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { propertiesApi, shulsApi } from '../../api';
import type { PropertyPhotoDto, PropertyShulDistanceDto } from '../../api/types';
import { PropertyMatchList, CreatePropertyMatchModal, type MatchScheduleData } from '../propertyMatches';
import { ScheduleShowingModal } from '../showings';

const { Title, Text } = Typography;
const { TextArea } = Input;
const { Option } = Select;

const statusColors: Record<string, string> = {
  Active: 'green',
  UnderContract: 'orange',
  Sold: 'blue',
  OffMarket: 'default',
};

const statusLabels: Record<string, string> = {
  Active: 'Active',
  UnderContract: 'Under Contract',
  Sold: 'Sold',
  OffMarket: 'Off Market',
};

const PropertyDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [form] = Form.useForm();
  const [createMatchModalOpen, setCreateMatchModalOpen] = useState(false);
  // Queue of showings to schedule - each modal close advances to the next
  const [showingsToSchedule, setShowingsToSchedule] = useState<MatchScheduleData[]>([]);

  const { data: property, isLoading, error } = useQuery({
    queryKey: ['property', id],
    queryFn: () => propertiesApi.getById(id!),
    enabled: !!id,
  });

  const { data: shulDistances, isLoading: shulDistancesLoading } = useQuery({
    queryKey: ['property-shul-distances', id],
    queryFn: () => shulsApi.getPropertyDistances(id!),
    enabled: !!id,
  });

  const updateMutation = useMutation({
    mutationFn: (values: Parameters<typeof propertiesApi.update>[1]) =>
      propertiesApi.update(id!, values),
    onSuccess: () => {
      message.success('Property updated successfully');
      queryClient.invalidateQueries({ queryKey: ['property', id] });
      setEditModalOpen(false);
    },
    onError: () => {
      message.error('Failed to update property');
    },
  });

  const uploadPhotoMutation = useMutation({
    mutationFn: (file: File) => propertiesApi.uploadPhoto(id!, file),
    onSuccess: () => {
      message.success('Photo uploaded successfully');
      queryClient.invalidateQueries({ queryKey: ['property', id] });
    },
    onError: () => {
      message.error('Failed to upload photo');
    },
  });

  const deletePhotoMutation = useMutation({
    mutationFn: (photoId: string) => propertiesApi.deletePhoto(id!, photoId),
    onSuccess: () => {
      message.success('Photo deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['property', id] });
    },
    onError: () => {
      message.error('Failed to delete photo');
    },
  });

  const setPrimaryPhotoMutation = useMutation({
    mutationFn: (photoId: string) => propertiesApi.setPrimaryPhoto(id!, photoId),
    onSuccess: () => {
      message.success('Primary photo updated');
      queryClient.invalidateQueries({ queryKey: ['property', id] });
    },
    onError: () => {
      message.error('Failed to set primary photo');
    },
  });

  const handleEdit = () => {
    if (!property) return;
    form.setFieldsValue({
      street: property.address.street,
      street2: property.address.street2,
      city: property.address.city,
      state: property.address.state,
      zipCode: property.address.zipCode,
      price: property.price,
      bedrooms: property.bedrooms,
      bathrooms: property.bathrooms,
      squareFeet: property.squareFeet,
      lotSize: property.lotSize,
      yearBuilt: property.yearBuilt,
      annualTaxes: property.annualTaxes,
      features: property.features,
      mlsNumber: property.mlsNumber,
      notes: property.notes,
      status: property.status,
    });
    setEditModalOpen(true);
  };

  const handleEditSubmit = async () => {
    try {
      const values = await form.validateFields();
      updateMutation.mutate({
        id: id!,
        ...values,
      });
    } catch {
      // Form validation error
    }
  };

  const handlePhotoUpload = (file: File) => {
    if (property && property.photos.length >= 10) {
      message.error('Maximum 10 photos allowed per property');
      return false;
    }
    uploadPhotoMutation.mutate(file);
    return false; // Prevent default upload behavior
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 0,
    }).format(price);
  };

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 100 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (error || !property) {
    return (
      <Card>
        <Empty description="Property not found" />
        <div style={{ textAlign: 'center', marginTop: 16 }}>
          <Button onClick={() => navigate('/properties')}>Back to Properties</Button>
        </div>
      </Card>
    );
  }

  return (
    <div>
      {/* Header */}
      <div style={{ marginBottom: 24 }}>
        <Button
          type="text"
          icon={<ArrowLeftOutlined />}
          onClick={() => navigate('/properties')}
          style={{ marginBottom: 8 }}
        >
          Back to Properties
        </Button>
        <Row justify="space-between" align="middle">
          <Col>
            <Space align="center">
              <HomeOutlined style={{ fontSize: 24 }} />
              <Title level={2} style={{ margin: 0 }}>
                {property.address.street}
              </Title>
              <Tag color={statusColors[property.status] || 'default'}>
                {statusLabels[property.status] || property.status}
              </Tag>
            </Space>
            <div style={{ marginTop: 4 }}>
              <Text type="secondary">
                {property.address.city}, {property.address.state} {property.address.zipCode}
              </Text>
            </div>
          </Col>
          <Col>
            <Button type="primary" icon={<EditOutlined />} onClick={handleEdit}>
              Edit Property
            </Button>
          </Col>
        </Row>
      </div>

      <Row gutter={[24, 24]}>
        {/* Main Details */}
        <Col xs={24} lg={16}>
          <Card title="Property Details">
            <Row gutter={[48, 24]}>
              <Col xs={12} sm={6}>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: 28, fontWeight: 600, color: '#1890ff' }}>
                    {formatPrice(property.price)}
                  </div>
                  <Text type="secondary">Price</Text>
                </div>
              </Col>
              <Col xs={12} sm={6}>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: 28, fontWeight: 600 }}>{property.bedrooms}</div>
                  <Text type="secondary">Bedrooms</Text>
                </div>
              </Col>
              <Col xs={12} sm={6}>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: 28, fontWeight: 600 }}>{property.bathrooms}</div>
                  <Text type="secondary">Bathrooms</Text>
                </div>
              </Col>
              <Col xs={12} sm={6}>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: 28, fontWeight: 600 }}>
                    {property.squareFeet?.toLocaleString() || '-'}
                  </div>
                  <Text type="secondary">Sq Ft</Text>
                </div>
              </Col>
            </Row>

            <Descriptions column={{ xs: 1, sm: 2 }} style={{ marginTop: 24 }}>
              <Descriptions.Item label="MLS Number">
                {property.mlsNumber || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Year Built">
                {property.yearBuilt || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Lot Size">
                {property.lotSize ? `${property.lotSize} acres` : '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Annual Taxes">
                {property.annualTaxes ? formatPrice(property.annualTaxes) : '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Features" span={2}>
                {property.features.length > 0 ? (
                  <Space wrap>
                    {property.features.map((feature, index) => (
                      <Tag key={index}>{feature}</Tag>
                    ))}
                  </Space>
                ) : (
                  '-'
                )}
              </Descriptions.Item>
              <Descriptions.Item label="Notes" span={2}>
                {property.notes || '-'}
              </Descriptions.Item>
            </Descriptions>
          </Card>

          {/* Photos Section */}
          <Card
            title="Photos"
            style={{ marginTop: 24 }}
            extra={
              <Upload
                accept="image/jpeg,image/png"
                showUploadList={false}
                beforeUpload={handlePhotoUpload}
                disabled={uploadPhotoMutation.isPending || property.photos.length >= 10}
              >
                <Button
                  icon={<UploadOutlined />}
                  loading={uploadPhotoMutation.isPending}
                  disabled={property.photos.length >= 10}
                >
                  Upload Photo
                </Button>
              </Upload>
            }
          >
            {property.photos.length > 0 ? (
              <>
                {/* Primary Photo - Large Display */}
                {(() => {
                  const primaryPhoto = property.photos.find(p => p.isPrimary) || property.photos[0];
                  return primaryPhoto && (
                    <div style={{ marginBottom: 16 }}>
                      <Image
                        src={primaryPhoto.url}
                        alt={primaryPhoto.description || 'Primary property photo'}
                        style={{ width: '100%', maxHeight: 400, objectFit: 'cover', borderRadius: 8 }}
                      />
                      <div style={{ marginTop: 4, display: 'flex', alignItems: 'center', gap: 8 }}>
                        <StarFilled style={{ color: '#faad14' }} />
                        <Text type="secondary">Primary Photo</Text>
                      </div>
                    </div>
                  );
                })()}

                {/* Photo Thumbnails */}
                <Image.PreviewGroup>
                  <Row gutter={[16, 16]}>
                    {property.photos.map((photo: PropertyPhotoDto) => (
                      <Col key={photo.id} xs={12} sm={8} md={6}>
                        <div style={{
                          position: 'relative',
                          border: photo.isPrimary ? '2px solid #faad14' : '2px solid transparent',
                          borderRadius: 4,
                        }}>
                          <Image
                            src={photo.url}
                            alt={photo.description || 'Property photo'}
                            style={{ width: '100%', height: 150, objectFit: 'cover' }}
                          />
                          {/* Primary indicator badge */}
                          {photo.isPrimary && (
                            <div style={{
                              position: 'absolute',
                              top: 4,
                              left: 4,
                              background: '#faad14',
                              color: 'white',
                              padding: '2px 6px',
                              borderRadius: 4,
                              fontSize: 11,
                              fontWeight: 500,
                            }}>
                              Primary
                            </div>
                          )}
                          {/* Action buttons */}
                          <div style={{
                            position: 'absolute',
                            top: 4,
                            right: 4,
                            display: 'flex',
                            gap: 4,
                          }}>
                            {!photo.isPrimary && (
                              <Button
                                type="text"
                                icon={<StarOutlined />}
                                size="small"
                                title="Set as primary"
                                onClick={() => setPrimaryPhotoMutation.mutate(photo.id)}
                                style={{
                                  background: 'rgba(255,255,255,0.8)',
                                }}
                                loading={setPrimaryPhotoMutation.isPending}
                              />
                            )}
                            <Popconfirm
                              title="Delete this photo?"
                              onConfirm={() => deletePhotoMutation.mutate(photo.id)}
                              okText="Delete"
                              cancelText="Cancel"
                            >
                              <Button
                                type="text"
                                danger
                                icon={<DeleteOutlined />}
                                size="small"
                                style={{
                                  background: 'rgba(255,255,255,0.8)',
                                }}
                                loading={deletePhotoMutation.isPending}
                              />
                            </Popconfirm>
                          </div>
                        </div>
                        {photo.description && (
                          <Text type="secondary" style={{ fontSize: 12 }}>
                            {photo.description}
                          </Text>
                        )}
                      </Col>
                    ))}
                  </Row>
                </Image.PreviewGroup>
              </>
            ) : (
              <Empty description="No photos uploaded" />
            )}
            <div style={{ marginTop: 8 }}>
              <Text type="secondary">{property.photos.length}/10 photos</Text>
            </div>
          </Card>

          {/* Interested Families / Property Matches */}
          <div style={{ marginTop: 24 }}>
            <PropertyMatchList
              propertyId={property.id}
              onCreateMatch={() => setCreateMatchModalOpen(true)}
              onScheduleShowings={(matches) => {
                // Queue all matches for sequential scheduling
                setShowingsToSchedule(matches);
              }}
              showApplicant={true}
              showProperty={false}
            />
          </div>
        </Col>

        {/* Sidebar */}
        <Col xs={24} lg={8}>
          <Card title="Status & Dates">
            <Descriptions column={1} size="small">
              <Descriptions.Item label="Status">
                <Tag color={statusColors[property.status] || 'default'}>
                  {statusLabels[property.status] || property.status}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Created">
                {new Date(property.createdAt).toLocaleDateString()}
              </Descriptions.Item>
              {property.modifiedAt && (
                <Descriptions.Item label="Last Modified">
                  {new Date(property.modifiedAt).toLocaleDateString()}
                </Descriptions.Item>
              )}
            </Descriptions>
          </Card>

          {/* Walking Distances to Shuls */}
          <Card
            title={
              <Space>
                <EnvironmentOutlined />
                Walking to Shuls
              </Space>
            }
            style={{ marginTop: 24 }}
            size="small"
          >
            {shulDistancesLoading ? (
              <div style={{ textAlign: 'center', padding: 16 }}>
                <Spin size="small" />
              </div>
            ) : shulDistances && shulDistances.length > 0 ? (
              <div style={{ maxHeight: 300, overflowY: 'auto' }}>
                {shulDistances.map((distance: PropertyShulDistanceDto) => (
                  <div
                    key={distance.shulId}
                    style={{
                      padding: '8px 0',
                      borderBottom: '1px solid #f0f0f0',
                    }}
                  >
                    <div style={{ fontWeight: 500 }}>{distance.shulName}</div>
                    <div style={{ display: 'flex', gap: 16, color: '#666', fontSize: 13 }}>
                      <span>{distance.distanceMiles.toFixed(2)} mi</span>
                      <span>{distance.walkingTimeMinutes} min walk</span>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <Empty
                description="No shul distances calculated yet"
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            )}
          </Card>
        </Col>
      </Row>

      {/* Edit Modal */}
      <Modal
        title="Edit Property"
        open={editModalOpen}
        onCancel={() => setEditModalOpen(false)}
        onOk={handleEditSubmit}
        confirmLoading={updateMutation.isPending}
        width={700}
      >
        <Form form={form} layout="vertical">
          <Title level={5}>Address</Title>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="street" label="Street" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="street2" label="Street 2">
                <Input />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="city" label="City" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="state" label="State" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="zipCode" label="Zip Code" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
            </Col>
          </Row>

          <Title level={5}>Details</Title>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="price" label="Price" rules={[{ required: true }]}>
                <InputNumber
                  prefix={<DollarOutlined />}
                  style={{ width: '100%' }}
                  min={0}
                  formatter={(value) => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={(value) => Number(value?.replace(/\$\s?|(,*)/g, '') || 0) as unknown as 0}
                />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="bedrooms" label="Bedrooms" rules={[{ required: true }]}>
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="bathrooms" label="Bathrooms" rules={[{ required: true }]}>
                <InputNumber style={{ width: '100%' }} min={0} step={0.5} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="squareFeet" label="Square Feet">
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="lotSize" label="Lot Size (acres)">
                <InputNumber style={{ width: '100%' }} min={0} step={0.01} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="yearBuilt" label="Year Built">
                <InputNumber style={{ width: '100%' }} min={1800} max={new Date().getFullYear()} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="annualTaxes" label="Annual Taxes">
                <InputNumber
                  prefix={<DollarOutlined />}
                  style={{ width: '100%' }}
                  min={0}
                  formatter={(value) => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={(value) => Number(value?.replace(/\$\s?|(,*)/g, '') || 0) as unknown as 0}
                />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="mlsNumber" label="MLS Number">
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="status" label="Status" rules={[{ required: true }]}>
                <Select>
                  <Option value="Active">Active</Option>
                  <Option value="UnderContract">Under Contract</Option>
                  <Option value="Sold">Sold</Option>
                  <Option value="OffMarket">Off Market</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="features" label="Features">
            <Select mode="tags" placeholder="Add features (press Enter to add)">
              <Option value="Central Air">Central Air</Option>
              <Option value="Garage">Garage</Option>
              <Option value="Basement">Basement</Option>
              <Option value="Pool">Pool</Option>
              <Option value="Fireplace">Fireplace</Option>
              <Option value="Hardwood Floors">Hardwood Floors</Option>
            </Select>
          </Form.Item>
          <Form.Item name="notes" label="Notes">
            <TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>

      {/* Create Property Match Modal */}
      <CreatePropertyMatchModal
        open={createMatchModalOpen}
        onClose={() => setCreateMatchModalOpen(false)}
        propertyId={property.id}
      />

      {/* Schedule Showing Modal - processes queue of showings sequentially */}
      {showingsToSchedule.length > 0 && (
        <ScheduleShowingModal
          open={true}
          onClose={() => {
            // Advance to next showing in queue, or clear if done
            setShowingsToSchedule((prev) => prev.slice(1));
          }}
          propertyMatchId={showingsToSchedule[0].id}
          propertyInfo={{
            street: showingsToSchedule[0].propertyStreet,
            city: showingsToSchedule[0].propertyCity,
          }}
          applicantInfo={{ name: showingsToSchedule[0].applicantName }}
          queueInfo={showingsToSchedule.length > 1 ? {
            current: 1,
            total: showingsToSchedule.length,
          } : undefined}
        />
      )}
    </div>
  );
};

export default PropertyDetailPage;
