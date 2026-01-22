import { Layout, Menu } from 'antd';
import {
  DashboardOutlined,
  TeamOutlined,
  AppstoreOutlined,
  SettingOutlined,
  LogoutOutlined,
  HomeOutlined,
  BellOutlined,
  UserOutlined,
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
    if (path.startsWith('/properties')) return 'properties';
    if (path.startsWith('/reminders')) return 'reminders';
    if (path.startsWith('/settings')) return 'settings';
    if (path.startsWith('/users')) return 'users';
    return 'dashboard';
  };

  const isAdmin = user?.roles?.includes('Admin');

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
      key: 'properties',
      icon: <HomeOutlined />,
      label: 'Properties',
    },
    {
      key: 'reminders',
      icon: <BellOutlined />,
      label: 'Reminders',
    },
    {
      key: 'settings',
      icon: <SettingOutlined />,
      label: 'Settings',
    },
    // Admin-only menu items
    ...(isAdmin
      ? [
          {
            key: 'users',
            icon: <UserOutlined />,
            label: 'Users',
          },
        ]
      : []),
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
        <img src="/logo.png" alt="וועד הישוב דקהילת יוניאן" title="וועד הישוב דקהילת יוניאן" className="sidebar-logo-image" />
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
            <div className="user-role">{user?.roles?.[0] || 'User'}</div>
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
