import { Layout, Menu } from 'antd';
import {
  DashboardOutlined,
  TeamOutlined,
  AppstoreOutlined,
  SettingOutlined,
  LogoutOutlined,
} from '@ant-design/icons';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import type { MenuProps } from 'antd';

const { Sider } = Layout;

type MenuItem = Required<MenuProps>['items'][number];

const Sidebar = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuthStore();

  // Determine active menu key from current path
  const getSelectedKey = () => {
    const path = location.pathname;
    if (path.startsWith('/applicants')) return 'applicants';
    if (path.startsWith('/pipeline')) return 'pipeline';
    if (path.startsWith('/settings')) return 'settings';
    return 'dashboard';
  };

  const handleMenuClick: MenuProps['onClick'] = (e) => {
    if (e.key === 'logout') {
      logout();
      navigate('/login', { replace: true });
    } else {
      navigate(`/${e.key === 'dashboard' ? 'dashboard' : e.key}`);
    }
  };

  const menuItems: MenuItem[] = [
    {
      key: 'dashboard',
      icon: <DashboardOutlined />,
      label: 'Dashboard',
    },
    {
      key: 'applicants',
      icon: <TeamOutlined />,
      label: 'Applicants',
    },
    {
      key: 'pipeline',
      icon: <AppstoreOutlined />,
      label: 'Pipeline',
    },
    {
      key: 'settings',
      icon: <SettingOutlined />,
      label: 'Settings',
    },
  ];

  // Get user initials for avatar
  const getInitials = (email?: string) => {
    if (!email) return 'U';
    const name = email.split('@')[0];
    return name.substring(0, 2).toUpperCase();
  };

  return (
    <Sider className="app-sidebar" width={220} theme="light">
      {/* Logo */}
      <div className="sidebar-logo">
        <div className="logo-icon">VH</div>
        <div className="logo-text">
          <div className="logo-title">וועד הישוב</div>
          <div className="logo-subtitle">CRM</div>
        </div>
      </div>

      {/* Navigation */}
      <Menu
        mode="inline"
        selectedKeys={[getSelectedKey()]}
        items={menuItems}
        onClick={handleMenuClick}
        className="sidebar-menu"
      />

      {/* User section */}
      <div className="sidebar-footer">
        <div className="user-info">
          <div className="user-avatar">{getInitials(user?.email)}</div>
          <div className="user-details">
            <div className="user-name">{user?.email?.split('@')[0] || 'User'}</div>
            <div className="user-role">Coordinator</div>
          </div>
        </div>
        <button className="logout-btn" onClick={() => handleMenuClick({ key: 'logout' } as never)}>
          <LogoutOutlined /> Sign out
        </button>
      </div>
    </Sider>
  );
};

export default Sidebar;
