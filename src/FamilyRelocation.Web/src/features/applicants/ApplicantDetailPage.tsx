import { Typography } from 'antd';
import { useParams } from 'react-router-dom';

const { Title } = Typography;

/**
 * Applicant detail page - to be implemented in UV-26
 */
const ApplicantDetailPage = () => {
  const { id } = useParams<{ id: string }>();

  return (
    <div>
      <Title level={2}>Applicant Detail</Title>
      <p>Applicant ID: {id}</p>
      <p>Detail page - Coming in UV-26</p>
    </div>
  );
};

export default ApplicantDetailPage;
