import apiClient from '../client';
import type { LoginRequest, LoginResponse, ChallengeResponse, RefreshTokenRequest, RefreshTokenResponse } from '../types';

export const authApi = {
  login: async (request: LoginRequest): Promise<LoginResponse | ChallengeResponse> => {
    const response = await apiClient.post('/auth/login', request);
    return response.data;
  },

  respondToChallenge: async (request: {
    email: string;
    challengeName: string;
    session: string;
    responses: Record<string, string>;
  }): Promise<LoginResponse | ChallengeResponse> => {
    const response = await apiClient.post('/auth/respond-to-challenge', request);
    return response.data;
  },

  refresh: async (request: RefreshTokenRequest): Promise<RefreshTokenResponse> => {
    const response = await apiClient.post('/auth/refresh', request);
    return response.data;
  },

  forgotPassword: async (email: string): Promise<{ message: string }> => {
    const response = await apiClient.post('/auth/forgot-password', { email });
    return response.data;
  },

  confirmForgotPassword: async (request: {
    email: string;
    code: string;
    newPassword: string;
  }): Promise<{ message: string }> => {
    const response = await apiClient.post('/auth/confirm-forgot-password', request);
    return response.data;
  },

  resendConfirmation: async (email: string): Promise<{ message: string }> => {
    const response = await apiClient.post('/auth/resend-confirmation', { email });
    return response.data;
  },

  confirmEmail: async (request: { email: string; code: string }): Promise<{ message: string }> => {
    const response = await apiClient.post('/auth/confirm-email', request);
    return response.data;
  },

  getMyRoles: async (): Promise<{ userId: string; email: string | null; roles: string[] }> => {
    const response = await apiClient.get('/auth/me/roles');
    return response.data;
  },

  bootstrapAdmin: async (): Promise<{ success: boolean; message: string; roles: string[] }> => {
    const response = await apiClient.post('/auth/bootstrap-admin');
    return response.data;
  },
};

// Helper to check if response is a challenge
export const isChallengeResponse = (
  response: LoginResponse | ChallengeResponse
): response is ChallengeResponse => {
  return 'challengeName' in response;
};
