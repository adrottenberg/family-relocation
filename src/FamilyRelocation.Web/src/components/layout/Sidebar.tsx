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
  CalendarOutlined,
  BankOutlined,
  FileTextOutlined,
  OrderedListOutlined,
  ScheduleOutlined,
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
    if (path.startsWith('/listings')) return 'listings';
    if (path.startsWith('/schedule-showings')) return 'schedule-showings';
    if (path.startsWith('/showings')) return 'showings';
    if (path.startsWith('/reminders')) return 'reminders';
    if (path.startsWith('/shuls')) return 'shuls';
    if (path.startsWith('/users')) return 'users';
    // Settings sub-routes need exact matching
    if (path === '/settings/document-types') return 'settings/document-types';
    if (path === '/settings/stage-requirements') return 'settings/stage-requirements';
    if (path.startsWith('/settings')) return 'settings/document-types';
    return 'dashboard';
  };

  // Determine which submenus should be open
  const getOpenKeys = () => {
    const path = location.pathname;
    const keys: string[] = [];
    if (path.startsWith('/listings') || path.startsWith('/showings') || path.startsWith('/schedule-showings')) {
      keys.push('listings-group');
    }
    if (path.startsWith('/shuls') || path.startsWith('/users') || path.startsWith('/settings')) {
      keys.push('settings-group');
    }
    return keys;
  };

  const isAdmin = user?.roles?.includes('Admin');

  // Use onClick instead of onSelect to ensure navigation works even when
  // clicking an already-selected item (e.g., clicking Applicants from /applicants/:id)
  const handleMenuClick: MenuProps['onClick'] = (info) => {
    const key = info.key;

    // Don't navigate for group/submenu keys
    if (key === 'listings-group' || key === 'settings-group') return;

    // Navigate to the path based on key
    const path = key === 'dashboard' ? '/dashboard' : `/${key}`;
    navigate(path);
  };

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
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
      key: 'listings-group',
      icon: <HomeOutlined />,
      label: 'Listings',
      children: [
        {
          key: 'listings',
          icon: <HomeOutlined />,
          label: 'All Listings',
        },
        {
          key: 'schedule-showings',
          icon: <ScheduleOutlined />,
          label: 'Schedule Showings',
        },
        {
          key: 'showings',
          icon: <CalendarOutlined />,
          label: 'Showings Calendar',
        },
      ],
    },
    {
      key: 'reminders',
      icon: <BellOutlined />,
      label: 'Reminders',
    },
    {
      key: 'settings-group',
      icon: <SettingOutlined />,
      label: 'Settings',
      children: [
        {
          key: 'shuls',
          icon: <BankOutlined />,
          label: 'Shuls',
        },
        {
          key: 'settings/document-types',
          icon: <FileTextOutlined />,
          label: 'Document Types',
        },
        {
          key: 'settings/stage-requirements',
          icon: <OrderedListOutlined />,
          label: 'Stage Requirements',
        },
        // Admin-only: Users
        ...(isAdmin
          ? [
              {
                key: 'users',
                icon: <UserOutlined />,
                label: 'Users',
              },
            ]
          : []),
      ],
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
        <img src="/logo.png" alt="וועד הישוב דקהילת יוניאן" title="וועד הישוב דקהילת יוניאן" className="sidebar-logo-image" />
      </div>

      {/* Navigation */}
      <Menu
        mode="inline"
        selectedKeys={[getSelectedKey()]}
        defaultOpenKeys={getOpenKeys()}
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
        <button className="logout-btn" onClick={handleLogout}>
          <LogoutOutlined /> Sign out
        </button>
      </div>
    </Sider>
  );
};

export default Sidebar;
