import { Form, Input, Button, Select, Space } from 'antd';
import { PlusOutlined, MinusCircleOutlined } from '@ant-design/icons';
import type { ApplicationData } from '../PublicApplicationPage';

interface HusbandInfoStepProps {
  data: ApplicationData;
  onNext: (data: Partial<ApplicationData>) => void;
}

const PHONE_TYPES = [
  { value: 'Mobile', label: 'Mobile' },
  { value: 'Home', label: 'Home' },
  { value: 'Work', label: 'Work' },
];

const HusbandInfoStep = ({ data, onNext }: HusbandInfoStepProps) => {
  const [form] = Form.useForm();

  const handleFinish = (values: {
    firstName: string;
    lastName: string;
    fatherName?: string;
    email?: string;
    phones?: { number: string; type: string }[];
  }) => {
    onNext({
      husband: {
        firstName: values.firstName,
        lastName: values.lastName,
        fatherName: values.fatherName,
        email: values.email,
        phoneNumbers: values.phones?.filter(p => p?.number)?.map((p, i) => ({
          number: p.number,
          type: p.type || 'Mobile',
          isPrimary: i === 0,
        })),
      },
    });
  };

  const initialPhones = data.husband?.phoneNumbers?.length
    ? data.husband.phoneNumbers.map(p => ({ number: p.number, type: p.type }))
    : [{ number: '', type: 'Mobile' }];

  return (
    <Form
      form={form}
      layout="vertical"
      initialValues={{
        firstName: data.husband?.firstName || '',
        lastName: data.husband?.lastName || '',
        fatherName: data.husband?.fatherName || '',
        email: data.husband?.email || '',
        phones: initialPhones,
      }}
      onFinish={handleFinish}
    >
      <h3>Husband Information</h3>

      <Form.Item
        name="firstName"
        label="First Name"
        rules={[{ required: true, message: 'First name is required' }]}
      >
        <Input placeholder="Enter first name" />
      </Form.Item>

      <Form.Item
        name="lastName"
        label="Last Name"
        rules={[{ required: true, message: 'Last name is required' }]}
      >
        <Input placeholder="Enter last name" />
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

      <div className="step-buttons-right">
        <Button type="primary" htmlType="submit">
          Next
        </Button>
      </div>
    </Form>
  );
};

export default HusbandInfoStep;
