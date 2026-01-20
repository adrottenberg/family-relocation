import apiClient from '../client';

export interface UploadResult {
  documentKey: string;
  documentUrl: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedAt: string;
}

export interface PresignedUrlResult {
  url: string;
  expiresAt: string;
}

export const documentsApi = {
  /**
   * Upload a document to S3
   */
  upload: async (
    file: File,
    applicantId: string,
    documentType: 'BrokerAgreement' | 'CommunityTakanos' | 'Other'
  ): Promise<UploadResult> => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('applicantId', applicantId);
    formData.append('documentType', documentType);

    const response = await apiClient.post('/documents/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
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
};
