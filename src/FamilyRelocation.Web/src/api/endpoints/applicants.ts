import apiClient from '../client';
import type {
  ApplicantDto,
  ApplicantListItemDto,
  PaginatedList,
  HousingSearchDto,
  AuditLogDto,
} from '../types';

export interface GetApplicantsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  boardDecision?: string;
  stage?: string;
}

export interface ChangeStageRequest {
  newStage: string;
  notes?: string;
}

export interface UpdatePreferencesRequest {
  budgetAmount?: number;
  minBedrooms?: number;
  minBathrooms?: number;
  preferredCities?: string[];
  requiredFeatures?: string[];
  moveTimeline?: string;
  shulProximity?: {
    maxWalkingMinutes?: number;
    preferredShuls?: string[];
  };
}

export interface SetBoardDecisionRequest {
  decision: string;
  notes?: string;
  reviewDate?: string;
}

export const applicantsApi = {
  getAll: async (params?: GetApplicantsParams): Promise<PaginatedList<ApplicantListItemDto>> => {
    const response = await apiClient.get('/applicants', { params });
    return response.data;
  },

  getById: async (id: string): Promise<ApplicantDto> => {
    const response = await apiClient.get(`/applicants/${id}`);
    return response.data;
  },

  create: async (applicant: Partial<ApplicantDto>): Promise<{ applicantId: string; housingSearchId: string }> => {
    const response = await apiClient.post('/applicants', applicant);
    return response.data;
  },

  update: async (id: string, applicant: Partial<ApplicantDto>): Promise<ApplicantDto> => {
    const response = await apiClient.put(`/applicants/${id}`, applicant);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/applicants/${id}`);
  },

  // Housing Search
  changeStage: async (applicantId: string, request: ChangeStageRequest): Promise<HousingSearchDto> => {
    const response = await apiClient.put(
      `/applicants/${applicantId}/housing-search/stage`,
      request
    );
    return response.data;
  },

  updatePreferences: async (
    applicantId: string,
    preferences: UpdatePreferencesRequest
  ): Promise<HousingSearchDto> => {
    const response = await apiClient.put(
      `/applicants/${applicantId}/housing-search/preferences`,
      preferences
    );
    return response.data;
  },

  // Board Review
  setBoardDecision: async (
    applicantId: string,
    request: SetBoardDecisionRequest
  ): Promise<{
    applicantId: string;
    housingSearchId: string;
    previousStage: string;
    newStage: string;
    message: string;
  }> => {
    const response = await apiClient.put(`/applicants/${applicantId}/board-review`, request);
    return response.data;
  },

  startHouseHunting: async (applicantId: string): Promise<HousingSearchDto> => {
    const response = await apiClient.put(
      `/applicants/${applicantId}/stage`,
      { newStage: 'HouseHunting' }
    );
    return response.data;
  },

  // Audit logs for an applicant
  getAuditLogs: async (
    applicantId: string,
    params?: { page?: number; pageSize?: number }
  ): Promise<PaginatedList<AuditLogDto>> => {
    const response = await apiClient.get('/audit-logs', {
      params: {
        entityType: 'Applicant',
        entityId: applicantId,
        ...params,
      },
    });
    return response.data;
  },
};
