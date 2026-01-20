// Stage transition rules and validation

export type Stage = 'Submitted' | 'BoardApproved' | 'HouseHunting' | 'UnderContract' | 'Closed';

export type TransitionType =
  | 'direct'          // Can transition directly without modal
  | 'needsBoardApproval'  // Needs board approval first
  | 'needsAgreements'     // Needs signed agreements
  | 'needsContractInfo'   // Needs contract details
  | 'needsClosingInfo'    // Needs closing confirmation
  | 'contractFailed'      // Contract fell through
  | 'blocked';            // Transition not allowed

export interface TransitionResult {
  type: TransitionType;
  message?: string;
}

export interface ApplicantContext {
  boardDecision: string;
}

// Valid transitions map
const validTransitions: Record<Stage, Stage[]> = {
  Submitted: ['BoardApproved'],  // Can only go to BoardApproved via board decision
  BoardApproved: ['HouseHunting'],
  HouseHunting: ['UnderContract'],
  UnderContract: ['HouseHunting', 'Closed'],
  Closed: [], // Terminal state
};

export function validateTransition(
  fromStage: Stage,
  toStage: Stage,
  context: ApplicantContext
): TransitionResult {
  // Same stage - no change needed
  if (fromStage === toStage) {
    return { type: 'direct' };
  }

  // Check if transition is valid
  if (!validTransitions[fromStage]?.includes(toStage)) {
    // Special case: Submitted -> HouseHunting requires board approval
    if (fromStage === 'Submitted' && toStage === 'HouseHunting') {
      if (context.boardDecision !== 'Approved') {
        return {
          type: 'needsBoardApproval',
          message: 'Board approval is required before moving to House Hunting',
        };
      }
    }

    return {
      type: 'blocked',
      message: `Cannot move from ${formatStage(fromStage)} to ${formatStage(toStage)}`,
    };
  }

  // Submitted -> BoardApproved: Needs board approval
  if (fromStage === 'Submitted' && toStage === 'BoardApproved') {
    return {
      type: 'needsBoardApproval',
      message: 'Board approval is required to move this applicant forward',
    };
  }

  // BoardApproved -> HouseHunting: Need signed agreements (checked dynamically by modal)
  if (fromStage === 'BoardApproved' && toStage === 'HouseHunting') {
    return {
      type: 'needsAgreements',
      message: 'Signed agreements are required before starting House Hunting',
    };
  }

  // HouseHunting -> UnderContract: Need contract info
  if (fromStage === 'HouseHunting' && toStage === 'UnderContract') {
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

  // UnderContract -> HouseHunting: Contract failed
  if (fromStage === 'UnderContract' && toStage === 'HouseHunting') {
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
    BoardApproved: 'Board Approved',
    HouseHunting: 'House Hunting',
    UnderContract: 'Under Contract',
    Closed: 'Closed',
  };
  return names[stage] || stage;
}
