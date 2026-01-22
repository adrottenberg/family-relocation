import { useEffect } from 'react';
import { Layout } from 'antd';
import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';
import { useAuthStore } from '../../store/authStore';
import './AppLayout.css';

const { Content } = Layout;

const AppLayout = () => {
  const { fetchAndSetRoles } = useAuthStore();

  // Fetch roles from backend after login (non-blocking)
  useEffect(() => {
    fetchAndSetRoles();
  }, [fetchAndSetRoles]);

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
