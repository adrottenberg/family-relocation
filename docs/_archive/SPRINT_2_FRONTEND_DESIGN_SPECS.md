# SPRINT 2 FRONTEND STORIES - DESIGN SPECIFICATIONS

## Design System Reference
- **Design System HTML**: `crm-design-system-v4.html`
- **Theme Config**: `antd-theme.ts`
- **Login Prototype**: `prototype-login-page.html`
- **Pipeline Prototype**: `prototype-pipeline-kanban.html`

---

## 🎨 DESIGN SYSTEM SUMMARY

### Color Strategy
| Role | Color | Hex | Usage |
|------|-------|-----|-------|
| **Primary** | Light Blue | `#d0e4fc` bg / `#1e40af` text | Buttons, focus states, active items |
| **Brand** | Green | `#3d9a4a` | Logo, success states, brand accents |
| **Neutral** | Gray scale | `#1a1d1a` - `#f8f9f8` | Text, backgrounds, borders |

### Typography
- **Font Family**: `'Assistant', 'Heebo', sans-serif` (supports Hebrew)
- **Base Size**: 14px
- **Headings**: 30px / 24px / 20px / 18px / 16px

### Button Style (Option B - Light)
```css
.btn-primary {
    background: #d0e4fc;
    color: #1e40af;
    border: 1px solid #bfdbfe;
}
.btn-primary:hover {
    background: #dbeafe;
}
```

---

## US-F01: React Project Setup (3 points)

### Story
**As a** developer  
**I want to** set up the React project with proper tooling and design system  
**So that** I have a solid foundation matching our design specifications

### Design Requirements

#### 1. Install Dependencies
```bash
npm create vite@latest family-relocation-web -- --template react-ts
cd family-relocation-web

# Core dependencies
npm install antd @ant-design/icons
npm install react-router-dom
npm install @tanstack/react-query @tanstack/react-query-devtools
npm install zustand
npm install axios
npm install dayjs

# Dev dependencies
npm install -D @types/node
```

#### 2. Configure Ant Design Theme
Copy `antd-theme.ts` to `src/theme/antd-theme.ts` and use in `main.tsx`:

```tsx
// src/main.tsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import { ConfigProvider } from 'antd';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { theme, cssVariables } from './theme/antd-theme';
import App from './App';
import './index.css';

// Inject CSS variables for custom components
const style = document.createElement('style');
style.textContent = cssVariables;
document.head.appendChild(style);

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <ConfigProvider theme={theme}>
        <App />
      </ConfigProvider>
    </QueryClientProvider>
  </React.StrictMode>
);
```

#### 3. Add Google Fonts
In `index.html`:
```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Assistant:wght@400;500;600;700&family=Heebo:wght@400;500;600;700&display=swap" rel="stylesheet">
```

#### 4. Global Styles (`src/index.css`)
```css
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: var(--font-family);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: var(--neutral-100);
}

::-webkit-scrollbar-thumb {
  background: var(--neutral-300);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: var(--neutral-400);
}
```

#### 5. Folder Structure
```
src/
├── api/
│   ├── client.ts
│   ├── endpoints/
│   │   ├── applicants.ts
│   │   ├── housingSearches.ts
│   │   └── auth.ts
│   └── types/
├── components/
│   ├── common/
│   │   ├── StatusTag.tsx        # Board decision tags
│   │   ├── StageTag.tsx         # Pipeline stage tags
│   │   ├── LoadingSpinner.tsx
│   │   └── PageHeader.tsx
│   └── layout/
│       ├── AppLayout.tsx
│       ├── Sidebar.tsx
│       └── Header.tsx
├── features/
│   ├── auth/
│   ├── applicants/
│   └── pipeline/
├── hooks/
├── store/
├── theme/
│   └── antd-theme.ts            # From design system
├── utils/
├── App.tsx
├── main.tsx
└── routes.tsx
```

