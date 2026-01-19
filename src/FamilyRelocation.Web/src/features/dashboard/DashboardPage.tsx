import { Typography, Card, Row, Col } from 'antd';

const { Title, Text } = Typography;

/**
 * Dashboard page - basic placeholder
 */
const DashboardPage = () => {
  return (
    <div>
      <Title level={2}>Dashboard</Title>
      <Row gutter={[16, 16]}>
        <Col span={8}>
          <Card>
            <Text type="secondary">Total Applicants</Text>
            <Title level={3} style={{ margin: '8px 0 0' }}>--</Title>
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Text type="secondary">In Pipeline</Text>
            <Title level={3} style={{ margin: '8px 0 0' }}>--</Title>
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Text type="secondary">Moved In</Text>
            <Title level={3} style={{ margin: '8px 0 0' }}>--</Title>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default DashboardPage;
