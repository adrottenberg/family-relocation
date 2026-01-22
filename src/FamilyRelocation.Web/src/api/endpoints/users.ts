import apiClient from '../client';

export interface UserDto {
  id: string;
  email: string;
  name: string | null;
  roles: string[];
  status: string;
  emailVerified: boolean;
  mfaEnabled: boolean;
  createdAt: string;
  lastLogin: string | null;
}

export interface UserListResponse {
  users: UserDto[];
  paginationToken: string | null;
}

export interface UpdateRolesRequest {
  roles: string[];
}

export interface UpdateRolesResponse {
  userId: string;
  roles: string[];
  message: string;
}

export interface UserStatusResponse {
  userId: string;
  status: string;
  message: string;
}

export interface CreateUserRequest {
  email: string;
  roles?: string[];
}

export interface CreateUserResponse {
  userId: string;
  email: string;
  temporaryPassword: string;
  roles: string[];
  message: string;
}

export const usersApi = {
  create: async (request: CreateUserRequest): Promise<CreateUserResponse> => {
    const response = await apiClient.post<CreateUserResponse>('/users', request);
    return response.data;
  },

  list: async (params?: {
    search?: string;
    status?: string;
    limit?: number;
    paginationToken?: string;
  }): Promise<UserListResponse> => {
    const searchParams = new URLSearchParams();
    if (params?.search) searchParams.append('search', params.search);
    if (params?.status) searchParams.append('status', params.status);
    if (params?.limit) searchParams.append('limit', params.limit.toString());
    if (params?.paginationToken) searchParams.append('paginationToken', params.paginationToken);

    const queryString = searchParams.toString();
    const url = queryString ? `/users?${queryString}` : '/users';
    const response = await apiClient.get<UserListResponse>(url);
    return response.data;
  },

  getById: async (userId: string): Promise<UserDto> => {
    const response = await apiClient.get<UserDto>(`/users/${encodeURIComponent(userId)}`);
    return response.data;
  },

  updateRoles: async (userId: string, roles: string[]): Promise<UpdateRolesResponse> => {
    const response = await apiClient.put<UpdateRolesResponse>(
      `/users/${encodeURIComponent(userId)}/roles`,
      { roles }
    );
    return response.data;
  },

  deactivate: async (userId: string): Promise<UserStatusResponse> => {
    const response = await apiClient.post<UserStatusResponse>(
      `/users/${encodeURIComponent(userId)}/deactivate`
    );
    return response.data;
  },

  reactivate: async (userId: string): Promise<UserStatusResponse> => {
    const response = await apiClient.post<UserStatusResponse>(
      `/users/${encodeURIComponent(userId)}/reactivate`
    );
    return response.data;
  },
};
