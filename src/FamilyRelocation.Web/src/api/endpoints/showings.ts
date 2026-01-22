import apiClient from '../client';
import type { ShowingDto, ShowingListDto } from '../types';

export interface GetShowingsParams {
  fromDate?: string;
  toDate?: string;
  status?: string;
  brokerId?: string;
  propertyMatchId?: string;
}

export interface ScheduleShowingRequest {
  propertyMatchId: string;
  scheduledDate: string;
  scheduledTime: string;
  notes?: string;
  brokerUserId?: string;
}

export interface RescheduleShowingRequest {
  newDate: string;
  newTime: string;
}

export interface UpdateShowingStatusRequest {
  status: string;
  notes?: string;
}

export const showingsApi = {
  // Get showings with filters
  getAll: async (params?: GetShowingsParams): Promise<ShowingListDto[]> => {
    const response = await apiClient.get('/showings', { params });
    return response.data;
  },

  // Get upcoming showings
  getUpcoming: async (days?: number): Promise<ShowingListDto[]> => {
    const params = days ? { days } : {};
    const response = await apiClient.get('/showings/upcoming', { params });
    return response.data;
  },

  // Get a single showing by ID
  getById: async (id: string): Promise<ShowingDto> => {
    const response = await apiClient.get(`/showings/${id}`);
    return response.data;
  },

  // Schedule a new showing
  schedule: async (request: ScheduleShowingRequest): Promise<ShowingDto> => {
    const response = await apiClient.post('/showings', request);
    return response.data;
  },

  // Reschedule a showing
  reschedule: async (id: string, request: RescheduleShowingRequest): Promise<ShowingDto> => {
    const response = await apiClient.put(`/showings/${id}/reschedule`, request);
    return response.data;
  },

  // Update showing status (complete, cancel, no-show)
  updateStatus: async (id: string, request: UpdateShowingStatusRequest): Promise<ShowingDto> => {
    const response = await apiClient.put(`/showings/${id}/status`, request);
    return response.data;
  },
};
