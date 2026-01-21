import apiClient from '../client';
import type { HousingPreferencesDto } from '../types';

export interface ChangeStageRequest {
  newStage: string;
  reason?: string;
  contract?: {
    propertyId?: string;
    price: number;
    expectedClosingDate?: string;
  };
  closingDate?: string;
  movedInDate?: string;
}

export interface ChangeStageResponse {
  housingSearchId: string;
  stage: string;
  stageChangedDate: string;
}

export interface UpdatePreferencesRequest {
  budgetAmount?: number;
  minBedrooms?: number;
  minBathrooms?: number;
  requiredFeatures?: string[];
  shulProximity?: {
    preferredShulIds?: string[];
    maxWalkingDistanceMiles?: number;
    maxWalkingTimeMinutes?: number;
    anyShulAcceptable?: boolean;
  };
  moveTimeline?: string;
}

export interface UpdatePreferencesResponse {
  housingSearchId: string;
  preferences: HousingPreferencesDto;
  modifiedDate: string;
}

export const housingSearchesApi = {
  /**
   * Changes the stage of a housing search.
   * Required fields depend on the target stage:
   * - AwaitingAgreements -> Searching: No additional fields required
   * - Searching -> UnderContract: contract.price required
   * - Searching -> Paused: reason optional
   * - UnderContract -> Closed: closingDate required
   * - UnderContract -> Searching: reason optional (contract fell through)
   * - Closed -> MovedIn: movedInDate required
   * - Paused -> Searching: No additional fields required
   */
  changeStage: async (
    housingSearchId: string,
    request: ChangeStageRequest
  ): Promise<ChangeStageResponse> => {
    const response = await apiClient.put(
      `/housing-searches/${housingSearchId}/stage`,
      request
    );
    return response.data;
  },

  /**
   * Updates housing preferences for a housing search.
   * Can only be updated after applicant is approved.
   */
  updatePreferences: async (
    housingSearchId: string,
    preferences: UpdatePreferencesRequest
  ): Promise<UpdatePreferencesResponse> => {
    const response = await apiClient.put(
      `/housing-searches/${housingSearchId}/preferences`,
      preferences
    );
    return response.data;
  },
};
