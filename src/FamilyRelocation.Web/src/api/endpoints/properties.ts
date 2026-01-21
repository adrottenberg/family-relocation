import apiClient from '../client';
import type { PropertyDto, PropertyListDto, PaginatedList } from '../types';

export interface GetPropertiesParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
  minPrice?: number;
  maxPrice?: number;
  minBeds?: number;
  city?: string;
  sortBy?: string;
  sortOrder?: string;
}

export interface CreatePropertyRequest {
  street: string;
  street2?: string;
  city: string;
  state: string;
  zipCode: string;
  price: number;
  bedrooms: number;
  bathrooms: number;
  squareFeet?: number;
  lotSize?: number;
  yearBuilt?: number;
  annualTaxes?: number;
  features?: string[];
  mlsNumber?: string;
  notes?: string;
}

export interface UpdatePropertyRequest extends CreatePropertyRequest {
  id: string;
}

export const propertiesApi = {
  getAll: async (params?: GetPropertiesParams): Promise<PaginatedList<PropertyListDto>> => {
    const response = await apiClient.get('/properties', { params });
    return response.data;
  },

  getById: async (id: string): Promise<PropertyDto> => {
    const response = await apiClient.get(`/properties/${id}`);
    return response.data;
  },

  create: async (property: CreatePropertyRequest): Promise<PropertyDto> => {
    const response = await apiClient.post('/properties', property);
    return response.data;
  },

  update: async (id: string, property: UpdatePropertyRequest): Promise<PropertyDto> => {
    const response = await apiClient.put(`/properties/${id}`, property);
    return response.data;
  },

  updateStatus: async (id: string, status: string): Promise<PropertyDto> => {
    const response = await apiClient.put(`/properties/${id}/status`, { status });
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/properties/${id}`);
  },
};
