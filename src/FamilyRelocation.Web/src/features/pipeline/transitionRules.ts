// Stage transition rules and validation
// Updated for the separated ApplicationStatus / HousingSearchStage model

// Pipeline stages combine ApplicationStatus and HousingSearchStage for display
// - Submitted: No housing search (ApplicationStatus = Submitted)
// - AwaitingAgreements: Board approved, waiting for agreements to be signed
// - Searching: Has housing search in Searching stage (actively searching)
// - UnderContract: Has housing search in UnderContract stage
// - Closed: Has housing search in Closed stage
export type PipelineStage = 'Submitted' | 'AwaitingAgreements' | 'Searching' | 'UnderContract' | 'Closed';

// Legacy alias for backward compatibility
export type Stage = PipelineStage;

export type TransitionType =
  | 'direct'              // Can transition directly without modal
  | 'needsBoardApproval'  // Needs board approval first (Submitted -> AwaitingAgreements)
  | 'needsAgreements'     // Needs agreements signed (AwaitingAgreements -> Searching)
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
// Note: Submitted -> AwaitingAgreements happens via board approval (which auto-creates HousingSearch)
// Note: AwaitingAgreements -> Searching happens when required agreements are signed
const validTransitions: Record<PipelineStage, PipelineStage[]> = {
  Submitted: ['AwaitingAgreements'],      // Via board approval
  AwaitingAgreements: ['Searching'],      // When agreements are signed
  Searching: ['UnderContract'],           // When contract is signed
  UnderContract: ['Searching', 'Closed'], // Contract failed or closed
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

  // Submitted -> AwaitingAgreements: Needs board approval
  // This transition happens via SetBoardDecision which auto-creates HousingSearch in AwaitingAgreements stage
  if (fromStage === 'Submitted' && toStage === 'AwaitingAgreements') {
    return {
      type: 'needsBoardApproval',
      message: 'Board approval is required to move this applicant forward',
    };
  }

  // AwaitingAgreements -> Searching: Needs agreements signed
  if (fromStage === 'AwaitingAgreements' && toStage === 'Searching') {
    return {
      type: 'needsAgreements',
      message: 'Required agreements must be signed before searching can begin',
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
    AwaitingAgreements: 'Awaiting Agreements',
    Searching: 'Searching',
    UnderContract: 'Under Contract',
    Closed: 'Closed',
    MovedIn: 'Moved In',
    Paused: 'Paused',
    // Legacy names for backward compatibility
    BoardApproved: 'Awaiting Agreements',
    HouseHunting: 'Searching',
  };
  return names[stage] || stage;
}

// Helper to determine the pipeline stage from applicant data
// Returns null for rejected applicants (they should not appear in pipeline)
export function getPipelineStage(
  boardDecision?: string,
  housingSearchStage?: string
): PipelineStage | null {
  // Rejected applicants don't appear in the pipeline
  if (boardDecision === 'Rejected') {
    return null;
  }

  // If no board decision or pending, they're in Submitted
  if (!boardDecision || boardDecision === 'Pending' || boardDecision === 'Deferred') {
    return 'Submitted';
  }

  // Approved - check housing search stage
  if (!housingSearchStage) {
    // Approved but no housing search - shouldn't happen, but treat as AwaitingAgreements
    return 'AwaitingAgreements';
  }

  // Map housing search stage to pipeline stage
  switch (housingSearchStage) {
    case 'AwaitingAgreements':
      return 'AwaitingAgreements';
    case 'Searching':
    case 'Paused':
      return 'Searching';
    case 'UnderContract':
      return 'UnderContract';
    case 'Closed':
    case 'MovedIn':
      return 'Closed';
    default:
      return 'AwaitingAgreements';
  }
}
