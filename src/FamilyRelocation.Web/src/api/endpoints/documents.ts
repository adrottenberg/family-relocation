import apiClient from '../client';
import type { ApplicantDocumentDto } from '../types';

export interface UploadResult {
  documentKey: string;
  documentUrl: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedAt: string;
}

export interface DocumentUploadResponse {
  document: ApplicantDocumentDto;
  uploadResult: UploadResult;
}

export interface PresignedUrlResult {
  url: string;
  expiresAt: string;
}

export const documentsApi = {
  /**
   * Get all documents for an applicant
   */
  getApplicantDocuments: async (applicantId: string): Promise<ApplicantDocumentDto[]> => {
    const response = await apiClient.get(`/documents/applicant/${applicantId}`);
    return response.data;
  },

  /**
   * Upload a document to S3 using document type ID
   */
  upload: async (
    file: File,
    applicantId: string,
    documentTypeId: string
  ): Promise<DocumentUploadResponse> => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('applicantId', applicantId);
    formData.append('documentTypeId', documentTypeId);

    const response = await apiClient.post('/documents/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  /**
   * Delete a document
   */
  delete: async (documentId: string): Promise<void> => {
    await apiClient.delete(`/documents/${documentId}`);
  },

  /**
   * Get a pre-signed URL for viewing/downloading a document
   */
  getPresignedUrl: async (
    documentKey: string,
    expiryMinutes?: number
  ): Promise<PresignedUrlResult> => {
    const response = await apiClient.get('/documents/presigned-url', {
      params: { documentKey, expiryMinutes },
    });
    return response.data;
  },

  /**
   * Get the URL for viewing a document inline (PDF/images displayed in browser)
   */
  getViewUrl: (documentId: string): string => {
    return `/api/documents/${documentId}/view`;
  },

  /**
   * Get the URL for downloading a document (triggers file download)
   */
  getDownloadUrl: (documentId: string): string => {
    return `/api/documents/${documentId}/download?download=true`;
  },
};
