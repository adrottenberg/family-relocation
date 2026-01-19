import { Layout } from 'antd';
import { Outlet } from 'react-router-dom';

const { Content } = Layout;

/**
 * App shell layout - to be implemented in UV-24
 */
const AppLayout = () => {
  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Content style={{ padding: 24 }}>
        <Outlet />
      </Content>
    </Layout>
  );
};

export default AppLayout;
