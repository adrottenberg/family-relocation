// Stage transition rules and validation
// Updated for the separated ApplicationStatus / HousingSearchStage model

// Pipeline stages combine ApplicationStatus and HousingSearchStage for display
// - Submitted: No housing search (ApplicationStatus = Submitted)
// - Searching: Has housing search in Searching stage (ApplicationStatus = Approved)
// - UnderContract: Has housing search in UnderContract stage
// - Closed: Has housing search in Closed stage
export type PipelineStage = 'Submitted' | 'Searching' | 'UnderContract' | 'Closed';

// Legacy alias for backward compatibility
export type Stage = PipelineStage;

export type TransitionType =
  | 'direct'              // Can transition directly without modal
  | 'needsBoardApproval'  // Needs board approval first (Submitted -> Searching)
  | 'needsContractInfo'   // Needs contract details (Searching -> UnderContract)
  | 'needsClosingInfo'    // Needs closing confirmation (UnderContract -> Closed)
  | 'contractFailed'      // Contract fell through (UnderContract -> Searching)
  | 'blocked';            // Transition not allowed

export interface TransitionResult {
  type: TransitionType;
  message?: string;
}

export interface ApplicantContext {
  boardDecision?: string;
  hasSignedAgreements?: boolean;
}

// Valid transitions map
// Note: Submitted -> Searching happens via board approval (which auto-creates HousingSearch)
const validTransitions: Record<PipelineStage, PipelineStage[]> = {
  Submitted: ['Searching'],      // Via board approval
  Searching: ['UnderContract'],  // When contract is signed
  UnderContract: ['Searching', 'Closed'],  // Contract failed or closed
  Closed: [], // Terminal state
};

export function validateTransition(
  fromStage: PipelineStage,
  toStage: PipelineStage,
  _context: ApplicantContext
): TransitionResult {
  // Same stage - no change needed
  if (fromStage === toStage) {
    return { type: 'direct' };
  }

  // Check if transition is valid
  if (!validTransitions[fromStage]?.includes(toStage)) {
    return {
      type: 'blocked',
      message: `Cannot move from ${formatStage(fromStage)} to ${formatStage(toStage)}`,
    };
  }

  // Submitted -> Searching: Needs board approval
  // This transition happens via SetBoardDecision which auto-creates HousingSearch
  if (fromStage === 'Submitted' && toStage === 'Searching') {
    return {
      type: 'needsBoardApproval',
      message: 'Board approval is required to move this applicant forward',
    };
  }

  // Searching -> UnderContract: Need contract info
  if (fromStage === 'Searching' && toStage === 'UnderContract') {
    return {
      type: 'needsContractInfo',
      message: 'Please provide contract details',
    };
  }

  // UnderContract -> Closed: Need closing confirmation
  if (fromStage === 'UnderContract' && toStage === 'Closed') {
    return {
      type: 'needsClosingInfo',
      message: 'Please confirm closing details',
    };
  }

  // UnderContract -> Searching: Contract failed
  if (fromStage === 'UnderContract' && toStage === 'Searching') {
    return {
      type: 'contractFailed',
      message: 'Please provide the reason the contract fell through',
    };
  }

  return { type: 'direct' };
}

export function formatStage(stage: string): string {
  const names: Record<string, string> = {
    Submitted: 'Submitted',
    Searching: 'Searching',
    UnderContract: 'Under Contract',
    Closed: 'Closed',
    MovedIn: 'Moved In',
    Paused: 'Paused',
    // Legacy names for backward compatibility
    BoardApproved: 'Board Approved',
    HouseHunting: 'Searching',
  };
  return names[stage] || stage;
}

// Helper to determine the pipeline stage from applicant data
export function getPipelineStage(
  boardDecision?: string,
  housingSearchStage?: string
): PipelineStage {
  // If no board decision or not approved, they're in Submitted
  if (!boardDecision || boardDecision !== 'Approved') {
    return 'Submitted';
  }

  // Approved - check housing search stage
  if (!housingSearchStage) {
    // Approved but no housing search - shouldn't happen, but treat as Searching
    return 'Searching';
  }

  // Map housing search stage to pipeline stage
  switch (housingSearchStage) {
    case 'Searching':
    case 'Paused':
      return 'Searching';
    case 'UnderContract':
      return 'UnderContract';
    case 'Closed':
    case 'MovedIn':
      return 'Closed';
    default:
      return 'Searching';
  }
}
