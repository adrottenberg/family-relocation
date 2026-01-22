import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Form, Input, Button, Checkbox, Alert, Typography, message } from 'antd';
import { EyeInvisibleOutlined, EyeTwoTone, MailOutlined, LockOutlined, NumberOutlined } from '@ant-design/icons';
import { useAuthStore } from '../../store/authStore';
import { authApi, isChallengeResponse, getApiError } from '../../api';
import type { LoginResponse, ChallengeResponse } from '../../api/types';
import './LoginPage.css';

const { Title, Text, Link } = Typography;

interface LoginFormValues {
  email: string;
  password: string;
  remember: boolean;
}

interface ChallengeFormValues {
  newPassword: string;
  confirmPassword: string;
}

interface ForgotPasswordFormValues {
  email: string;
}

interface ResetPasswordFormValues {
  code: string;
  newPassword: string;
  confirmPassword: string;
}

type ForgotPasswordStep = 'email' | 'code' | null;

const LoginPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { setTokens, setUser } = useAuthStore();

  // Get return URL from navigation state (set by ProtectedRoute)
  const returnUrl = (location.state as { returnUrl?: string })?.returnUrl || '/dashboard';

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [challenge, setChallenge] = useState<ChallengeResponse | null>(null);
  const [email, setEmail] = useState('');
  const [forgotPasswordStep, setForgotPasswordStep] = useState<ForgotPasswordStep>(null);

  const [loginForm] = Form.useForm<LoginFormValues>();
  const [challengeForm] = Form.useForm<ChallengeFormValues>();
  const [forgotPasswordForm] = Form.useForm<ForgotPasswordFormValues>();
  const [resetPasswordForm] = Form.useForm<ResetPasswordFormValues>();

  const handleLogin = async (values: LoginFormValues) => {
    setLoading(true);
    setError(null);

    try {
      const response = await authApi.login({
        email: values.email,
        password: values.password,
      });

      if (isChallengeResponse(response)) {
        // Handle challenge (e.g., NEW_PASSWORD_REQUIRED)
        setChallenge(response);
        setEmail(values.email);
      } else {
        // Success - store tokens and redirect
        handleLoginSuccess(response as LoginResponse, values.email);
      }
    } catch (err) {
      const apiError = getApiError(err);
      setError(apiError.message);
    } finally {
      setLoading(false);
    }
  };

  const handleChallengeResponse = async (values: ChallengeFormValues) => {
    if (!challenge) return;

    setLoading(true);
    setError(null);

    try {
      const response = await authApi.respondToChallenge({
        email,
        challengeName: challenge.challengeName,
        session: challenge.session,
        responses: {
          newPassword: values.newPassword,
        },
      });

      if (isChallengeResponse(response)) {
        // Another challenge
        setChallenge(response);
      } else {
        // Success
        handleLoginSuccess(response as LoginResponse, email);
      }
    } catch (err) {
      const apiError = getApiError(err);
      setError(apiError.message);
    } finally {
      setLoading(false);
    }
  };

  const handleLoginSuccess = (tokens: LoginResponse, userEmail: string) => {
    setTokens({
      accessToken: tokens.accessToken,
      idToken: tokens.idToken,
      refreshToken: tokens.refreshToken,
      expiresIn: tokens.expiresIn,
    });
    // Roles will be fetched from backend by AppLayout after navigation
    setUser({ email: userEmail, roles: [] });
    // Navigate to the original URL they tried to access, or dashboard
    navigate(returnUrl, { replace: true });
  };

  const handleForgotPassword = () => {
    setError(null);
    setForgotPasswordStep('email');
  };

  const handleForgotPasswordSubmit = async (values: ForgotPasswordFormValues) => {
    setLoading(true);
    setError(null);

    try {
      await authApi.forgotPassword(values.email);
      setEmail(values.email);
      setForgotPasswordStep('code');
      message.success('Verification code sent to your email');
    } catch (err) {
      const apiError = getApiError(err);
      setError(apiError.message);
    } finally {
      setLoading(false);
    }
  };

  const handleResetPasswordSubmit = async (values: ResetPasswordFormValues) => {
    setLoading(true);
    setError(null);

    try {
      await authApi.confirmForgotPassword({
        email,
        code: values.code,
        newPassword: values.newPassword,
      });
      message.success('Password reset successfully. Please sign in.');
      setForgotPasswordStep(null);
      forgotPasswordForm.resetFields();
      resetPasswordForm.resetFields();
    } catch (err) {
      const apiError = getApiError(err);
      setError(apiError.message);
    } finally {
      setLoading(false);
    }
  };

  const handleBackToLogin = () => {
    setForgotPasswordStep(null);
    setError(null);
    forgotPasswordForm.resetFields();
    resetPasswordForm.resetFields();
  };

  // Forgot password - enter email
  if (forgotPasswordStep === 'email') {
    return (
      <div className="login-background">
        <div className="login-container">
          <div className="login-card">
            <div className="logo-section">
              <img src="/logo.png" alt="Vaad HaYishuv Logo" className="logo-image" />
            </div>

            <Title level={4} className="login-title">Forgot Password</Title>
            <Text type="secondary" className="login-subtitle">
              Enter your email address and we'll send you a verification code
            </Text>

            {error && (
              <Alert
                message={error}
                type="error"
                showIcon
                className="error-alert"
              />
            )}

            <Form
              form={forgotPasswordForm}
              layout="vertical"
              onFinish={handleForgotPasswordSubmit}
              requiredMark={false}
            >
              <Form.Item
                name="email"
                label="Email"
                validateTrigger="onBlur"
                rules={[
                  { required: true, message: 'Please enter your email' },
                  { type: 'email', message: 'Please enter a valid email' },
                ]}
              >
                <Input
                  size="large"
                  placeholder="you@example.com"
                  prefix={<MailOutlined className="input-icon" />}
                />
              </Form.Item>

              <Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  size="large"
                  block
                  loading={loading}
                >
                  {loading ? 'Sending...' : 'Send Verification Code'}
                </Button>
              </Form.Item>

              <Button type="link" block onClick={handleBackToLogin}>
                Back to Login
              </Button>
            </Form>
          </div>
        </div>
      </div>
    );
  }

  // Forgot password - enter code and new password
  if (forgotPasswordStep === 'code') {
    return (
      <div className="login-background">
        <div className="login-container">
          <div className="login-card">
            <div className="logo-section">
              <img src="/logo.png" alt="Vaad HaYishuv Logo" className="logo-image" />
            </div>

            <Title level={4} className="login-title">Reset Password</Title>
            <Text type="secondary" className="login-subtitle">
              Enter the verification code sent to {email}
            </Text>

            {error && (
              <Alert
                message={error}
                type="error"
                showIcon
                className="error-alert"
              />
            )}

            <Form
              form={resetPasswordForm}
              layout="vertical"
              onFinish={handleResetPasswordSubmit}
              requiredMark={false}
            >
              <Form.Item
                name="code"
                label="Verification Code"
                rules={[{ required: true, message: 'Please enter the verification code' }]}
              >
                <Input
                  size="large"
                  placeholder="Enter code"
                  prefix={<NumberOutlined className="input-icon" />}
                />
              </Form.Item>

              <Form.Item
                name="newPassword"
                label="New Password"
                rules={[
                  { required: true, message: 'Please enter a new password' },
                  { min: 8, message: 'Password must be at least 8 characters' },
                ]}
              >
                <Input.Password
                  size="large"
                  placeholder="Enter new password"
                  prefix={<LockOutlined className="input-icon" />}
                  iconRender={(visible) =>
                    visible ? <EyeTwoTone /> : <EyeInvisibleOutlined />
                  }
                />
              </Form.Item>

              <Form.Item
                name="confirmPassword"
                label="Confirm Password"
                dependencies={['newPassword']}
                rules={[
                  { required: true, message: 'Please confirm your password' },
                  ({ getFieldValue }) => ({
                    validator(_, value) {
                      if (!value || getFieldValue('newPassword') === value) {
                        return Promise.resolve();
                      }
                      return Promise.reject(new Error('Passwords do not match'));
                    },
                  }),
                ]}
              >
                <Input.Password
                  size="large"
                  placeholder="Confirm new password"
                  prefix={<LockOutlined className="input-icon" />}
                  iconRender={(visible) =>
                    visible ? <EyeTwoTone /> : <EyeInvisibleOutlined />
                  }
                />
              </Form.Item>

              <Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  size="large"
                  block
                  loading={loading}
                >
                  {loading ? 'Resetting...' : 'Reset Password'}
                </Button>
              </Form.Item>

              <Button type="link" block onClick={handleBackToLogin}>
                Back to Login
              </Button>
            </Form>
          </div>
        </div>
      </div>
    );
  }

  // Challenge form (e.g., NEW_PASSWORD_REQUIRED)
  if (challenge) {
    return (
      <div className="login-background">
        <div className="login-container">
          <div className="login-card">
            <div className="logo-section">
              <img src="/logo.png" alt="Vaad HaYishuv Logo" className="logo-image" />
            </div>

            <Title level={4} className="login-title">{challenge.message}</Title>
            <Text type="secondary" className="login-subtitle">
              {challenge.challengeName === 'NEW_PASSWORD_REQUIRED'
                ? 'Please set a new password to continue'
                : 'Additional verification required'}
            </Text>

            {error && (
              <Alert
                message={error}
                type="error"
                showIcon
                className="error-alert"
              />
            )}

            <Form
              form={challengeForm}
              layout="vertical"
              onFinish={handleChallengeResponse}
              requiredMark={false}
            >
              {challenge.requiredFields.includes('newPassword') && (
                <>
                  <Form.Item
                    name="newPassword"
                    label="New Password"
                    rules={[
                      { required: true, message: 'Please enter a new password' },
                      { min: 8, message: 'Password must be at least 8 characters' },
                    ]}
                  >
                    <Input.Password
                      size="large"
                      placeholder="Enter new password"
                      prefix={<LockOutlined className="input-icon" />}
                      iconRender={(visible) =>
                        visible ? <EyeTwoTone /> : <EyeInvisibleOutlined />
                      }
                    />
                  </Form.Item>

                  <Form.Item
                    name="confirmPassword"
                    label="Confirm Password"
                    dependencies={['newPassword']}
                    rules={[
                      { required: true, message: 'Please confirm your password' },
                      ({ getFieldValue }) => ({
                        validator(_, value) {
                          if (!value || getFieldValue('newPassword') === value) {
                            return Promise.resolve();
                          }
                          return Promise.reject(new Error('Passwords do not match'));
                        },
                      }),
                    ]}
                  >
                    <Input.Password
                      size="large"
                      placeholder="Confirm new password"
                      prefix={<LockOutlined className="input-icon" />}
                      iconRender={(visible) =>
                        visible ? <EyeTwoTone /> : <EyeInvisibleOutlined />
                      }
                    />
                  </Form.Item>
                </>
              )}

              <Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  size="large"
                  block
                  loading={loading}
                >
                  {loading ? 'Submitting...' : 'Continue'}
                </Button>
              </Form.Item>

              <Button
                type="link"
                block
                onClick={() => {
                  setChallenge(null);
                  setError(null);
                }}
              >
                Back to Login
              </Button>
            </Form>
          </div>
        </div>
      </div>
    );
  }

  // Main login form
  return (
    <div className="login-background">
      <div className="login-container">
        <div className="login-card">
          <div className="logo-section">
            <img src="/logo.png" alt="Vaad HaYishuv Logo" className="logo-image" />
          </div>

          <Title level={4} className="login-title">Welcome back</Title>
          <Text type="secondary" className="login-subtitle">
            Sign in to continue to your dashboard
          </Text>

          {error && (
            <Alert
              message={error}
              type="error"
              showIcon
              className="error-alert"
            />
          )}

          <Form
            form={loginForm}
            layout="vertical"
            onFinish={handleLogin}
            initialValues={{ remember: true }}
            requiredMark={false}
          >
            <Form.Item
              name="email"
              label="Email"
              validateTrigger="onBlur"
              rules={[
                { required: true, message: 'Please enter your email' },
                { type: 'email', message: 'Please enter a valid email' },
              ]}
            >
              <Input
                size="large"
                placeholder="you@example.com"
                prefix={<MailOutlined className="input-icon" />}
              />
            </Form.Item>

            <Form.Item
              name="password"
              label="Password"
              rules={[{ required: true, message: 'Please enter your password' }]}
            >
              <Input.Password
                size="large"
                placeholder="Enter your password"
                prefix={<LockOutlined className="input-icon" />}
                iconRender={(visible) =>
                  visible ? <EyeTwoTone /> : <EyeInvisibleOutlined />
                }
              />
            </Form.Item>

            <Form.Item>
              <div className="form-row">
                <Form.Item name="remember" valuePropName="checked" noStyle>
                  <Checkbox>Remember me</Checkbox>
                </Form.Item>
                <Link onClick={handleForgotPassword}>Forgot password?</Link>
              </div>
            </Form.Item>

            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                size="large"
                block
                loading={loading}
              >
                {loading ? 'Signing in...' : 'Sign in'}
              </Button>
            </Form.Item>
          </Form>

          <Text type="secondary" className="help-text">
            Need help? Call <Link href="tel:9087777101">(908) 777-7101</Link>
          </Text>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
