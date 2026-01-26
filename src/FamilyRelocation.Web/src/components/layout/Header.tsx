import { Layout, Button, Badge, Tooltip } from 'antd';
import { BellOutlined } from '@ant-design/icons';
import { useLocation, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { remindersApi } from '../../api';

const { Header: AntHeader } = Layout;

const Header = () => {
  const location = useLocation();
  const navigate = useNavigate();

  // Fetch global reminder counts using due report
  // Use upcomingDays=0 to only count overdue and due today (not upcoming)
  const { data: dueReport } = useQuery({
    queryKey: ['reminders', 'due-report'],
    queryFn: () => remindersApi.getDueReport(0),
    refetchInterval: 60000, // Refresh every minute
  });

  const urgentCount = (dueReport?.overdueCount || 0) + (dueReport?.dueTodayCount || 0);

  // Get page title based on current route
  const getPageTitle = () => {
    const path = location.pathname;
    if (path.startsWith('/applicants/') && path !== '/applicants') return 'Applicant Details';
    if (path.startsWith('/applicants')) return 'Applicants';
    if (path.startsWith('/pipeline')) return 'Pipeline';
    if (path.startsWith('/listings/') && path !== '/listings') return 'Listing Details';
    if (path.startsWith('/listings')) return 'Listings';
    if (path.startsWith('/showings')) return 'Showings Calendar';
    if (path.startsWith('/reminders')) return 'Reminders';
    if (path.startsWith('/shuls')) return 'Shuls';
    if (path.startsWith('/users')) return 'Users';
    if (path === '/settings/document-types') return 'Document Types';
    if (path === '/settings/stage-requirements') return 'Stage Requirements';
    if (path.startsWith('/settings')) return 'Settings';
    return 'Dashboard';
  };

  return (
    <AntHeader className="app-header">
      <div className="header-left">
        <h1 className="page-title">{getPageTitle()}</h1>
      </div>
      <div className="header-right">
        <Tooltip title={urgentCount > 0 ? `${urgentCount} reminder${urgentCount > 1 ? 's' : ''} ${urgentCount === 1 ? 'needs' : 'need'} attention` : 'No urgent reminders'}>
          <Badge count={urgentCount} showZero={false}>
            <Button
              type="text"
              icon={<BellOutlined style={{ fontSize: 18, color: urgentCount > 0 ? '#ff4d4f' : undefined }} />}
              className="notification-btn"
              onClick={() => navigate('/reminders')}
            />
          </Badge>
        </Tooltip>
      </div>
    </AntHeader>
  );
};

export default Header;
