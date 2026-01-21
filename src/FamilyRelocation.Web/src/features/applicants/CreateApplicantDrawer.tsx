import { Drawer, Form, Input, Select, Button, Space, Collapse, message, InputNumber, Checkbox } from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { applicantsApi } from '../../api';
import type { PhoneNumberDto, ChildDto } from '../../api/types';

const { Panel } = Collapse;

interface CreateApplicantDrawerProps {
  open: boolean;
  onClose: () => void;
}

interface FormValues {
  husband: {
    firstName: string;
    lastName: string;
    fatherName?: string;
    email?: string;
    phoneNumbers?: { number: string; type: string; isPrimary: boolean }[];
    occupation?: string;
    employerName?: string;
  };
  wife?: {
    firstName: string;
    maidenName?: string;
    fatherName?: string;
    email?: string;
    phoneNumbers?: { number: string; type: string; isPrimary: boolean }[];
    occupation?: string;
    employerName?: string;
    highSchool?: string;
  };
  address?: {
    street: string;
    street2?: string;
    city: string;
    state: string;
    zipCode: string;
  };
  children?: ChildDto[];
  currentKehila?: string;
  shabbosShul?: string;
}

const PHONE_TYPES = [
  { value: 'Mobile', label: 'Mobile' },
  { value: 'Home', label: 'Home' },
  { value: 'Work', label: 'Work' },
];

const GENDERS = [
  { value: 'Male', label: 'Male' },
  { value: 'Female', label: 'Female' },
];

const SHULS = [
  { value: 'Bobov', label: 'Bobov' },
  { value: 'Yismach Yisroel', label: 'Yismach Yisroel' },
  { value: 'Nassad', label: 'Nassad' },
];

const US_STATES = [
  'AL', 'AK', 'AZ', 'AR', 'CA', 'CO', 'CT', 'DE', 'FL', 'GA',
  'HI', 'ID', 'IL', 'IN', 'IA', 'KS', 'KY', 'LA', 'ME', 'MD',
  'MA', 'MI', 'MN', 'MS', 'MO', 'MT', 'NE', 'NV', 'NH', 'NJ',
  'NM', 'NY', 'NC', 'ND', 'OH', 'OK', 'OR', 'PA', 'RI', 'SC',
  'SD', 'TN', 'TX', 'UT', 'VT', 'VA', 'WA', 'WV', 'WI', 'WY',
].map(s => ({ value: s, label: s }));

