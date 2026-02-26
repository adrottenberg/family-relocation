import { Form, Input, Button, Select, Space } from 'antd';
import { PlusOutlined, MinusCircleOutlined } from '@ant-design/icons';
import type { ApplicationData } from '../PublicApplicationPage';

interface WifeInfoStepProps {
  data: ApplicationData;
  onNext: (data: Partial<ApplicationData>) => void;
  onBack: () => void;
}

const PHONE_TYPES = [
  { value: 'Mobile', label: 'Mobile' },
  { value: 'Home', label: 'Home' },
  { value: 'Work', label: 'Work' },
];

const WifeInfoStep = ({ data, onNext, onBack }: WifeInfoStepProps) => {
  const [form] = Form.useForm();

  const handleFinish = (values: {
    firstName: string;
    maidenName: string;
    fatherName?: string;
    email?: string;
    phones?: { number: string; type: string }[];
    highSchool?: string;
  }) => {
    onNext({
      wife: {
        firstName: values.firstName,
        maidenName: values.maidenName,
        fatherName: values.fatherName,
        email: values.email,
        phoneNumbers: values.phones?.filter(p => p?.number)?.map((p, i) => ({
          number: p.number,
          type: p.type || 'Mobile',
          isPrimary: i === 0,
        })),
        highSchool: values.highSchool,
      },
    });
  };

  const initialPhones = data.wife?.phoneNumbers?.length
    ? data.wife.phoneNumbers.map(p => ({ number: p.number, type: p.type }))
    : [{ number: '', type: 'Mobile' }];

  return (
    <Form
      form={form}
      layout="vertical"
      initialValues={{
        firstName: data.wife?.firstName || '',
        maidenName: data.wife?.maidenName || '',
        fatherName: data.wife?.fatherName || '',
        email: data.wife?.email || '',
        phones: initialPhones,
        highSchool: data.wife?.highSchool || '',
      }}
      onFinish={handleFinish}
    >
      <h3>Wife Information</h3>

      <Form.Item
        name="firstName"
        label="First Name"
        rules={[{ required: true, message: 'First name is required' }]}
      >
        <Input placeholder="Enter first name" />
      </Form.Item>

      <Form.Item
        name="maidenName"
        label="Maiden Name"
        rules={[{ required: true, message: 'Maiden name is required' }]}
      >
        <Input placeholder="Enter maiden name" />
      </Form.Item>

      <Form.Item name="fatherName" label="Father's Name">
        <Input placeholder="Enter father's name" />
      </Form.Item>

      <Form.Item
        name="email"
        label="Email"
        validateTrigger="onBlur"
        rules={[{ type: 'email', message: 'Please enter a valid email' }]}
      >
        <Input placeholder="Enter email address" />
      </Form.Item>

      <Form.Item label="Phone Numbers">
        <Form.List name="phones">
          {(fields, { add, remove }) => (
            <>
              {fields.map(({ key, name, ...restField }) => (
                <Space key={key} style={{ display: 'flex', marginBottom: 8 }} align="baseline">
                  <Form.Item
                    {...restField}
                    name={[name, 'number']}
                    style={{ marginBottom: 0, width: 200 }}
                  >
                    <Input placeholder="Phone number" />
                  </Form.Item>
                  <Form.Item
                    {...restField}
                    name={[name, 'type']}
                    style={{ marginBottom: 0 }}
                  >
                    <Select options={PHONE_TYPES} style={{ width: 100 }} />
                  </Form.Item>
                  {fields.length > 1 && (
                    <MinusCircleOutlined onClick={() => remove(name)} />
                  )}
                </Space>
              ))}
              <Button type="dashed" onClick={() => add({ number: '', type: 'Mobile' })} icon={<PlusOutlined />}>
                Add Phone
              </Button>
            </>
          )}
        </Form.List>
      </Form.Item>

      <Form.Item name="highSchool" label="High School">
        <Input placeholder="Enter high school" />
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

export default WifeInfoStep;