### Definition of Done
- [ ] Vite project created with TypeScript
- [ ] Ant Design configured with custom theme from `antd-theme.ts`
- [ ] Google Fonts (Assistant, Heebo) loading
- [ ] CSS variables injected for custom components
- [ ] Folder structure established
- [ ] `npm run dev` starts without errors
- [ ] Primary button renders with light blue style

---

## US-F02: Authentication Flow - Login Page (5 points)

### Story
**As a** coordinator  
**I want to** log in to the system  
**So that** I can access the CRM features

### Design Requirements

**Reference**: `prototype-login-page.html`

#### Layout Specifications
| Element | Specification |
|---------|---------------|
| Container | Centered, max-width 420px |
| Background | Gradient: `brand-50` to `primary-50` |
| Card | White, border-radius 16px, shadow-lg, padding 40px |
| Logo | Tree image (40-64px height) + "וועד הישוב" text |

#### Component Structure
```tsx
// src/features/auth/LoginPage.tsx
<div className="login-container">
  <Card className="login-card">
    {/* Logo Section */}
    <div className="logo-section">
      <img src="/logo-tree.png" alt="Logo" />
      <h1>וועד הישוב</h1>
      <p>Family Relocation CRM</p>
    </div>
    
    {/* Welcome Text */}
    <h2>Welcome back</h2>
    <p>Sign in to continue to your dashboard</p>
    
    {/* Error Alert (conditional) */}
    <Alert type="error" message={error} />
    
    {/* Login Form */}
    <Form onFinish={handleLogin}>
      <Form.Item name="email" rules={[{ required: true, type: 'email' }]}>
        <Input placeholder="you@example.com" size="large" />
      </Form.Item>
      
      <Form.Item name="password" rules={[{ required: true }]}>
        <Input.Password placeholder="Enter your password" size="large" />
      </Form.Item>
      
      <div className="form-row">
        <Checkbox>Remember me</Checkbox>
        <a href="#">Forgot password?</a>
      </div>
      
      <Button type="primary" htmlType="submit" block size="large" loading={isLoading}>
        Sign in
      </Button>
    </Form>
    
    {/* Help Text */}
    <p>Need help? <a href="#">Contact support</a></p>
  </Card>
</div>
```

#### Styling
```css
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--brand-50) 0%, var(--primary-50) 100%);
  padding: 20px;
}

.login-card {
  width: 100%;
  max-width: 420px;
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
}

.logo-section {
  text-align: center;
  margin-bottom: 32px;
}

.logo-section img {
  height: 64px;
  margin-bottom: 16px;
}

.logo-section h1 {
  font-size: 24px;
  font-weight: 700;
  color: var(--brand-600);
}
```

#### States to Implement
1. **Default** - Form ready for input
2. **Loading** - Button shows spinner, inputs disabled
3. **Error** - Red alert shown above form, inputs highlighted
4. **Success** - Redirect to `/dashboard`

#### Auth Store (Zustand)
```tsx
// src/store/authStore.ts
interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  refreshAccessToken: () => Promise<void>;
  clearError: () => void;
}
```

### Definition of Done
- [ ] Login page matches prototype design
- [ ] Logo with Hebrew text displays correctly
- [ ] Form validation works (email format, required fields)
- [ ] Loading state shows spinner in button
- [ ] Error state shows alert with message
- [ ] Successful login stores tokens and redirects
- [ ] Protected routes redirect to login if not authenticated

---

## US-F03: App Shell & Navigation (3 points)

### Story
**As a** coordinator  
**I want to** navigate between different sections of the CRM  
**So that** I can access all features easily

### Design Requirements

**Reference**: `prototype-pipeline-kanban.html` (sidebar section)

