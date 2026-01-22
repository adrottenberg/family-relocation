import apiClient from '../client';
import type { DashboardStatsDto } from '../types';

export const dashboardApi = {
  getStats: async (): Promise<DashboardStatsDto> => {
    const response = await apiClient.get('/dashboard/stats');
    return response.data;
  },
};
