import { Form, Input, Button, Select, Card } from 'antd';
import { PlusOutlined, MinusCircleOutlined } from '@ant-design/icons';
import type { ApplicationData } from '../PublicApplicationPage';

interface ChildrenStepProps {
  data: ApplicationData;
  onNext: (data: Partial<ApplicationData>) => void;
  onBack: () => void;
}

interface ChildFormData {
  name: string;
  age: number;
  gender: string;
  school?: string;
}

const ChildrenStep = ({ data, onNext, onBack }: ChildrenStepProps) => {
  const [form] = Form.useForm();

  const handleFinish = (values: { children?: ChildFormData[] }) => {
    onNext({ children: values.children || [] });
  };

  return (
    <Form
      form={form}
      layout="vertical"
      initialValues={{ children: data.children || [] }}
      onFinish={handleFinish}
    >
      <h3>Children</h3>
      <p style={{ color: '#666', marginBottom: 16 }}>
        Add information about your children. This section is optional.
      </p>

      <Form.List name="children">
        {(fields, { add, remove }) => (
          <>
            {fields.map(({ key, name, ...restField }) => (
              <Card key={key} size="small" className="child-card">
                <div className="child-form-row">
                  <Form.Item
                    {...restField}
                    name={[name, 'name']}
                    label="Name"
                  >
                    <Input placeholder="Child's name" />
                  </Form.Item>

                  <Form.Item
                    {...restField}
                    name={[name, 'age']}
                    label="Age"
                    rules={[{ required: true, message: 'Age is required' }]}
                  >
                    <Input type="number" min={0} max={30} placeholder="Age" style={{ width: 80 }} />
                  </Form.Item>

                  <Form.Item
                    {...restField}
                    name={[name, 'gender']}
                    label="Gender"
                    rules={[{ required: true, message: 'Gender is required' }]}
                  >
                    <Select
                      placeholder="Select"
                      style={{ width: 100 }}
                      options={[
                        { value: 'Male', label: 'Male' },
                        { value: 'Female', label: 'Female' },
                      ]}
                    />
                  </Form.Item>

                  <Form.Item {...restField} name={[name, 'school']} label="School">
                    <Input placeholder="Current school" />
                  </Form.Item>

                  <Button
                    type="text"
                    danger
                    icon={<MinusCircleOutlined />}
                    onClick={() => remove(name)}
                    className="remove-btn"
                  >
                    Remove
                  </Button>
                </div>
              </Card>
            ))}

            <Button type="dashed" onClick={() => add()} block icon={<PlusOutlined />}>
              Add Child
            </Button>
          </>
        )}
      </Form.List>

      <div className="step-buttons">
        <Button onClick={onBack}>Back</Button>
        <Button type="primary" htmlType="submit">
          Next
        </Button>
      </div>
    </Form>
  );
};

export default ChildrenStep;