#### Layout Structure
```
┌─────────────────────────────────────────────────────────┐
│ [Sidebar - 220px]  │  [Header - 60px]                   │
│                    │────────────────────────────────────│
│  Logo + וועד הישוב │                                    │
│  ───────────────   │  [Main Content Area]               │
│  📊 Dashboard      │                                    │
│  👥 Applicants     │                                    │
│  📋 Pipeline       │                                    │
│  🏠 Properties     │                                    │
│  📝 Activity       │                                    │
│  ⚙️ Settings       │                                    │
│  ───────────────   │                                    │
│  [User Avatar]     │                                    │
│  Yosef Klein       │                                    │
│  Coordinator       │                                    │
└─────────────────────────────────────────────────────────┘
```

#### Sidebar Specifications
| Element | Specification |
|---------|---------------|
| Width | 220px fixed |
| Background | White |
| Border | 1px solid `neutral-200` on right |
| Logo area | Padding 16px, border-bottom |
| Nav items | Padding 12px 14px, border-radius 8px |
| Active item | Background `primary-50`, color `primary-700` |
| Hover item | Background `neutral-100`, color `neutral-900` |
| User section | Bottom, border-top, padding 16px |

#### Sidebar Component
```tsx
// src/components/layout/Sidebar.tsx
const menuItems = [
  { key: 'dashboard', icon: '📊', label: 'Dashboard', path: '/' },
  { key: 'applicants', icon: '👥', label: 'Applicants', path: '/applicants' },
  { key: 'pipeline', icon: '📋', label: 'Pipeline', path: '/pipeline' },
  { key: 'properties', icon: '🏠', label: 'Properties', path: '/properties' },
  { key: 'activity', icon: '📝', label: 'Activity', path: '/activity' },
  { key: 'settings', icon: '⚙️', label: 'Settings', path: '/settings' },
];

<aside className="sidebar">
  <div className="sidebar-logo">
    <img src="/logo-tree.png" alt="" />
    <span>וועד הישוב</span>
  </div>
  
  <nav className="sidebar-nav">
    {menuItems.map(item => (
      <NavLink 
        key={item.key}
        to={item.path}
        className={({ isActive }) => `sidebar-item ${isActive ? 'active' : ''}`}
      >
        <span>{item.icon}</span>
        {item.label}
      </NavLink>
    ))}
  </nav>
  
  <div className="sidebar-footer">
    <Avatar>{user.initials}</Avatar>
    <div>
      <div className="user-name">{user.name}</div>
      <div className="user-role">{user.role}</div>
    </div>
  </div>
</aside>
```

#### Header Specifications
| Element | Specification |
|---------|---------------|
| Height | 60px |
| Background | White |
| Border | 1px solid `neutral-200` on bottom |
| Position | Sticky top |
| Left side | Page title (20px, bold) |
| Center/Left | Search input (280px, `neutral-100` bg) |
| Right side | User avatar, notifications |

#### Header Component
```tsx
// src/components/layout/Header.tsx
<header className="header">
  <div className="header-left">
    <h1 className="page-title">{title}</h1>
    <div className="header-search">
      <SearchOutlined />
      <input placeholder="Search..." />
    </div>
  </div>
  
  <div className="header-right">
    <Badge count={notifications}>
      <BellOutlined />
    </Badge>
    <Avatar>{user.initials}</Avatar>
  </div>
</header>
```

### Definition of Done
- [ ] Sidebar renders with logo and Hebrew text
- [ ] Navigation items highlight when active (light blue)
- [ ] User info displays at bottom of sidebar
- [ ] Header shows page title and search
- [ ] Layout is responsive (optional: collapsible sidebar on mobile)
- [ ] Routes work: /, /applicants, /pipeline

---

## US-F04: Applicant List Page (3 points)

### Story
**As a** coordinator  
**I want to** see a list of all applicants  
**So that** I can manage family applications

### Design Requirements

