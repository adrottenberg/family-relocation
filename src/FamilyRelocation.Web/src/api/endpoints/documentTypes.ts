import apiClient from '../client';
import type { DocumentTypeDto } from '../types';

/**
 * Get all document types.
 * @param activeOnly If true (default), only return active document types.
 */
export async function getDocumentTypes(activeOnly: boolean = true): Promise<DocumentTypeDto[]> {
  const response = await apiClient.get<DocumentTypeDto[]>('/document-types', {
    params: { activeOnly }
  });
  return response.data;
}

/**
 * Create a new document type.
 */
export async function createDocumentType(data: {
  name: string;
  displayName: string;
  description?: string;
}): Promise<void> {
  await apiClient.post('/document-types', data);
}

/**
 * Update an existing document type.
 */
export async function updateDocumentType(
  id: string,
  data: { displayName: string; description?: string }
): Promise<void> {
  await apiClient.put(`/document-types/${id}`, data);
}

/**
 * Delete (deactivate) a document type.
 */
export async function deleteDocumentType(id: string): Promise<void> {
  await apiClient.delete(`/document-types/${id}`);
}
