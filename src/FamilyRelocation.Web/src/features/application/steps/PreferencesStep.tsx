import { Form, InputNumber, Select, Button, Checkbox } from 'antd';
import type { ApplicationData } from '../PublicApplicationPage';

interface PreferencesStepProps {
  data: ApplicationData;
  onNext: (data: Partial<ApplicationData>) => void;
  onBack: () => void;
}

const FEATURES = [
  { value: 'Garage', label: 'Garage' },
  { value: 'Finished Basement', label: 'Finished Basement' },
  { value: 'Central Air', label: 'Central Air' },
  { value: 'In-Ground Pool', label: 'In-Ground Pool' },
  { value: 'Large Backyard', label: 'Large Backyard' },
  { value: 'Modern Kitchen', label: 'Modern Kitchen' },
  { value: 'Master Suite', label: 'Master Suite' },
  { value: 'Home Office', label: 'Home Office' },
];

const MOVE_TIMELINES = [
  { value: 'Immediate', label: 'Immediate (within 1 month)' },
  { value: 'ShortTerm', label: 'Short Term (1-3 months)' },
  { value: 'MediumTerm', label: 'Medium Term (3-6 months)' },
  { value: 'LongTerm', label: 'Long Term (6+ months)' },
  { value: 'Flexible', label: 'Flexible' },
];

const SHULS = [
  { value: 'Bobov', label: 'Bobov' },
  { value: 'Yismach Yisroel', label: 'Yismach Yisroel' },
  { value: 'Nassad', label: 'Nassad' },
];

const PreferencesStep = ({ data, onNext, onBack }: PreferencesStepProps) => {
  const [form] = Form.useForm();

  const handleFinish = (values: {
    budgetAmount?: number;
    minBedrooms?: number;
    minBathrooms?: number;
    requiredFeatures?: string[];
    moveTimeline?: string;
    maxWalkingMinutes?: number;
    preferredShuls?: string[];
  }) => {
    onNext({
      housingPreferences: {
        budgetAmount: values.budgetAmount,
        minBedrooms: values.minBedrooms,
        minBathrooms: values.minBathrooms,
        requiredFeatures: values.requiredFeatures,
        moveTimeline: values.moveTimeline,
        shulProximity: values.maxWalkingMinutes || (values.preferredShuls && values.preferredShuls.length > 0)
          ? {
              maxWalkingMinutes: values.maxWalkingMinutes,
              preferredShuls: values.preferredShuls,
            }
          : undefined,
      },
    });
  };

  return (
    <Form
      form={form}
      layout="vertical"
      initialValues={{
        budgetAmount: data.housingPreferences?.budgetAmount,
        minBedrooms: data.housingPreferences?.minBedrooms,
        minBathrooms: data.housingPreferences?.minBathrooms,
        requiredFeatures: data.housingPreferences?.requiredFeatures || [],
        moveTimeline: data.housingPreferences?.moveTimeline,
        maxWalkingMinutes: data.housingPreferences?.shulProximity?.maxWalkingMinutes,
        preferredShuls: data.housingPreferences?.shulProximity?.preferredShuls || [],
      }}
      onFinish={handleFinish}
    >
      <h3>Housing Preferences</h3>
      <p style={{ color: '#666', marginBottom: 16 }}>
        Help us understand what you're looking for. All fields are optional.
      </p>

      <Form.Item name="budgetAmount" label="Budget">
        <InputNumber
          style={{ width: '100%' }}
          formatter={(value) => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
          parser={(value) => Number(value?.replace(/\$\s?|(,*)/g, '') || 0) as unknown as 0}
          placeholder="Enter your budget"
          min={0}
          step={10000}
        />
      </Form.Item>

      <Form.Item name="minBedrooms" label="Minimum Bedrooms">
        <InputNumber min={1} max={10} placeholder="Number of bedrooms" style={{ width: 150 }} />
      </Form.Item>

      <Form.Item name="minBathrooms" label="Minimum Bathrooms">
        <InputNumber min={1} max={10} step={0.5} placeholder="Number of bathrooms" style={{ width: 150 }} />
      </Form.Item>

      <Form.Item name="requiredFeatures" label="Desired Features">
        <Checkbox.Group options={FEATURES} />
      </Form.Item>

      <Form.Item name="moveTimeline" label="Move Timeline">
        <Select placeholder="When are you looking to move?" options={MOVE_TIMELINES} allowClear />
      </Form.Item>

      <Form.Item name="maxWalkingMinutes" label="Maximum Walk to Shul (minutes)">
        <InputNumber min={1} max={60} placeholder="15" style={{ width: 150 }} />
      </Form.Item>

      <Form.Item name="preferredShuls" label="Preferred Shuls">
        <Select
          mode="multiple"
          placeholder="Select preferred shuls"
          options={SHULS}
          allowClear
        />
      </Form.Item>

      <div className="step-buttons">
        <Button onClick={onBack}>Back</Button>
        <Button type="primary" htmlType="submit">
          Next
        </Button>
      </div>
    </Form>
  );
};

export default PreferencesStep;
