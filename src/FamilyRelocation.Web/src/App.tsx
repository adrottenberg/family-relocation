import { Routes, Route, Navigate, useLocation } from 'react-router-dom';
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
const ShowingsCalendarPage = lazy(() => import('./features/showings/ShowingsCalendarPage'));
const ScheduleShowingsPage = lazy(() => import('./features/showings/ScheduleShowingsPage'));
const ShulsPage = lazy(() => import('./features/shuls/ShulsPage'));

const LoadingFallback = () => (
  <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
    <Spin size="large" />
  </div>
);

// Protected route wrapper - checks auth BEFORE rendering children
const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const location = useLocation();

  // Redirect to login immediately if not authenticated
  // This happens before any lazy-loaded content is rendered
  if (!isAuthenticated) {
    const returnUrl = location.pathname + location.search;
    return <Navigate to="/login" state={{ returnUrl }} replace />;
  }

  return <>{children}</>;
};

function App() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const hasHydrated = useAuthStore((state) => state._hasHydrated);
  const location = useLocation();

  // Wait for store to hydrate from localStorage before making any routing decisions
  // This prevents the flash of login page before auth state is restored
  if (!hasHydrated) {
    return <LoadingFallback />;
  }

  // For protected routes, check auth BEFORE showing any loading state
  // This prevents the flash of the main layout before redirecting to login
  const isProtectedRoute = !location.pathname.startsWith('/login') && !location.pathname.startsWith('/apply');

  if (isProtectedRoute && !isAuthenticated) {
    const returnUrl = location.pathname + location.search;
    return <Navigate to="/login" state={{ returnUrl }} replace />;
  }

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
          {/* Listings (formerly Properties) */}
          <Route path="listings" element={<PropertiesListPage />} />
          <Route path="listings/:id" element={<PropertyDetailPage />} />
          {/* Redirect old properties URLs */}
          <Route path="properties" element={<Navigate to="/listings" replace />} />
          <Route path="properties/:id" element={<Navigate to="/listings" replace />} />
          {/* Showings */}
          <Route path="showings" element={<ShowingsCalendarPage />} />
          <Route path="schedule-showings" element={<ScheduleShowingsPage />} />
          {/* Redirect old broker-showings to main showings */}
          <Route path="broker-showings" element={<Navigate to="/showings" replace />} />
          <Route path="reminders" element={<RemindersPage />} />
          {/* Settings routes */}
          <Route path="settings" element={<Navigate to="/settings/document-types" replace />} />
          <Route path="settings/document-types" element={<SettingsPage section="documentTypes" />} />
          <Route path="settings/stage-requirements" element={<SettingsPage section="stageRequirements" />} />
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
