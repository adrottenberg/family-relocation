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
