import { Layout, Button, Badge } from 'antd';
import { BellOutlined } from '@ant-design/icons';
import { useLocation } from 'react-router-dom';

const { Header: AntHeader } = Layout;

const Header = () => {
  const location = useLocation();

  // Get page title based on current route
  const getPageTitle = () => {
    const path = location.pathname;
    if (path.startsWith('/applicants/') && path !== '/applicants') return 'Applicant Details';
    if (path.startsWith('/applicants')) return 'Applicants';
    if (path.startsWith('/pipeline')) return 'Pipeline';
    if (path.startsWith('/properties/') && path !== '/properties') return 'Property Details';
    if (path.startsWith('/properties')) return 'Properties';
    if (path.startsWith('/showings')) return 'Showings';
    if (path.startsWith('/reminders')) return 'Reminders';
    if (path.startsWith('/settings')) return 'Settings';
    return 'Dashboard';
  };

  return (
    <AntHeader className="app-header">
      <div className="header-left">
        <h1 className="page-title">{getPageTitle()}</h1>
      </div>
      <div className="header-right">
        <Badge count={0} showZero={false}>
          <Button
            type="text"
            icon={<BellOutlined style={{ fontSize: 18 }} />}
            className="notification-btn"
          />
        </Badge>
      </div>
    </AntHeader>
  );
};

export default Header;
