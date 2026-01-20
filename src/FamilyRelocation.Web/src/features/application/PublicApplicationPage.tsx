import { useState } from 'react';
import { Steps, Card, message } from 'antd';
import { useMutation } from '@tanstack/react-query';
import { applicantsApi } from '../../api';
import type {
  HusbandInfoDto,
  SpouseInfoDto,
  ChildDto,
  AddressDto,
  HousingPreferencesDto,
} from '../../api/types';
import HusbandInfoStep from './steps/HusbandInfoStep';
import WifeInfoStep from './steps/WifeInfoStep';
import ChildrenStep from './steps/ChildrenStep';
import AddressStep from './steps/AddressStep';
import PreferencesStep from './steps/PreferencesStep';
import ReviewStep from './steps/ReviewStep';
import ApplicationSuccessPage from './ApplicationSuccessPage';
import './PublicApplicationPage.css';

export interface ApplicationData {
  husband?: HusbandInfoDto;
  wife?: SpouseInfoDto;
  children?: ChildDto[];
  address?: AddressDto;
  currentKehila?: string;
  shabbosShul?: string;
  housingPreferences?: HousingPreferencesDto;
}

const PublicApplicationPage = () => {
  const [currentStep, setCurrentStep] = useState(0);
  const [formData, setFormData] = useState<ApplicationData>({});
  const [submitted, setSubmitted] = useState(false);

  const submitMutation = useMutation({
    mutationFn: (data: ApplicationData) =>
      applicantsApi.create({
        husband: data.husband!,
        wife: data.wife,
        children: data.children,
        address: data.address,
        currentKehila: data.currentKehila,
        shabbosShul: data.shabbosShul,
      }),
    onSuccess: () => {
      setSubmitted(true);
    },
    onError: () => {
      message.error('Failed to submit application. Please try again.');
    },
  });

  const handleNext = (stepData: Partial<ApplicationData>) => {
    setFormData((prev) => ({ ...prev, ...stepData }));
    setCurrentStep((prev) => prev + 1);
  };

  const handleBack = () => {
    setCurrentStep((prev) => prev - 1);
  };

  const handleSubmit = () => {
    submitMutation.mutate(formData);
  };

  if (submitted) {
    return <ApplicationSuccessPage />;
  }

  const steps = [
    { title: 'Husband', key: 'husband' },
    { title: 'Wife', key: 'wife' },
    { title: 'Children', key: 'children' },
    { title: 'Address', key: 'address' },
    { title: 'Preferences', key: 'preferences' },
    { title: 'Review', key: 'review' },
  ];

  const renderStepContent = () => {
    switch (currentStep) {
      case 0:
        return <HusbandInfoStep data={formData} onNext={handleNext} />;
      case 1:
        return <WifeInfoStep data={formData} onNext={handleNext} onBack={handleBack} />;
      case 2:
        return <ChildrenStep data={formData} onNext={handleNext} onBack={handleBack} />;
      case 3:
        return <AddressStep data={formData} onNext={handleNext} onBack={handleBack} />;
      case 4:
        return <PreferencesStep data={formData} onNext={handleNext} onBack={handleBack} />;
      case 5:
        return (
          <ReviewStep
            data={formData}
            onSubmit={handleSubmit}
            onBack={handleBack}
            isLoading={submitMutation.isPending}
          />
        );
      default:
        return null;
    }
  };

  return (
    <div className="public-application-page">
      <div className="application-header">
        <h1>Family Relocation Program</h1>
        <p>Application for Union County, NJ Community</p>
      </div>

      <Card className="application-card">
        <Steps
          current={currentStep}
          items={steps.map((s) => ({ title: s.title, key: s.key }))}
          responsive={false}
          size="small"
        />
        <div className="step-content">{renderStepContent()}</div>
      </Card>

      <div className="application-footer">
        <p>Questions? Contact us at vaadhayishuvunion@gmail.com</p>
      </div>
    </div>
  );
};

export default PublicApplicationPage;
