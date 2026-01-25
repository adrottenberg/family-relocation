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

// Application status - tracks applicant's progress through approval process
export type ApplicationStatus = 'Submitted' | 'Approved' | 'Rejected';

// Housing search stage - tracks approved applicant's searching journey
export type HousingSearchStage = 'AwaitingAgreements' | 'Searching' | 'UnderContract' | 'Closed' | 'MovedIn' | 'Paused';

// Pipeline stage - combined view for the pipeline UI
export type PipelineStage = 'Submitted' | 'AwaitingAgreements' | 'Searching' | 'UnderContract' | 'Closed';

// Applicant list item (lightweight for list views)
export interface ApplicantListItemDto {
  id: string;
  husbandFullName: string;
  wifeMaidenName?: string;
  husbandEmail?: string;
  husbandPhone?: string;
  boardDecision?: string;
  createdDate: string;
  stage?: string; // HousingSearchStage when approved, undefined when not
  housingSearchId?: string;
}

// Applicant full detail
export interface ApplicantDto {
  id: string;
  status: ApplicationStatus; // Submitted, Approved, or Rejected
  husband: HusbandInfoDto;
  wife?: SpouseInfoDto;
  address?: AddressDto;
  children?: ChildDto[];
  currentKehila?: string;
  shabbosShul?: string;
  familyName: string;
  numberOfChildren: number;
  isPendingBoardReview: boolean;
  isSelfSubmitted: boolean;
  boardReview?: BoardReviewDto;
  housingSearch?: HousingSearchDto; // Active housing search (if approved)
  allHousingSearches?: HousingSearchDto[]; // All housing searches (active first, then inactive by date)
  createdDate: string;
}

export interface HusbandInfoDto {
  firstName: string;
  lastName: string;
  fatherName?: string;
  email?: string;
  phoneNumbers?: PhoneNumberDto[];
  occupation?: string;
  employerName?: string;
}

export interface SpouseInfoDto {
  firstName: string;
  maidenName?: string;
  fatherName?: string;
  email?: string;
  phoneNumbers?: PhoneNumberDto[];
  occupation?: string;
  employerName?: string;
  highSchool?: string;
}

export interface PhoneNumberDto {
  number: string;
  type: string;
  isPrimary: boolean;
}

export interface AddressDto {
  street: string;
  street2?: string;
  city: string;
  state: string;
  zipCode: string;
  fullAddress?: string;
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
}

export interface HousingSearchDto {
  id: string;
  stage: string;
  stageChangedDate: string;
  preferences?: HousingPreferencesDto;
  currentContract?: ContractDto;
  failedContractCount: number;
  notes?: string;
  isActive: boolean;
  createdDate: string;
}

// Document types
export interface DocumentTypeDto {
  id: string;
  name: string;
  displayName: string;
  description?: string;
  isActive: boolean;
  isSystemType: boolean;
}

export interface ApplicantDocumentDto {
  id: string;
  documentTypeId: string;
  documentTypeName: string;
  fileName: string;
  storageKey: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedAt: string;
  uploadedBy?: string;
}

export interface StageTransitionRequirementsDto {
  fromStage: string;
  toStage: string;
  requirements: DocumentRequirementDto[];
}

export interface StageTransitionRequirementDto {
  id: string;
  fromStage: string;
  toStage: string;
  documentTypeId: string;
  documentTypeName: string;
  isRequired: boolean;
}

export interface DocumentRequirementDto {
  documentTypeId: string;
  documentTypeName: string;
  isRequired: boolean;
  isUploaded: boolean;
}

export interface HousingPreferencesDto {
  budgetAmount?: number;
  minBedrooms?: number;
  minBathrooms?: number;
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
  preferredShulIds?: string[];
}

export interface ContractDto {
  propertyId?: string;
  price: number;
  contractDate: string;
  expectedClosingDate?: string;
  actualClosingDate?: string;
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
  userEmail?: string;
  timestamp: string;
  resolvedNames?: Record<string, string>;
  entityDescription?: string;
}

