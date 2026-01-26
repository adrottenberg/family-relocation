import apiClient from '../client';
import type { PropertyMatchDto, PropertyMatchListDto } from '../types';

export interface CreatePropertyMatchRequest {
  housingSearchId: string;
  propertyId: string;
  notes?: string;
}

export interface UpdatePropertyMatchStatusRequest {
  status: string;
  notes?: string;
  offerAmount?: number;
}

export const propertyMatchesApi = {
  // Get matches for a housing search
  getForHousingSearch: async (housingSearchId: string, status?: string): Promise<PropertyMatchListDto[]> => {
    const params: Record<string, string> = { housingSearchId };
    if (status) params.status = status;
    const response = await apiClient.get('/property-matches', { params });
    return response.data;
  },

  // Get matches for a property (interested families)
  getForProperty: async (propertyId: string, status?: string): Promise<PropertyMatchListDto[]> => {
    const params: Record<string, string> = { propertyId };
    if (status) params.status = status;
    const response = await apiClient.get('/property-matches', { params });
    return response.data;
  },

  // Get a single match by ID
  getById: async (id: string): Promise<PropertyMatchDto> => {
    const response = await apiClient.get(`/property-matches/${id}`);
    return response.data;
  },

  // Create a new match
  create: async (request: CreatePropertyMatchRequest): Promise<PropertyMatchDto> => {
    const response = await apiClient.post('/property-matches', request);
    return response.data;
  },

  // Update match status
  updateStatus: async (id: string, request: UpdatePropertyMatchStatusRequest): Promise<PropertyMatchDto> => {
    const response = await apiClient.put(`/property-matches/${id}/status`, request);
    return response.data;
  },

  // Request showings for multiple matches (batch)
  requestShowings: async (matchIds: string[]): Promise<{ updatedCount: number }> => {
    const response = await apiClient.post('/property-matches/request-showings', matchIds);
    return response.data;
  },

  // Delete a match
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/property-matches/${id}`);
  },
};
