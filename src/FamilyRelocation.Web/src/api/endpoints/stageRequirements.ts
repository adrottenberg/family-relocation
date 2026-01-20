import apiClient from '../client';
import type { StageTransitionRequirementsDto, StageTransitionRequirementDto } from '../types';

/**
 * Get all stage transition requirements.
 */
export async function getAllStageRequirements(): Promise<StageTransitionRequirementDto[]> {
  const response = await apiClient.get<StageTransitionRequirementDto[]>('/stage-requirements');
  return response.data;
}

/**
 * Get document requirements for a specific stage transition.
 * @param fromStage The stage transitioning from.
 * @param toStage The stage transitioning to.
 * @param applicantId Optional applicant ID to check which documents are already uploaded.
 */
export async function getStageRequirements(
  fromStage: string,
  toStage: string,
  applicantId?: string
): Promise<StageTransitionRequirementsDto> {
  const response = await apiClient.get<StageTransitionRequirementsDto>(
    `/stage-requirements/${fromStage}/${toStage}`,
    { params: applicantId ? { applicantId } : {} }
  );
  return response.data;
}

/**
 * Create a new stage transition requirement.
 */
export async function createStageRequirement(data: {
  fromStage: string;
  toStage: string;
  documentTypeId: string;
  isRequired: boolean;
}): Promise<void> {
  await apiClient.post('/stage-requirements', data);
}

/**
 * Delete a stage transition requirement.
 */
export async function deleteStageRequirement(id: string): Promise<void> {
  await apiClient.delete(`/stage-requirements/${id}`);
}