// Create Applicant Request - matches backend CreateApplicantCommand
export interface CreateApplicantRequest {
  husband: HusbandInfoDto;
  wife?: SpouseInfoDto;
  address?: AddressDto;
  children?: ChildDto[];
  currentKehila?: string;
  shabbosShul?: string;
  housingPreferences?: HousingPreferencesDto;
}

// Paginated list
export interface PaginatedList<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Property types
export type ListingStatus = 'Active' | 'UnderContract' | 'Sold' | 'OffMarket';

export interface PropertyDto {
  id: string;
  address: PropertyAddressDto;
  price: number;
  bedrooms: number;
  bathrooms: number;
  squareFeet?: number;
  lotSize?: number;
  yearBuilt?: number;
  annualTaxes?: number;
  features: string[];
  status: string;
  mlsNumber?: string;
  notes?: string;
  photos: PropertyPhotoDto[];
  createdAt: string;
  modifiedAt?: string;
}

export interface PropertyListDto {
  id: string;
  street: string;
  city: string;
  price: number;
  bedrooms: number;
  bathrooms: number;
  squareFeet?: number;
  status: string;
  mlsNumber?: string;
  primaryPhotoUrl?: string;
}

export interface PropertyAddressDto {
  street: string;
  street2?: string;
  city: string;
  state: string;
  zipCode: string;
  fullAddress?: string;
}

export interface PropertyPhotoDto {
  id: string;
  url: string;
  description?: string;
  displayOrder: number;
  isPrimary: boolean;
  uploadedAt: string;
}

// Dashboard types
export interface DashboardStatsDto {
  applicants: ApplicantStatsDto;
  properties: PropertyStatsDto;
}

export interface ApplicantStatsDto {
  total: number;
  byBoardDecision: Record<string, number>;
  byStage: Record<string, number>;
}

export interface PropertyStatsDto {
  total: number;
  byStatus: Record<string, number>;
}

// Activity types are defined in endpoints/activities.ts

// Property Match types
export type PropertyMatchStatus = 'MatchIdentified' | 'ShowingRequested' | 'ApplicantInterested' | 'OfferMade' | 'ApplicantRejected';

export interface PropertyMatchDto {
  id: string;
  housingSearchId: string;
  propertyId: string;
  status: PropertyMatchStatus;
  matchScore: number;
  matchDetails?: MatchScoreBreakdownDto;
  notes?: string;
  isAutoMatched: boolean;
  createdAt: string;
  modifiedAt?: string;
  property: PropertyListDto;
  applicant: MatchApplicantDto;
}

export interface MatchShowingDto {
  id: string;
  scheduledDate: string;
  scheduledTime: string;
  status: string;
  brokerUserId?: string;
  brokerUserName?: string;
  notes?: string;
  completedAt?: string;
}

export interface PropertyMatchListDto {
  id: string;
  housingSearchId: string;
  propertyId: string;
  status: PropertyMatchStatus;
  matchScore: number;
  isAutoMatched: boolean;
  createdAt: string;
  propertyStreet: string;
  propertyCity: string;
  propertyPrice: number;
  propertyBedrooms: number;
  propertyBathrooms: number;
  propertyPhotoUrl?: string;
  applicantId: string;
  applicantName: string;
  // All showings for this match
  showings: MatchShowingDto[];
  // Convenience properties - first scheduled showing (computed on backend)
  scheduledShowingDate?: string;
  scheduledShowingTime?: string;
}

export interface MatchScoreBreakdownDto {
  budgetScore: number;
  maxBudgetScore: number;
  budgetNotes?: string;
  bedroomsScore: number;
  maxBedroomsScore: number;
  bedroomsNotes?: string;
  bathroomsScore: number;
  maxBathroomsScore: number;
  bathroomsNotes?: string;
  cityScore: number;
  maxCityScore: number;
  cityNotes?: string;
  featuresScore: number;
  maxFeaturesScore: number;
  featuresNotes?: string;
  totalScore: number;
  maxTotalScore: number;
}

