import { Typography } from 'antd';

const { Title } = Typography;

/**
 * Login page - to be implemented in UV-23
 */
const LoginPage = () => {
  return (
    <div style={{
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      height: '100vh',
      background: 'linear-gradient(135deg, var(--brand-50) 0%, var(--primary-50) 100%)'
    }}>
      <Title level={2}>Login Page - Coming in UV-23</Title>
    </div>
  );
};

export default LoginPage;
