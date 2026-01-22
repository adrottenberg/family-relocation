import apiClient from '../client';

export type ActivityType = 'System' | 'PhoneCall' | 'Email' | 'SMS' | 'Note';
export type CallOutcome = 'Connected' | 'Voicemail' | 'NoAnswer' | 'Busy' | 'LeftMessage';

export interface ActivityDto {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  description: string;
  userId?: string;
  userName?: string;
  timestamp: string;
  type: ActivityType;
  durationMinutes?: number;
  outcome?: string;
  followUpReminderId?: string;
}

export interface LogActivityRequest {
  entityType: string;
  entityId: string;
  type: string;
  description: string;
  durationMinutes?: number;
  outcome?: string;
  createFollowUp?: boolean;
  followUpDate?: string;
  followUpTitle?: string;
}

export interface LogActivityResult {
  activityId: string;
  followUpReminderId?: string;
}

export const activitiesApi = {
  /**
   * Get recent activities across all entities.
   */
  getRecent: async (count: number = 10): Promise<ActivityDto[]> => {
    const response = await apiClient.get<ActivityDto[]>(`/activities/recent?count=${count}`);
    return response.data;
  },

  /**
   * Get activities for a specific entity.
   */
  getByEntity: async (
    entityType: string,
    entityId: string,
    page: number = 1,
    pageSize: number = 20
  ): Promise<{ items: ActivityDto[]; totalCount: number; page: number; pageSize: number }> => {
    const response = await apiClient.get(
      `/activities/${entityType}/${entityId}?page=${page}&pageSize=${pageSize}`
    );
    return response.data;
  },

  /**
   * Log a manual activity (phone call, note, etc.).
   */
  log: async (request: LogActivityRequest): Promise<LogActivityResult> => {
    const response = await apiClient.post<LogActivityResult>('/activities', request);
    return response.data;
  },
};