export interface MatchApplicantDto {
  id: string;
  familyName: string;
  husbandFirstName?: string;
  wifeFirstName?: string;
}

// Showing types
export type ShowingStatus = 'Scheduled' | 'Completed' | 'Cancelled' | 'NoShow';

export interface ShowingDto {
  id: string;
  propertyMatchId: string;
  scheduledDate: string;
  scheduledTime: string;
  status: ShowingStatus;
  notes?: string;
  brokerUserId?: string;
  brokerUserName?: string;
  createdAt: string;
  modifiedAt?: string;
  completedAt?: string;
  propertyId: string;
  propertyStreet: string;
  propertyCity: string;
  propertyPrice: number;
  propertyPhotoUrl?: string;
  applicantId: string;
  applicantName: string;
}

export interface ShowingListDto {
  id: string;
  propertyMatchId: string;
  scheduledDate: string;
  scheduledTime: string;
  status: ShowingStatus;
  brokerUserId?: string;
  propertyId: string;
  propertyStreet: string;
  propertyCity: string;
  propertyPhotoUrl?: string;
  applicantId: string;
  applicantName: string;
}

// Reminder types
export type ReminderPriority = 'Low' | 'Normal' | 'High' | 'Urgent';
export type ReminderStatus = 'Open' | 'Completed' | 'Snoozed' | 'Dismissed';

export interface ReminderDto {
  id: string;
  title: string;
  notes?: string;
  dueDate: string;
  dueTime?: string;
  priority: ReminderPriority;
  entityType: string;
  entityId: string;
  entityDisplayName?: string;
  assignedToUserId?: string;
  assignedToUserName?: string;
  status: ReminderStatus;
  sendEmailNotification: boolean;
  snoozedUntil?: string;
  snoozeCount: number;
  createdAt: string;
  createdBy: string;
  createdByName?: string;
  completedAt?: string;
  completedBy?: string;
  completedByName?: string;
  isOverdue: boolean;
  isDueToday: boolean;
  // Source activity info (if reminder was created from an activity)
  sourceActivityId?: string;
  sourceActivityType?: string;
  sourceActivityTimestamp?: string;
}

export interface ReminderListDto {
  id: string;
  title: string;
  dueDate: string;
  dueTime?: string;
  priority: ReminderPriority;
  status: ReminderStatus;
  entityType: string;
  entityId: string;
  entityDisplayName?: string;
  isOverdue: boolean;
  isDueToday: boolean;
  snoozeCount: number;
}

export interface DueRemindersReportDto {
  asOfDate: string;
  overdue: ReminderListDto[];
  dueToday: ReminderListDto[];
  upcoming: ReminderListDto[];
  overdueCount: number;
  dueTodayCount: number;
  upcomingCount: number;
  totalOpenCount: number;
}

// Shul types
export interface ShulDto {
  id: string;
  name: string;
  address: ShulAddressDto;
  location?: ShulCoordinatesDto;
  rabbi?: string;
  denomination?: string;
  website?: string;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  modifiedAt?: string;
}

export interface ShulListDto {
  id: string;
  name: string;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  rabbi?: string;
  denomination?: string;
  isActive: boolean;
  latitude?: number;
  longitude?: number;
}

export interface ShulAddressDto {
  street: string;
  street2?: string;
  city: string;
  state: string;
  zipCode: string;
  fullAddress?: string;
}

export interface ShulCoordinatesDto {
  latitude: number;
  longitude: number;
}

export interface PropertyShulDistanceDto {
  shulId: string;
  shulName: string;
  distanceMiles: number;
  walkingTimeMinutes: number;
  calculatedAt: string;
}
