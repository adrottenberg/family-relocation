import { Drawer, Form, Input, Select, Button, Space, Collapse, message, InputNumber, Checkbox } from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { applicantsApi, shulsApi } from '../../api';
import type { ApplicantDto, PhoneNumberDto, ChildDto } from '../../api/types';
import { useEffect } from 'react';

const { Panel } = Collapse;

interface EditApplicantDrawerProps {
  open: boolean;
  onClose: () => void;
  applicant: ApplicantDto;
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

const US_STATES = [
  'AL', 'AK', 'AZ', 'AR', 'CA', 'CO', 'CT', 'DE', 'FL', 'GA',
  'HI', 'ID', 'IL', 'IN', 'IA', 'KS', 'KY', 'LA', 'ME', 'MD',
  'MA', 'MI', 'MN', 'MS', 'MO', 'MT', 'NE', 'NV', 'NH', 'NJ',
  'NM', 'NY', 'NC', 'ND', 'OH', 'OK', 'OR', 'PA', 'RI', 'SC',
  'SD', 'TN', 'TX', 'UT', 'VT', 'VA', 'WA', 'WV', 'WI', 'WY',
].map(s => ({ value: s, label: s }));

const EditApplicantDrawer = ({ open, onClose, applicant }: EditApplicantDrawerProps) => {
  const [form] = Form.useForm<FormValues>();
  const queryClient = useQueryClient();

  // Fetch shuls from API
  const { data: shulsData, isLoading: shulsLoading } = useQuery({
    queryKey: ['shuls'],
    queryFn: () => shulsApi.getAll({ pageSize: 100 }),
  });

  const shulOptions = shulsData?.items.map(shul => ({
    value: shul.id,
    label: shul.name,
  })) || [];

  // Populate form with existing data when opened
  useEffect(() => {
    if (open && applicant) {
      form.setFieldsValue({
        husband: {
          firstName: applicant.husband.firstName,
          lastName: applicant.husband.lastName,
          fatherName: applicant.husband.fatherName,
          email: applicant.husband.email,
          phoneNumbers: applicant.husband.phoneNumbers?.map(p => ({
            number: p.number,
            type: p.type,
            isPrimary: p.isPrimary,
          })) || [{ number: '', type: 'Mobile', isPrimary: true }],
          occupation: applicant.husband.occupation,
          employerName: applicant.husband.employerName,
        },
        wife: applicant.wife ? {
          firstName: applicant.wife.firstName,
          maidenName: applicant.wife.maidenName,
          fatherName: applicant.wife.fatherName,
          email: applicant.wife.email,
          phoneNumbers: applicant.wife.phoneNumbers?.map(p => ({
            number: p.number,
            type: p.type,
            isPrimary: p.isPrimary,
          })) || [],
          occupation: applicant.wife.occupation,
          employerName: applicant.wife.employerName,
          highSchool: applicant.wife.highSchool,
        } : undefined,
        address: applicant.address ? {
          street: applicant.address.street,
          street2: applicant.address.street2,
          city: applicant.address.city,
          state: applicant.address.state,
          zipCode: applicant.address.zipCode,
        } : undefined,
        children: applicant.children || [],
        currentKehila: applicant.currentKehila,
        shabbosShul: applicant.shabbosShul,
      });
    }
  }, [open, applicant, form]);

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

      return applicantsApi.update(applicant.id, {
        id: applicant.id,
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
      } as ApplicantDto);
    },
    onSuccess: () => {
      message.success('Applicant updated successfully');
      queryClient.invalidateQueries({ queryKey: ['applicant', applicant.id] });
      queryClient.invalidateQueries({ queryKey: ['applicants'] });
      onClose();
    },
    onError: () => {
      message.error('Failed to update applicant');
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
      title={`Edit: ${applicant.husband.lastName} Family`}
      placement="right"
      width={600}
      open={open}
      onClose={handleCancel}
      extra={
        <Space>
          <Button onClick={handleCancel}>Cancel</Button>
          <Button type="primary" onClick={handleSubmit} loading={mutation.isPending}>
            Save Changes
          </Button>
        </Space>
      }
    >
      <Form form={form} layout="vertical">
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
                options={shulOptions}
                loading={shulsLoading}
                placeholder="Select shuls"
              />
            </Form.Item>
          </Panel>
        </Collapse>
      </Form>
    </Drawer>
  );
};

export default EditApplicantDrawer;
