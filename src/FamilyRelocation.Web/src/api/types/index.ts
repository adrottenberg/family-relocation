// Auth types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  idToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface ChallengeResponse {
  challengeName: string;
  session: string;
  message: string;
  requiredFields: string[];
}

export interface RefreshTokenRequest {
  username: string;
  refreshToken: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
  idToken: string;
  expiresIn: number;
}

// Applicant types
export interface ApplicantDto {
  id: string;
  husband: PersonInfoDto;
  wife?: PersonInfoDto;
  address: AddressDto;
  children: ChildDto[];
  currentKehila?: string;
  shabbosShul?: string;
  boardReview: BoardReviewDto;
  housingSearch?: HousingSearchDto;
  createdDate: string;
  modifiedDate: string;
}

export interface PersonInfoDto {
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  occupation?: string;
}

export interface AddressDto {
  street: string;
  city: string;
  state: string;
  zipCode: string;
}

export interface ChildDto {
  name: string;
  age: number;
  gender: string;
  school?: string;
}

export interface BoardReviewDto {
  decision: string;
  reviewDate?: string;
  notes?: string;
  reviewedBy?: string;
}

export interface HousingSearchDto {
  id: string;
  stage: string;
  stageChangedDate: string;
  preferences?: HousingPreferencesDto;
  currentContract?: ContractDto;
  failedContractCount: number;
  brokerAgreementSigned: boolean;
  communityTakanosSigned: boolean;
}

export interface HousingPreferencesDto {
  budget?: MoneyDto;
  minBedrooms?: number;
  minBathrooms?: number;
  preferredCities?: string[];
  requiredFeatures?: string[];
  moveTimeline?: string;
  shulProximity?: ShulProximityDto;
}

export interface MoneyDto {
  amount: number;
  currency: string;
}

export interface ShulProximityDto {
  maxWalkingMinutes?: number;
  preferredShuls?: string[];
}

export interface ContractDto {
  propertyId: string;
  contractPrice: MoneyDto;
  contractDate: string;
  expectedClosingDate?: string;
  actualClosingDate?: string;
}

// Pipeline types
export interface PipelineResponse {
  stages: PipelineStageDto[];
  totalCount: number;
}

export interface PipelineStageDto {
  stage: string;
  count: number;
  items: PipelineItemDto[];
}

export interface PipelineItemDto {
  applicantId: string;
  housingSearchId: string;
  familyName: string;
  husbandFirstName: string;
  wifeFirstName?: string;
  childrenCount: number;
  boardDecision: string;
  stage: string;
  daysInStage: number;
  budget?: number;
  preferredCities?: string[];
  currentContractAddress?: string;
}

// Audit log types
export interface AuditLogDto {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  oldValues?: Record<string, unknown>;
  newValues?: Record<string, unknown>;
  userId?: string;
  userName?: string;
  timestamp: string;
}

// Paginated list
export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
