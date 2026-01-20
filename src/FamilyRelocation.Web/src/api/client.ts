import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '../store/authStore';

// Create axios instance
const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - add auth token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const { tokens } = useAuthStore.getState();

    if (tokens?.accessToken) {
      config.headers.Authorization = `Bearer ${tokens.accessToken}`;
    }

    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - handle errors
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config;

    // Handle 401 Unauthorized
    if (error.response?.status === 401) {
      const { tokens, logout } = useAuthStore.getState();

      // Try to refresh token
      if (tokens?.refreshToken && originalRequest) {
        try {
          const response = await axios.post('/api/auth/refresh', {
            username: useAuthStore.getState().user?.email,
            refreshToken: tokens.refreshToken,
          });

          const { accessToken, idToken, expiresIn } = response.data;

          useAuthStore.getState().setTokens({
            ...tokens,
            accessToken,
            idToken,
            expiresIn,
          });

          // Retry original request
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          return apiClient(originalRequest);
        } catch {
          // Refresh failed, logout
          logout();
          window.location.href = '/login';
        }
      } else {
        logout();
        window.location.href = '/login';
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;

// API error helper
export interface ApiError {
  message: string;
  status?: number;
  errors?: Record<string, string[]>;
}

export const getApiError = (error: unknown): ApiError => {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<{ message?: string; errors?: Record<string, string[]> }>;
    return {
      message: axiosError.response?.data?.message || axiosError.message || 'An error occurred',
      status: axiosError.response?.status,
      errors: axiosError.response?.data?.errors,
    };
  }
  return {
    message: error instanceof Error ? error.message : 'An unknown error occurred',
  };
};