#### Page Layout
```
┌──────────────────────────────────────────────────────────┐
│ Applicants                           [+ Add Applicant]   │
│ 47 total • 8 pending review                              │
├──────────────────────────────────────────────────────────┤
│ [🔍 Search by name...]  [All Decisions ▼] [All Stages ▼] │
├──────────────────────────────────────────────────────────┤
│ Family    │ Email          │ Phone    │ Board  │ Stage   │
│───────────┼────────────────┼──────────┼────────┼─────────│
│ Cohen     │ moshe@...      │ (908)... │ ✓ Appr │ Hunting │
│ Levy      │ david@...      │ (718)... │ ○ Pend │ Submit  │
└──────────────────────────────────────────────────────────┘
```

#### Table Specifications
| Element | Specification |
|---------|---------------|
| Header bg | `neutral-50` |
| Header text | 12px, uppercase, `neutral-500` |
| Row hover | `primary-50` background |
| Row padding | 16px |
| Border | 1px solid `neutral-100` between rows |

#### Status/Stage Tags
Use custom Tag components with design system colors:

```tsx
// src/components/common/StatusTag.tsx
import { Tag } from 'antd';
import { statusTagStyles } from '@/theme/antd-theme';

type BoardDecision = 'Pending' | 'Approved' | 'Rejected' | 'Deferred';

export const StatusTag = ({ status }: { status: BoardDecision }) => {
  const styleMap = {
    Pending: statusTagStyles.pending,
    Approved: statusTagStyles.approved,
    Rejected: statusTagStyles.rejected,
    Deferred: statusTagStyles.deferred,
  };
  
  return <Tag style={styleMap[status]}>{status}</Tag>;
};

// src/components/common/StageTag.tsx
import { Tag } from 'antd';
import { stageTagStyles } from '@/theme/antd-theme';

type HousingSearchStage = 'Submitted' | 'HouseHunting' | 'UnderContract' | 'Closed';

export const StageTag = ({ stage }: { stage: HousingSearchStage }) => {
  const labels = {
    Submitted: 'Submitted',
    HouseHunting: 'House Hunting',
    UnderContract: 'Under Contract',
    Closed: 'Closed',
  };
  
  const styleMap = {
    Submitted: stageTagStyles.submitted,
    HouseHunting: stageTagStyles.houseHunting,
    UnderContract: stageTagStyles.underContract,
    Closed: stageTagStyles.closed,
  };
  
  return <Tag style={styleMap[stage]}>{labels[stage]}</Tag>;
};
```

#### Table Columns
```tsx
const columns = [
  {
    title: 'Family',
    dataIndex: 'familyName',
    render: (_, record) => (
      <div>
        <div style={{ fontWeight: 600 }}>{record.familyName}</div>
        <div style={{ fontSize: 12, color: 'var(--neutral-500)' }}>
          {record.husbandFirstName} & {record.wifeFirstName}
        </div>
      </div>
    ),
  },
  { title: 'Email', dataIndex: 'email' },
  { title: 'Phone', dataIndex: 'phone' },
  {
    title: 'Board',
    dataIndex: 'boardDecision',
    render: (status) => <StatusTag status={status} />,
  },
  {
    title: 'Stage',
    dataIndex: 'stage',
    render: (stage) => <StageTag stage={stage} />,
  },
  {
    title: 'Created',
    dataIndex: 'createdAt',
    render: (date) => dayjs(date).format('MMM D, YYYY'),
  },
];
```

