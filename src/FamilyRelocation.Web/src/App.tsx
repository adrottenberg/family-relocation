import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './store/authStore';

// Lazy load pages for code splitting
import { lazy, Suspense } from 'react';
import { Spin } from 'antd';

const LoginPage = lazy(() => import('./features/auth/LoginPage'));
const PublicApplicationPage = lazy(() => import('./features/application/PublicApplicationPage'));
const AppLayout = lazy(() => import('./components/layout/AppLayout'));
const DashboardPage = lazy(() => import('./features/dashboard/DashboardPage'));
const ApplicantListPage = lazy(() => import('./features/applicants/ApplicantListPage'));
const ApplicantDetailPage = lazy(() => import('./features/applicants/ApplicantDetailPage'));
const PipelinePage = lazy(() => import('./features/pipeline/PipelinePage'));
const PropertiesListPage = lazy(() => import('./features/properties/PropertiesListPage'));
const PropertyDetailPage = lazy(() => import('./features/properties/PropertyDetailPage'));
const RemindersPage = lazy(() => import('./features/reminders/RemindersPage'));
const SettingsPage = lazy(() => import('./features/settings/SettingsPage'));
const UsersPage = lazy(() => import('./features/users/UsersPage'));
const ShowingsPage = lazy(() => import('./features/showings/ShowingsPage'));
const BrokerShowingsPage = lazy(() => import('./features/showings/BrokerShowingsPage'));
const ShulsPage = lazy(() => import('./features/shuls/ShulsPage'));

const LoadingFallback = () => (
  <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
    <Spin size="large" />
  </div>
);

// Protected route wrapper
const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const location = window.location;

  if (!isAuthenticated) {
    // Pass the intended URL as state so we can redirect after login
    const returnUrl = location.pathname + location.search;
    return <Navigate to="/login" state={{ returnUrl }} replace />;
  }

  return <>{children}</>;
};

function App() {
  return (
    <Suspense fallback={<LoadingFallback />}>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/apply" element={<PublicApplicationPage />} />

        {/* Protected routes */}
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <AppLayout />
            </ProtectedRoute>
          }
        >
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<DashboardPage />} />
          <Route path="applicants" element={<ApplicantListPage />} />
          <Route path="applicants/:id" element={<ApplicantDetailPage />} />
          <Route path="pipeline" element={<PipelinePage />} />
          <Route path="properties" element={<PropertiesListPage />} />
          <Route path="properties/:id" element={<PropertyDetailPage />} />
          <Route path="showings" element={<ShowingsPage />} />
          <Route path="broker-showings" element={<BrokerShowingsPage />} />
          <Route path="reminders" element={<RemindersPage />} />
          <Route path="settings" element={<SettingsPage />} />
          <Route path="users" element={<UsersPage />} />
          <Route path="shuls" element={<ShulsPage />} />
        </Route>

        {/* Catch all */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  );
}

export default App;
