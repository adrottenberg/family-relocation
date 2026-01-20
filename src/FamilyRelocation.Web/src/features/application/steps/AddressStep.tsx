import { Form, Input, Select, Button } from 'antd';
import type { ApplicationData } from '../PublicApplicationPage';

interface AddressStepProps {
  data: ApplicationData;
  onNext: (data: Partial<ApplicationData>) => void;
  onBack: () => void;
}

const US_STATES = [
  { value: 'AL', label: 'Alabama' },
  { value: 'AK', label: 'Alaska' },
  { value: 'AZ', label: 'Arizona' },
  { value: 'AR', label: 'Arkansas' },
  { value: 'CA', label: 'California' },
  { value: 'CO', label: 'Colorado' },
  { value: 'CT', label: 'Connecticut' },
  { value: 'DE', label: 'Delaware' },
  { value: 'FL', label: 'Florida' },
  { value: 'GA', label: 'Georgia' },
  { value: 'HI', label: 'Hawaii' },
  { value: 'ID', label: 'Idaho' },
  { value: 'IL', label: 'Illinois' },
  { value: 'IN', label: 'Indiana' },
  { value: 'IA', label: 'Iowa' },
  { value: 'KS', label: 'Kansas' },
  { value: 'KY', label: 'Kentucky' },
  { value: 'LA', label: 'Louisiana' },
  { value: 'ME', label: 'Maine' },
  { value: 'MD', label: 'Maryland' },
  { value: 'MA', label: 'Massachusetts' },
  { value: 'MI', label: 'Michigan' },
  { value: 'MN', label: 'Minnesota' },
  { value: 'MS', label: 'Mississippi' },
  { value: 'MO', label: 'Missouri' },
  { value: 'MT', label: 'Montana' },
  { value: 'NE', label: 'Nebraska' },
  { value: 'NV', label: 'Nevada' },
  { value: 'NH', label: 'New Hampshire' },
  { value: 'NJ', label: 'New Jersey' },
  { value: 'NM', label: 'New Mexico' },
  { value: 'NY', label: 'New York' },
  { value: 'NC', label: 'North Carolina' },
  { value: 'ND', label: 'North Dakota' },
  { value: 'OH', label: 'Ohio' },
  { value: 'OK', label: 'Oklahoma' },
  { value: 'OR', label: 'Oregon' },
  { value: 'PA', label: 'Pennsylvania' },
  { value: 'RI', label: 'Rhode Island' },
  { value: 'SC', label: 'South Carolina' },
  { value: 'SD', label: 'South Dakota' },
  { value: 'TN', label: 'Tennessee' },
  { value: 'TX', label: 'Texas' },
  { value: 'UT', label: 'Utah' },
  { value: 'VT', label: 'Vermont' },
  { value: 'VA', label: 'Virginia' },
  { value: 'WA', label: 'Washington' },
  { value: 'WV', label: 'West Virginia' },
  { value: 'WI', label: 'Wisconsin' },
  { value: 'WY', label: 'Wyoming' },
];

const AddressStep = ({ data, onNext, onBack }: AddressStepProps) => {
  const [form] = Form.useForm();

  const handleFinish = (values: {
    street: string;
    street2?: string;
    city: string;
    state: string;
    zipCode: string;
    currentKehila?: string;
    shabbosShul?: string;
  }) => {
    onNext({
      address: {
        street: values.street,
        street2: values.street2,
        city: values.city,
        state: values.state,
        zipCode: values.zipCode,
      },
      currentKehila: values.currentKehila,
      shabbosShul: values.shabbosShul,
    });
  };

  return (
    <Form
      form={form}
      layout="vertical"
      initialValues={{
        street: data.address?.street || '',
        street2: data.address?.street2 || '',
        city: data.address?.city || '',
        state: data.address?.state || '',
        zipCode: data.address?.zipCode || '',
        currentKehila: data.currentKehila || '',
        shabbosShul: data.shabbosShul || '',
      }}
      onFinish={handleFinish}
    >
      <h3>Current Address & Community</h3>

      <Form.Item
        name="street"
        label="Street Address"
        rules={[{ required: true, message: 'Street address is required' }]}
      >
        <Input placeholder="123 Main Street" />
      </Form.Item>

      <Form.Item name="street2" label="Apartment/Unit (optional)">
        <Input placeholder="Apt 4B" />
      </Form.Item>

      <Form.Item
        name="city"
        label="City"
        rules={[{ required: true, message: 'City is required' }]}
      >
        <Input placeholder="City" />
      </Form.Item>

      <Form.Item
        name="state"
        label="State"
        rules={[{ required: true, message: 'State is required' }]}
      >
        <Select
          placeholder="Select state"
          options={US_STATES}
          showSearch
          filterOption={(input, option) =>
            (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
          }
        />
      </Form.Item>

      <Form.Item
        name="zipCode"
        label="ZIP Code"
        rules={[
          { required: true, message: 'ZIP code is required' },
          { pattern: /^\d{5}(-\d{4})?$/, message: 'Please enter a valid ZIP code' },
        ]}
      >
        <Input placeholder="12345" maxLength={10} />
      </Form.Item>

      <Form.Item name="currentKehila" label="Affiliated Kehila">
        <Input placeholder="Enter your affiliated kehila" />
      </Form.Item>

      <Form.Item name="shabbosShul" label="Shabbos Shul">
        <Input placeholder="Where do you daven on Shabbos?" />
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

export default AddressStep;