#### Page Component
```tsx
// src/features/applicants/ApplicantListPage.tsx
export const ApplicantListPage = () => {
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>();
  const [stageFilter, setStageFilter] = useState<string>();
  
  const { data, isLoading } = useApplicants({ search, status: statusFilter, stage: stageFilter });
  
  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1>Applicants</h1>
          <p className="page-subtitle">
            {data?.totalCount} total • {data?.pendingCount} pending review
          </p>
        </div>
        <Button type="primary" icon={<PlusOutlined />}>
          Add Applicant
        </Button>
      </div>
      
      <Card>
        <div className="filters-row">
          <Input.Search 
            placeholder="Search by name..." 
            style={{ width: 250 }}
            onChange={(e) => setSearch(e.target.value)}
          />
          <Select placeholder="All Decisions" allowClear onChange={setStatusFilter}>
            <Option value="Pending">Pending</Option>
            <Option value="Approved">Approved</Option>
            <Option value="Rejected">Rejected</Option>
          </Select>
          <Select placeholder="All Stages" allowClear onChange={setStageFilter}>
            <Option value="Submitted">Submitted</Option>
            <Option value="HouseHunting">House Hunting</Option>
            <Option value="UnderContract">Under Contract</Option>
            <Option value="Closed">Closed</Option>
          </Select>
        </div>
        
        <Table 
          columns={columns}
          dataSource={data?.items}
          loading={isLoading}
          rowKey="id"
          onRow={(record) => ({
            onClick: () => navigate(`/applicants/${record.id}`),
            style: { cursor: 'pointer' },
          })}
        />
      </Card>
    </div>
  );
};
```

### Definition of Done
- [ ] Page header with title, count, and "Add Applicant" button
- [ ] Filters row with search, status, and stage dropdowns
- [ ] Table displays applicant data with custom tags
- [ ] Row hover shows light blue background
- [ ] Clicking row navigates to detail page
- [ ] Loading state shows skeleton/spinner
- [ ] Empty state shows message

---

## US-F05: Applicant Detail Page (3 points)

### Story
**As a** coordinator  
**I want to** view detailed information about an applicant  
**So that** I can review their application and housing search

### Design Requirements

#### Page Layout
```
┌──────────────────────────────────────────────────────────┐
│ ← Back to Applicants                                     │
│                                                          │
│ Cohen Family                      [Add Note] [Edit]      │
│ [Approved] [House Hunting]  Created Jan 15, 2026         │
├──────────────────────────────────────────────────────────┤
│ [Overview] [Housing Search] [Children (4)] [Activity]    │
├──────────────────────────────────────────────────────────┤
│ ┌─────────────────────┐  ┌─────────────────────┐        │
│ │ HUSBAND             │  │ WIFE                │        │
│ │ Name: Moshe Cohen   │  │ Name: Sarah         │        │
│ │ Email: moshe@...    │  │ Email: sarah@...    │        │
│ │ Phone: (908)...     │  │ High School: Beth R │        │
│ └─────────────────────┘  └─────────────────────┘        │
│ ┌─────────────────────┐  ┌─────────────────────┐        │
│ │ ADDRESS             │  │ PREFERENCES         │        │
│ │ 123 Brooklyn Ave    │  │ Budget: $450,000    │        │
│ │ Brooklyn, NY 11213  │  │ Bedrooms: 4+        │        │
│ │ Kehila: Crown Hts   │  │ Cities: Union, RP   │        │
│ └─────────────────────┘  └─────────────────────┘        │
└──────────────────────────────────────────────────────────┘
```

#### Page Header
```tsx
<div className="detail-header">
  <Link to="/applicants" className="back-link">
    <LeftOutlined /> Back to Applicants
  </Link>
  
  <div className="detail-title-row">
    <div>
      <h1>{applicant.familyName} Family</h1>
      <Space>
        <StatusTag status={applicant.boardDecision} />
        <StageTag stage={applicant.housingSearch?.stage} />
        <span className="meta-text">
          Created {dayjs(applicant.createdAt).format('MMM D, YYYY')}
        </span>
      </Space>
    </div>
    <Space>
      <Button icon={<EditOutlined />}>Add Note</Button>
      <Button type="primary" icon={<EditOutlined />}>Edit</Button>
    </Space>
  </div>
</div>
```

#### Tabs
```tsx
<Tabs defaultActiveKey="overview" items={[
  { key: 'overview', label: 'Overview' },
  { key: 'housing', label: 'Housing Search' },
  { key: 'children', label: `Children (${applicant.children?.length || 0})` },
  { key: 'activity', label: 'Activity' },
]} />
```

