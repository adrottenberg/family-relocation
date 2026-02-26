import { Button } from 'antd';
import type { ApplicationData } from '../PublicApplicationPage';

interface ReviewStepProps {
  data: ApplicationData;
  onSubmit: () => void;
  onBack: () => void;
  isLoading: boolean;
}

const ReviewStep = ({ data, onSubmit, onBack, isLoading }: ReviewStepProps) => {
  const formatPhone = (phone?: string) => phone || 'Not provided';
  const formatValue = (value?: string | number) => value || 'Not provided';

  const formatBudget = (amount?: number) => {
    if (!amount) return 'Not specified';
    return `$${amount.toLocaleString()}`;
  };

  return (
    <div>
      <h3>Review Your Application</h3>
      <p style={{ color: '#666', marginBottom: 24 }}>
        Please review your information before submitting.
      </p>

      {/* Husband Info */}
      <div className="review-section">
        <h4>Husband Information</h4>
        <div className="review-field">
          <span className="review-label">Name:</span>
          <span className="review-value">
            {data.husband?.firstName} {data.husband?.lastName}
          </span>
        </div>
        {data.husband?.fatherName && (
          <div className="review-field">
            <span className="review-label">Father's Name:</span>
            <span className="review-value">{data.husband.fatherName}</span>
          </div>
        )}
        <div className="review-field">
          <span className="review-label">Email:</span>
          <span className="review-value">{formatValue(data.husband?.email)}</span>
        </div>
        <div className="review-field">
          <span className="review-label">Phone:</span>
          <span className="review-value">{formatPhone(data.husband?.phoneNumbers?.[0]?.number)}</span>
        </div>
      </div>

      {/* Wife Info */}
      {data.wife && (
        <div className="review-section">
          <h4>Wife Information</h4>
          <div className="review-field">
            <span className="review-label">Name:</span>
            <span className="review-value">{data.wife.firstName}</span>
          </div>
          {data.wife.maidenName && (
            <div className="review-field">
              <span className="review-label">Maiden Name:</span>
              <span className="review-value">{data.wife.maidenName}</span>
            </div>
          )}
          <div className="review-field">
            <span className="review-label">Email:</span>
            <span className="review-value">{formatValue(data.wife.email)}</span>
          </div>
          <div className="review-field">
            <span className="review-label">Phone:</span>
            <span className="review-value">{formatPhone(data.wife.phoneNumbers?.[0]?.number)}</span>
          </div>
        </div>
      )}

      {/* Children */}
      {data.children && data.children.length > 0 && (
        <div className="review-section">
          <h4>Children ({data.children.length})</h4>
          <ul className="review-children-list">
            {data.children.map((child, index) => (
              <li key={index}>
                {child.name}, {child.age} years old ({child.gender})
                {child.school && ` - ${child.school}`}
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Address */}
      {data.address && (
        <div className="review-section">
          <h4>Current Address</h4>
          <div className="review-field">
            <span className="review-label">Street:</span>
            <span className="review-value">
              {data.address.street}
              {data.address.street2 && `, ${data.address.street2}`}
            </span>
          </div>
          <div className="review-field">
            <span className="review-label">City/State:</span>
            <span className="review-value">
              {data.address.city}, {data.address.state} {data.address.zipCode}
            </span>
          </div>
          {data.currentKehila && (
            <div className="review-field">
              <span className="review-label">Current Kehila:</span>
              <span className="review-value">{data.currentKehila}</span>
            </div>
          )}
          {data.shabbosShul && (
            <div className="review-field">
              <span className="review-label">Shabbos Shul:</span>
              <span className="review-value">{data.shabbosShul}</span>
            </div>
          )}
        </div>
      )}

      {/* Preferences */}
      {data.housingPreferences && (
        <div className="review-section">
          <h4>Housing Preferences</h4>
          {data.housingPreferences.budgetAmount && (
            <div className="review-field">
              <span className="review-label">Budget:</span>
              <span className="review-value">{formatBudget(data.housingPreferences.budgetAmount)}</span>
            </div>
          )}
          {data.housingPreferences.minBedrooms && (
            <div className="review-field">
              <span className="review-label">Min Bedrooms:</span>
              <span className="review-value">{data.housingPreferences.minBedrooms}</span>
            </div>
          )}
          {data.housingPreferences.minBathrooms && (
            <div className="review-field">
              <span className="review-label">Min Bathrooms:</span>
              <span className="review-value">{data.housingPreferences.minBathrooms}</span>
            </div>
          )}
          {data.housingPreferences.moveTimeline && (
            <div className="review-field">
              <span className="review-label">Move Timeline:</span>
              <span className="review-value">{data.housingPreferences.moveTimeline}</span>
            </div>
          )}
          {data.housingPreferences.requiredFeatures && data.housingPreferences.requiredFeatures.length > 0 && (
            <div className="review-field">
              <span className="review-label">Features:</span>
              <span className="review-value">{data.housingPreferences.requiredFeatures.join(', ')}</span>
            </div>
          )}
        </div>
      )}

      <div className="step-buttons">
        <Button onClick={onBack} disabled={isLoading}>
          Back
        </Button>
        <Button type="primary" onClick={onSubmit} loading={isLoading}>
          Submit Application
        </Button>
      </div>
    </div>
  );
};

export default ReviewStep;
