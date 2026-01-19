import { Layout } from 'antd';
import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';
import './AppLayout.css';

const { Content } = Layout;

const AppLayout = () => {
  return (
    <Layout className="app-layout">
      <Sidebar />
      <Layout className="main-layout">
        <Header />
        <Content className="main-content">
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
};

export default AppLayout;