#### Info Section Cards
```tsx
// src/components/common/InfoSection.tsx
export const InfoSection = ({ 
  title, 
  items 
}: { 
  title: string; 
  items: { label: string; value: React.ReactNode }[] 
}) => (
  <Card className="info-section">
    <div className="section-title">{title}</div>
    {items.map((item, i) => (
      <div key={i} className="info-row">
        <span className="info-label">{item.label}</span>
        <span className="info-value">{item.value}</span>
      </div>
    ))}
  </Card>
);

// Styling
.info-section {
  border: 1px solid var(--neutral-200);
  border-radius: 8px;
}

.section-title {
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--neutral-500);
  margin-bottom: 16px;
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid var(--neutral-100);
}

.info-row:last-child {
  border-bottom: none;
}

.info-label {
  color: var(--neutral-500);
  font-size: 13px;
}

.info-value {
  font-weight: 500;
  color: var(--neutral-900);
}
```

#### Overview Tab Content
```tsx
<div className="detail-grid">
  <InfoSection 
    title="Husband Information"
    items={[
      { label: 'Name', value: `${husband.firstName} ${husband.lastName}` },
      { label: 'Email', value: husband.email },
      { label: 'Phone', value: formatPhone(husband.phoneNumbers[0]) },
      { label: 'Occupation', value: husband.occupation },
    ]}
  />
  
  <InfoSection 
    title="Wife Information"
    items={[
      { label: 'Name', value: `${wife.firstName} (${wife.maidenName})` },
      { label: 'Email', value: wife.email },
      { label: 'High School', value: wife.highSchool },
      { label: 'Occupation', value: wife.occupation },
    ]}
  />
  
  <InfoSection 
    title="Current Address"
    items={[
      { label: 'Address', value: formatAddress(applicant.address) },
      { label: 'Current Kehila', value: applicant.currentKehila },
      { label: 'Shabbos Shul', value: applicant.shabbosShul },
    ]}
  />
  
  <InfoSection 
    title="Housing Preferences"
    items={[
      { label: 'Budget', value: formatCurrency(preferences.budgetAmount) },
      { label: 'Bedrooms', value: `${preferences.minBedrooms}+ bedrooms` },
      { label: 'Cities', value: preferences.preferredCities?.join(', ') },
      { label: 'Timeline', value: formatTimeline(preferences.moveTimeline) },
    ]}
  />
</div>
```

### Definition of Done
- [ ] Back link navigates to applicant list
- [ ] Header shows family name, status tags, and action buttons
- [ ] Tabs switch between Overview, Housing Search, Children, Activity
- [ ] Info sections display in 2-column grid
- [ ] Data loads from API with loading state
- [ ] 404 handling if applicant not found

---

## US-F06: Pipeline Kanban Board (5 points) - NEW

### Story
**As a** coordinator  
**I want to** view families in a Kanban board by housing search stage  
**So that** I can visualize and manage the pipeline

### Design Requirements

**Reference**: `prototype-pipeline-kanban.html`

#### Kanban Board Layout
```
┌─────────────────────────────────────────────────────────────────────┐
│ Pipeline                              [Export] [+ Add Family]       │
├─────────────────────────────────────────────────────────────────────┤
│ [All Cities ▼] [All Statuses ▼]                    [Kanban] [Table] │
├─────────────────────────────────────────────────────────────────────┤
│ ┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐            │
│ │● Submitted│ │● Hunting  │ │● Contract │ │● Closed   │            │
│ │    (3)    │ │    (4)    │ │    (2)    │ │    (3)    │            │
│ ├───────────┤ ├───────────┤ ├───────────┤ ├───────────┤            │
│ │ [Card]    │ │ [Card]    │ │ [Card]    │ │ [Card]    │            │
│ │ [Card]    │ │ [Card]    │ │ [Card]    │ │ [Card]    │            │
│ │ [Card]    │ │ [Card]    │ │           │ │ [Card]    │            │
│ │           │ │ [Card]    │ │           │ │           │            │
│ └───────────┘ └───────────┘ └───────────┘ └───────────┘            │
└─────────────────────────────────────────────────────────────────────┘
```

