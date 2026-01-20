import { Result, Button } from 'antd';
import { CheckCircleFilled } from '@ant-design/icons';
import './PublicApplicationPage.css';

const ApplicationSuccessPage = () => {
  return (
    <div className="public-application-page">
      <div className="application-header">
        <h1>Family Relocation Program</h1>
        <p>Union County, NJ Community</p>
      </div>

      <div className="success-card">
        <Result
          icon={<CheckCircleFilled style={{ color: '#52c41a' }} />}
          title="Application Submitted!"
          subTitle="Thank you for your interest in our community."
          extra={
            <div className="success-content">
              <h3>What happens next:</h3>
              <ol>
                <li>Our team will review your application</li>
                <li>The board will discuss at their next meeting</li>
                <li>You'll receive an email with the decision</li>
              </ol>
              <p className="contact-info">
                Questions? Contact us at <a href="mailto:vaadhayishuvunion@gmail.com">vaadhayishuvunion@gmail.com</a>
              </p>
              <Button type="primary" href="/" size="large">
                Return to Home
              </Button>
            </div>
          }
        />
      </div>
    </div>
  );
};

export default ApplicationSuccessPage;
