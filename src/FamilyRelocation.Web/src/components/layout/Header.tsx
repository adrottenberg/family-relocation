import { Layout, Input, Button, Badge } from 'antd';
import { SearchOutlined, BellOutlined, PlusOutlined } from '@ant-design/icons';
import { useLocation, useNavigate } from 'react-router-dom';

const { Header: AntHeader } = Layout;

const Header = () => {
  const location = useLocation();
  const navigate = useNavigate();

  // Get page title based on current route
  const getPageTitle = () => {
    const path = location.pathname;
    if (path.startsWith('/applicants/') && path !== '/applicants') return 'Applicant Details';
    if (path.startsWith('/applicants')) return 'Applicants';
    if (path.startsWith('/pipeline')) return 'Pipeline';
    if (path.startsWith('/settings')) return 'Settings';
    return 'Dashboard';
  };

  // Show "Add Applicant" button on applicants page
  const showAddButton = location.pathname === '/applicants';

  return (
    <AntHeader className="app-header">
      <div className="header-left">
        <h1 className="page-title">{getPageTitle()}</h1>
        <Input
          className="header-search"
          placeholder="Search families..."
          prefix={<SearchOutlined />}
          allowClear
        />
      </div>
      <div className="header-right">
        {showAddButton && (
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => navigate('/applicants/new')}
          >
            Add Applicant
          </Button>
        )}
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
