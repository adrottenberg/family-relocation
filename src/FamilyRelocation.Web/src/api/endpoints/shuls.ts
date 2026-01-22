import apiClient from '../client';
import type { ShulDto, ShulListDto, PaginatedList, PropertyShulDistanceDto } from '../types';

export interface GetShulsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  city?: string;
  denomination?: string;
  includeInactive?: boolean;
}

export interface CreateShulRequest {
  name: string;
  street: string;
  street2?: string;
  city: string;
  state: string;
  zipCode: string;
  latitude?: number;
  longitude?: number;
  rabbi?: string;
  denomination?: string;
  website?: string;
  notes?: string;
}

export interface UpdateShulRequest extends CreateShulRequest {
  id: string;
}

export const shulsApi = {
  getAll: async (params?: GetShulsParams): Promise<PaginatedList<ShulListDto>> => {
    const response = await apiClient.get('/shuls', { params });
    return response.data;
  },

  getById: async (id: string): Promise<ShulDto> => {
    const response = await apiClient.get(`/shuls/${id}`);
    return response.data;
  },

  create: async (shul: CreateShulRequest): Promise<ShulDto> => {
    const response = await apiClient.post('/shuls', shul);
    return response.data;
  },

  update: async (id: string, shul: UpdateShulRequest): Promise<ShulDto> => {
    const response = await apiClient.put(`/shuls/${id}`, shul);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/shuls/${id}`);
  },

  getPropertyDistances: async (propertyId: string): Promise<PropertyShulDistanceDto[]> => {
    const response = await apiClient.get(`/shuls/distances/property/${propertyId}`);
    return response.data;
  },
};