const CreateApplicantDrawer = ({ open, onClose }: CreateApplicantDrawerProps) => {
  const [form] = Form.useForm<FormValues>();
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const mutation = useMutation({
    mutationFn: (values: FormValues) => {
      // Clean up phone numbers - filter out empty ones and set first as primary if none
      const cleanPhones = (phones?: PhoneNumberDto[]): PhoneNumberDto[] | undefined => {
        if (!phones) return undefined;
        const filtered = phones.filter(p => p.number?.trim());
        if (filtered.length === 0) return undefined;
        if (!filtered.some(p => p.isPrimary)) {
          filtered[0].isPrimary = true;
        }
        return filtered;
      };

      return applicantsApi.create({
        husband: {
          ...values.husband,
          phoneNumbers: cleanPhones(values.husband.phoneNumbers as PhoneNumberDto[]),
        },
        wife: values.wife?.firstName ? {
          ...values.wife,
          phoneNumbers: cleanPhones(values.wife.phoneNumbers as PhoneNumberDto[]),
        } : undefined,
        address: values.address?.street ? values.address : undefined,
        children: values.children?.filter(c => c.age !== undefined),
        currentKehila: values.currentKehila,
        shabbosShul: values.shabbosShul,
      });
    },
    onSuccess: (data) => {
      message.success('Applicant created successfully');
      queryClient.invalidateQueries({ queryKey: ['applicants'] });
      form.resetFields();
      onClose();
      // Navigate to the new applicant's detail page
      if (data?.applicantId) {
        navigate(`/applicants/${data.applicantId}`);
      }
    },
    onError: () => {
      message.error('Failed to create applicant');
    },
  });

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      mutation.mutate(values);
    } catch {
      // Validation failed
    }
  };

  const handleCancel = () => {
    form.resetFields();
    onClose();
  };

  return (
    <Drawer
      title="Add New Applicant"
      placement="right"
      width={600}
      open={open}
      onClose={handleCancel}
      extra={
        <Space>
          <Button onClick={handleCancel}>Cancel</Button>
          <Button type="primary" onClick={handleSubmit} loading={mutation.isPending}>
            Create Applicant
          </Button>
        </Space>
      }
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={{
          husband: {
            phoneNumbers: [{ type: 'Mobile', number: '', isPrimary: true }],
          },
        }}
      >
        <Collapse defaultActiveKey={['husband', 'wife']} ghost>
          {/* Husband Section */}
          <Panel header="Husband Information" key="husband">
            <Form.Item
              name={['husband', 'firstName']}
              label="First Name"
              rules={[{ required: true, message: 'First name is required' }]}
            >
              <Input />
            </Form.Item>

            <Form.Item
              name={['husband', 'lastName']}
              label="Last Name"
              rules={[{ required: true, message: 'Last name is required' }]}
            >
              <Input />
            </Form.Item>

            <Form.Item name={['husband', 'fatherName']} label="Father's Name">
              <Input />
            </Form.Item>

            <Form.Item
              name={['husband', 'email']}
              label="Email"
              rules={[{ type: 'email', message: 'Please enter a valid email' }]}
              validateTrigger="onBlur"
            >
              <Input />
            </Form.Item>

            {/* Phone Numbers */}
            <Form.List name={['husband', 'phoneNumbers']}>
              {(fields, { add, remove }) => (
                <>
                  <label style={{ display: 'block', marginBottom: 8 }}>Phone Numbers</label>
                  {fields.map(({ key, name, ...restField }) => (
                    <Space key={key} style={{ display: 'flex', marginBottom: 8 }} align="baseline">
                      <Form.Item {...restField} name={[name, 'type']} noStyle>
                        <Select style={{ width: 100 }} options={PHONE_TYPES} />
                      </Form.Item>
                      <Form.Item {...restField} name={[name, 'number']} noStyle>
                        <Input placeholder="Phone number" style={{ width: 180 }} />
                      </Form.Item>
                      <Form.Item {...restField} name={[name, 'isPrimary']} valuePropName="checked" noStyle>
                        <Checkbox>Primary</Checkbox>
                      </Form.Item>
                      {fields.length > 1 && (
                        <DeleteOutlined onClick={() => remove(name)} style={{ color: '#ff4d4f' }} />
                      )}
                    </Space>
                  ))}
                  <Button type="dashed" onClick={() => add({ type: 'Mobile', number: '', isPrimary: false })} icon={<PlusOutlined />}>
                    Add Phone
                  </Button>
                </>
              )}
            </Form.List>

            <Form.Item name={['husband', 'occupation']} label="Occupation" style={{ marginTop: 16 }}>
              <Input />
            </Form.Item>

            <Form.Item name={['husband', 'employerName']} label="Employer">
              <Input />
            </Form.Item>
          </Panel>

          {/* Wife Section */}
          <Panel header="Wife Information" key="wife">
            <Form.Item
              name={['wife', 'firstName']}
              label="First Name"
              rules={[{ required: true, message: 'First name is required' }]}
            >
              <Input />
            </Form.Item>

            <Form.Item
              name={['wife', 'maidenName']}
              label="Maiden Name"
              rules={[{ required: true, message: 'Maiden name is required' }]}
            >
              <Input />
            </Form.Item>

            <Form.Item name={['wife', 'fatherName']} label="Father's Name">
              <Input />
            </Form.Item>

            <Form.Item
              name={['wife', 'email']}
              label="Email"
              rules={[{ type: 'email', message: 'Please enter a valid email' }]}
              validateTrigger="onBlur"
            >
              <Input />
            </Form.Item>

            {/* Phone Numbers */}
            <Form.List name={['wife', 'phoneNumbers']}>
              {(fields, { add, remove }) => (
                <>
                  <label style={{ display: 'block', marginBottom: 8 }}>Phone Numbers</label>
                  {fields.map(({ key, name, ...restField }) => (
                    <Space key={key} style={{ display: 'flex', marginBottom: 8 }} align="baseline">
                      <Form.Item {...restField} name={[name, 'type']} noStyle>
                        <Select style={{ width: 100 }} options={PHONE_TYPES} />
                      </Form.Item>
                      <Form.Item {...restField} name={[name, 'number']} noStyle>
                        <Input placeholder="Phone number" style={{ width: 180 }} />
                      </Form.Item>
                      <Form.Item {...restField} name={[name, 'isPrimary']} valuePropName="checked" noStyle>
                        <Checkbox>Primary</Checkbox>
                      </Form.Item>
                      {fields.length > 1 && (
                        <DeleteOutlined onClick={() => remove(name)} style={{ color: '#ff4d4f' }} />
                      )}
                    </Space>
                  ))}
                  <Button type="dashed" onClick={() => add({ type: 'Mobile', number: '', isPrimary: false })} icon={<PlusOutlined />}>
                    Add Phone
                  </Button>
                </>
              )}
            </Form.List>

            <Form.Item name={['wife', 'occupation']} label="Occupation" style={{ marginTop: 16 }}>
              <Input />
            </Form.Item>

            <Form.Item name={['wife', 'employerName']} label="Employer">
              <Input />
            </Form.Item>

            <Form.Item name={['wife', 'highSchool']} label="High School">
              <Input />
            </Form.Item>
          </Panel>

          {/* Address Section */}
          <Panel header="Current Address" key="address">
            <Form.Item
              name={['address', 'street']}
              label="Street Address"
              rules={[{ required: true, message: 'Street address is required' }]}
            >
              <Input />
            </Form.Item>

            <Form.Item name={['address', 'street2']} label="Apt/Suite">
              <Input />
            </Form.Item>

            <Space style={{ width: '100%' }}>
              <Form.Item
                name={['address', 'city']}
                label="City"
                style={{ flex: 2 }}
                rules={[{ required: true, message: 'City is required' }]}
              >
                <Input />
              </Form.Item>

              <Form.Item
                name={['address', 'state']}
                label="State"
                style={{ flex: 1 }}
                rules={[{ required: true, message: 'State is required' }]}
              >
                <Select options={US_STATES} showSearch />
              </Form.Item>

              <Form.Item
                name={['address', 'zipCode']}
                label="ZIP Code"
                style={{ flex: 1 }}
                rules={[{ required: true, message: 'ZIP code is required' }]}
              >
                <Input />
              </Form.Item>
            </Space>
          </Panel>

          {/* Children Section */}
          <Panel header="Children" key="children">
            <Form.List name="children">
              {(fields, { add, remove }) => (
                <>
                  {fields.map(({ key, name, ...restField }) => (
                    <div
                      key={key}
                      style={{
                        border: '1px solid #d9d9d9',
                        borderRadius: 8,
                        padding: 12,
                        marginBottom: 12,
                        position: 'relative',
                      }}
                    >
                      <DeleteOutlined
                        onClick={() => remove(name)}
                        style={{
                          color: '#ff4d4f',
                          position: 'absolute',
                          top: 12,
                          right: 12,
                          cursor: 'pointer',
                        }}
                      />
                      <Space wrap style={{ width: '100%' }}>
                        <Form.Item {...restField} name={[name, 'name']} label="Name">
                          <Input placeholder="Name (optional)" style={{ width: 150 }} />
                        </Form.Item>
                        <Form.Item
                          {...restField}
                          name={[name, 'age']}
                          label="Age"
                          rules={[{ required: true, message: 'Age required' }]}
                        >
                          <InputNumber min={0} max={30} style={{ width: 80 }} />
                        </Form.Item>
                        <Form.Item
                          {...restField}
                          name={[name, 'gender']}
                          label="Gender"
                          rules={[{ required: true, message: 'Gender required' }]}
                        >
                          <Select options={GENDERS} style={{ width: 100 }} />
                        </Form.Item>
                        <Form.Item {...restField} name={[name, 'school']} label="School">
                          <Input placeholder="School (optional)" style={{ width: 150 }} />
                        </Form.Item>
                      </Space>
                    </div>
                  ))}
                  <Button
                    type="dashed"
                    onClick={() => add({ name: '', age: undefined, gender: undefined, school: '' })}
                    icon={<PlusOutlined />}
                    block
                  >
                    Add Child
                  </Button>
                </>
              )}
            </Form.List>
          </Panel>

          {/* Community Section */}
          <Panel header="Community" key="community">
            <Form.Item name="currentKehila" label="Affiliated Kehila">
              <Input />
            </Form.Item>

            <Form.Item name="shabbosShul" label="Preferred Shul">
              <Select
                mode="multiple"
                options={SHULS}
                placeholder="Select shuls"
              />
            </Form.Item>
          </Panel>
        </Collapse>
      </Form>
    </Drawer>
  );
};

export default CreateApplicantDrawer;
