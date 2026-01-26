import apiClient from '../client';
import type {
  ReminderDto,
  ReminderListDto,
  DueRemindersReportDto,
  PaginatedList,
} from '../types';

export interface GetRemindersParams {
  skip?: number;
  take?: number;
  status?: string;
  priority?: string;
  entityType?: string;
  entityId?: string;
  assignedToUserId?: string;
  dueDateTimeFrom?: string; // UTC ISO datetime
  dueDateTimeTo?: string; // UTC ISO datetime
  overdueOnly?: boolean;
  dueTodayOnly?: boolean;
}

export interface CreateReminderRequest {
  title: string;
  dueDateTime: string; // UTC ISO datetime
  priority?: string;
  entityType: string;
  entityId: string;
  assignedToUserId?: string;
  notes?: string;
  sendEmailNotification?: boolean;
}

export interface UpdateReminderRequest {
  title?: string;
  dueDateTime?: string; // UTC ISO datetime
  priority?: string;
  notes?: string;
  assignedToUserId?: string;
  sendEmailNotification?: boolean;
}

export interface SnoozeReminderRequest {
  snoozeUntil: string;
}

export const remindersApi = {
  getAll: async (params?: GetRemindersParams): Promise<PaginatedList<ReminderListDto>> => {
    const response = await apiClient.get('/reminders', { params });
    return response.data;
  },

  getById: async (id: string): Promise<ReminderDto> => {
    const response = await apiClient.get(`/reminders/${id}`);
    return response.data;
  },

  getByEntity: async (
    entityType: string,
    entityId: string,
    status?: string
  ): Promise<ReminderListDto[]> => {
    const response = await apiClient.get(`/reminders/entity/${entityType}/${entityId}`, {
      params: { status },
    });
    // API returns paginated result with items property
    return response.data.items || response.data;
  },

  getDueReport: async (upcomingDays?: number, assignedToUserId?: string): Promise<DueRemindersReportDto> => {
    const response = await apiClient.get('/reminders/due-report', {
      params: { upcomingDays, assignedToUserId },
    });
    return response.data;
  },

  create: async (request: CreateReminderRequest): Promise<ReminderDto> => {
    const response = await apiClient.post('/reminders', request);
    return response.data;
  },

  update: async (id: string, request: UpdateReminderRequest): Promise<ReminderDto> => {
    const response = await apiClient.put(`/reminders/${id}`, request);
    return response.data;
  },

  complete: async (id: string): Promise<ReminderDto> => {
    const response = await apiClient.post(`/reminders/${id}/complete`);
    return response.data;
  },

  snooze: async (id: string, request: SnoozeReminderRequest): Promise<ReminderDto> => {
    const response = await apiClient.post(`/reminders/${id}/snooze`, request);
    return response.data;
  },

  dismiss: async (id: string): Promise<ReminderDto> => {
    const response = await apiClient.post(`/reminders/${id}/dismiss`);
    return response.data;
  },

  reopen: async (id: string): Promise<ReminderDto> => {
    const response = await apiClient.post(`/reminders/${id}/reopen`);
    return response.data;
  },
};
