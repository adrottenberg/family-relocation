import apiClient from '../client';
import type { DashboardStatsDto, ActivityDto } from '../types';

export const dashboardApi = {
  getStats: async (): Promise<DashboardStatsDto> => {
    const response = await apiClient.get('/dashboard/stats');
    return response.data;
  },
};

export const activitiesApi = {
  getRecent: async (count = 10): Promise<ActivityDto[]> => {
    const response = await apiClient.get('/activities/recent', { params: { count } });
    return response.data;
  },

  getByEntity: async (entityType: string, entityId: string): Promise<ActivityDto[]> => {
    const response = await apiClient.get(`/activities/${entityType}/${entityId}`);
    return response.data;
  },
};