#### Column Specifications
| Element | Specification |
|---------|---------------|
| Column width | 300px min |
| Column bg | `neutral-100` |
| Column radius | 12px |
| Header | Colored dot + title + count badge |
| Cards area | Scrollable, padding 12px |

#### Card Specifications
| Element | Specification |
|---------|---------------|
| Background | White |
| Border-left | 4px solid (stage color) |
| Border-radius | 10px |
| Padding | 16px |
| Shadow | `shadow-sm` |
| Hover | Lift effect (`shadow-md`, translateY -2px) |

#### Stage Colors
| Stage | Dot/Border Color | Background |
|-------|------------------|------------|
| Submitted | `#3b82f6` | `#dbeafe` |
| House Hunting | `#f59e0b` | `#fef3c7` |
| Under Contract | `#8b5cf6` | `#ede9fe` |
| Closed | `#10b981` | `#d1fae5` |

#### Kanban Card Content
```tsx
// src/features/pipeline/KanbanCard.tsx
export const KanbanCard = ({ family }: { family: PipelineFamily }) => (
  <div 
    className={`kanban-card ${family.stage.toLowerCase()}`}
    draggable
    onClick={() => openDetail(family.id)}
  >
    <div className="card-header">
      <span className="family-name">{family.familyName} Family</span>
      <StatusTag status={family.boardDecision} />
    </div>
    
    <div className="card-details">
      {family.husbandName} & {family.wifeName} • {family.childrenCount} children
    </div>
    
    <div className="card-meta">
      <div className="city-tags">
        {family.preferredCities?.map(city => (
          <span key={city} className="city-tag">{city}</span>
        ))}
      </div>
      <span className="days-in-stage">{family.daysInStage} days</span>
    </div>
    
    {family.budget && (
      <div className="card-budget">💰 {formatCurrency(family.budget)}</div>
    )}
  </div>
);
```

#### Drag and Drop
Implement using `@dnd-kit/core` or native HTML5 drag/drop:

```tsx
// On drop, call API to change stage
const handleDrop = async (familyId: string, newStage: string) => {
  await changeStage.mutateAsync({ familyId, stage: newStage });
  showNotification('success', `Family moved to ${formatStage(newStage)}`);
};
```

#### Modal on Card Click
When clicking a card, show a modal with:
- Family details (contact, preferences)
- Stage change buttons
- Quick actions (Add Note, View Full Profile)

### Definition of Done
- [ ] Four columns render with correct colors
- [ ] Cards display family info with stage-colored border
- [ ] Drag and drop changes stage (calls API)
- [ ] Click on card opens detail modal
- [ ] Search filters cards across all columns
- [ ] City/Status filters work
- [ ] View toggle switches between Kanban and Table view

---

## 📁 FILES INCLUDED

1. **`crm-design-system-v4.html`** - Complete design system reference
2. **`prototype-login-page.html`** - Interactive login page prototype
3. **`prototype-pipeline-kanban.html`** - Interactive Kanban board prototype
4. **`antd-theme.ts`** - Ant Design ConfigProvider theme configuration

---

## 🚀 IMPLEMENTATION ORDER

1. **US-F01** - Project setup with theme (Day 1)
2. **US-F02** - Login page (Day 2)
3. **US-F03** - App shell & navigation (Day 3)
4. **US-F04** - Applicant list (Day 4)
5. **US-F05** - Applicant detail (Day 5)
6. **US-F06** - Pipeline Kanban (Days 6-7)

