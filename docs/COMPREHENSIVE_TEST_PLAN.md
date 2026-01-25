# Comprehensive Test Plan - Family Relocation CRM v0.1.0

## Overview

This document provides a complete manual testing checklist for the Family Relocation CRM system. Tests are organized by feature area and indicate which PR introduced the functionality.

**Environment:** https://dev.unionvaad.com
**Test Data Guidelines:** Use test email addresses like `test+[name]@example.com` to avoid polluting production data.

---

## Table of Contents

1. [Authentication & Login](#1-authentication--login)
2. [User Management & RBAC](#2-user-management--rbac)
3. [Applicant Management](#3-applicant-management)
4. [Board Review Workflow](#4-board-review-workflow)
5. [Housing Search & Stage Transitions](#5-housing-search--stage-transitions)
6. [Document Management](#6-document-management)
7. [Property Management](#7-property-management)
8. [Property Matching](#8-property-matching)
9. [Showings](#9-showings)
10. [Shul & Walking Distances](#10-shul--walking-distances)
11. [Reminders](#11-reminders)
12. [Activity Logging](#12-activity-logging)
13. [Dashboard](#13-dashboard)
14. [Email Notifications](#14-email-notifications)
15. [Settings / Configuration](#15-settings--configuration)
16. [Security & Error Handling](#16-security--error-handling)
17. [Infrastructure & Deployment](#17-infrastructure--deployment)

---

## 1. Authentication & Login

**Related PRs:** #2 (Cognito Auth), #15 (Frontend Login), feature/ui-fixes (Login improvements)

### 1.1 Login Flow

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 1.1.1 | Valid login | Enter valid email/password, click Sign In | Redirects to dashboard | [ ] |
| 1.1.2 | Invalid credentials | Enter wrong password | Shows "Sign In Failed" error message, stays on login page | [ ] |
| 1.1.3 | Empty fields | Submit without email/password | Shows validation errors | [ ] |
| 1.1.4 | Invalid email format | Enter "notanemail", submit | Shows "Please enter a valid email" | [ ] |
| 1.1.5 | Email validation timing | Type partial email | Validation only triggers on blur (not while typing) | [ ] |
| 1.1.6 | Enter key submission | Press Enter in email or password field | Submits the form | [ ] |
| 1.1.7 | Remember me | Check "Remember me", login, close browser, reopen | Session should persist | [ ] |

### 1.2 New Password Required Challenge

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 1.2.1 | First login challenge | Login with temporary password | Shows "Password Change Required" info banner prominently | [ ] |
| 1.2.2 | Set new password | Enter new password and confirmation | Completes login, redirects to dashboard | [ ] |
| 1.2.3 | Password mismatch | Enter different passwords | Shows "Passwords do not match" error | [ ] |
| 1.2.4 | Back to login | Click "Back to Login" | Returns to main login form | [ ] |

### 1.3 Forgot Password

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 1.3.1 | Request reset code | Click "Forgot password?", enter email, submit | Shows success message, moves to code entry | [ ] |
| 1.3.2 | Enter verification code | Enter valid code from email | Allows entering new password | [ ] |
| 1.3.3 | Invalid code | Enter wrong verification code | Shows error message | [ ] |
| 1.3.4 | Reset password | Enter new password and confirmation | Shows success, returns to login | [ ] |

### 1.4 Token Refresh

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 1.4.1 | Auto refresh | Wait for access token to expire (~1 hour) | Automatically refreshes token, continues working | [ ] |
| 1.4.2 | Session expiry | Wait for refresh token to expire | Redirects to login page | [ ] |

### 1.5 Logout

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 1.5.1 | Logout | Click logout in sidebar | Clears tokens, redirects to login | [ ] |
| 1.5.2 | Post-logout access | After logout, try to access /dashboard | Redirects to login | [ ] |

---

## 2. User Management & RBAC

**Related PRs:** #26 (User Management & RBAC), feature/ui-fixes (Broker role)

### 2.1 User List (Admin Only)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 2.1.1 | View user list | Navigate to Settings → Users (as Admin) | Shows list of all users | [ ] |
| 2.1.2 | Search users | Enter search term | Filters users by name/email | [ ] |
| 2.1.3 | Non-admin access | Try to access /admin/users as Coordinator | Access denied or hidden | [ ] |

### 2.2 Create User (Admin Only)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 2.2.1 | Create user | Click "Add User", enter email, select roles | User created with temporary password displayed | [ ] |
| 2.2.2 | Copy credentials | Click "Copy Credentials" | Email and temp password copied to clipboard | [ ] |
| 2.2.3 | Duplicate email | Try to create user with existing email | Shows error | [ ] |

### 2.3 Update User Roles (Admin Only)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 2.3.1 | Edit roles | Click Edit Roles, check/uncheck roles | Roles updated successfully | [ ] |
| 2.3.2 | Available roles | Open Edit Roles modal | Shows Admin, Coordinator, BoardMember, Broker | [ ] |
| 2.3.3 | Remove own Admin | Try to remove Admin role from self | Should be prevented or show warning | [ ] |
| 2.3.4 | No roles warning | Remove all roles from a user | Shows warning about limited access | [ ] |

### 2.4 Deactivate/Reactivate User (Admin Only)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 2.4.1 | Deactivate user | Click Deactivate on a user | User shows as disabled | [ ] |
| 2.4.2 | Deactivated login | Try to login as deactivated user | Login fails | [ ] |
| 2.4.3 | Reactivate user | Click Reactivate on disabled user | User can login again | [ ] |
| 2.4.4 | Cannot deactivate self | Try to deactivate own account | Should be prevented | [ ] |

### 2.5 Role-Based Access Control

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 2.5.1 | Coordinator can edit applicants | Login as Coordinator, edit an applicant | Should succeed | [ ] |
| 2.5.2 | BoardMember cannot edit applicants | Login as BoardMember, try to edit | Should be denied or UI hidden | [ ] |
| 2.5.3 | BoardMember can set board decision | Login as BoardMember, set board decision | Should succeed | [ ] |
| 2.5.4 | Settings admin-only | Login as Coordinator, try to access Settings | Should be denied or hidden | [ ] |
| 2.5.5 | Broker limited access | Login as Broker | Can view properties but limited applicant info | [ ] |

---

## 3. Applicant Management

**Related PRs:** #5 (Create), #6 (View Details), #7 (Update), #8-9 (List/Search), #16 (Soft Delete), #21 (Stage changes)

### 3.1 Create Applicant

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 3.1.1 | Create with required fields | Enter husband first name, last name, email | Applicant created successfully | [ ] |
| 3.1.2 | Create with all fields | Fill all fields including wife, children, preferences | All data saved correctly | [ ] |
| 3.1.3 | Duplicate email check | Create applicant with existing email | Shows duplicate email error | [ ] |
| 3.1.4 | Invalid email format | Enter invalid email format | Validation error shown | [ ] |
| 3.1.5 | Phone formatting | Enter phone number | Displays formatted as (XXX) XXX-XXXX | [ ] |
| 3.1.6 | Auto-create HousingSearch | Create applicant | HousingSearch record created automatically | [ ] |

### 3.2 View Applicant List

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 3.2.1 | List applicants | Navigate to Applicants page | Shows paginated list | [ ] |
| 3.2.2 | Search by name | Enter family name in search | Filters to matching applicants | [ ] |
| 3.2.3 | Search by email | Enter email in search | Filters to matching applicants | [ ] |
| 3.2.4 | Search by phone | Enter phone number | Filters to matching applicants | [ ] |
| 3.2.5 | Filter by board status | Select board decision filter | Shows only matching applicants | [ ] |
| 3.2.6 | Filter by stage | Select stage filter | Shows only matching applicants | [ ] |
| 3.2.7 | Pagination | Navigate between pages | Shows correct records per page | [ ] |
| 3.2.8 | Sort by family name | Click column header | Sorts ascending/descending | [ ] |
| 3.2.9 | Sort by created date | Click column header | Sorts by date | [ ] |

### 3.3 View Applicant Details

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 3.3.1 | View details | Click on applicant row | Opens detail page with all info | [ ] |
| 3.3.2 | Overview tab | Click Overview tab | Shows family info, board status | [ ] |
| 3.3.3 | Housing Search tab | Click Housing Search tab | Shows preferences and current stage | [ ] |
| 3.3.4 | Children tab | Click Children tab | Lists all children with details | [ ] |
| 3.3.5 | Activity tab | Click Activity tab | Shows activity timeline | [ ] |
| 3.3.6 | Documents tab | Click Documents tab | Shows uploaded documents | [ ] |

### 3.4 Edit Applicant

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 3.4.1 | Open edit drawer | Click Edit button on detail page | Opens drawer with current data | [ ] |
| 3.4.2 | Edit husband info | Change husband's phone number | Saves successfully | [ ] |
| 3.4.3 | Edit wife info | Change wife's email | Saves successfully | [ ] |
| 3.4.4 | Add child | Add a new child | Child added to list | [ ] |
| 3.4.5 | Remove child | Remove an existing child | Child removed | [ ] |
| 3.4.6 | Cancel edit | Make changes, click Cancel | Changes not saved | [ ] |

### 3.5 Delete Applicant (Soft Delete)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 3.5.1 | Delete applicant | Click Delete, confirm | Applicant removed from list | [ ] |
| 3.5.2 | Deleted not in search | Search for deleted applicant | Should not appear | [ ] |
| 3.5.3 | Deleted not in pipeline | Check pipeline | Should not appear | [ ] |

---

## 4. Board Review Workflow

**Related PRs:** #14 (Board Review API), #16 (Board Review UI), #18 (AwaitingAgreements stage)

### 4.1 Set Board Decision

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 4.1.1 | Record Pending | Set decision to Pending | Status shows Pending | [ ] |
| 4.1.2 | Record Approved | Set decision to Approved | Creates HousingSearch, moves to AwaitingAgreements | [ ] |
| 4.1.3 | Record Rejected | Set decision to Rejected | Moves to Rejected stage | [ ] |
| 4.1.4 | Record Deferred | Set decision to Deferred | Stays in Submitted, can re-review | [ ] |
| 4.1.5 | Add review notes | Add notes with decision | Notes saved and displayed | [ ] |
| 4.1.6 | Set review date | Select past date | Date recorded correctly | [ ] |

### 4.2 Board Review from Pipeline

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 4.2.1 | Drag to HouseHunting without approval | Drag Submitted card to HouseHunting | Shows "Board Approval Required" modal | [ ] |
| 4.2.2 | Set decision in modal | Use modal to set decision to Approved | Moves card to appropriate column | [ ] |
| 4.2.3 | Rejected in modal | Set decision to Rejected | Card moves to Rejected column | [ ] |

### 4.3 Board Review UI on Detail Page

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 4.3.1 | Shows current status | View Submitted applicant | Shows "Pending" or current decision | [ ] |
| 4.3.2 | Record Decision button | Click "Record Board Decision" | Opens modal for decision entry | [ ] |
| 4.3.3 | After approval | View Approved applicant | Shows approval info and next steps | [ ] |

---

## 5. Housing Search & Stage Transitions

**Related PRs:** #11 (Stage Transitions), #12 (Preferences), #18 (Stage Separation), #20 (HousingSearchesController), #21 (Stage change from detail)

### 5.1 Stage Transitions

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 5.1.1 | Submitted → AwaitingAgreements | Board approves applicant | Auto-transitions to AwaitingAgreements | [ ] |
| 5.1.2 | AwaitingAgreements → Searching | Upload all required documents | Can transition to Searching | [ ] |
| 5.1.3 | Searching → UnderContract | Enter contract details | Transitions with contract info | [ ] |
| 5.1.4 | UnderContract → Closed | Enter closing date | Marks as Closed | [ ] |
| 5.1.5 | Closed → MovedIn | Enter move-in date | Marks as MovedIn | [ ] |
| 5.1.6 | UnderContract → Searching | Contract falls through | Records failed contract, returns to Searching | [ ] |
| 5.1.7 | Any → Paused | Pause a search | Status changes to Paused | [ ] |
| 5.1.8 | Paused → Previous | Resume from Paused | Returns to previous stage | [ ] |

### 5.2 Pipeline Kanban

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 5.2.1 | View pipeline | Navigate to Pipeline page | Shows 6 columns with cards | [ ] |
| 5.2.2 | Drag valid transition | Drag card to valid next stage | Shows appropriate modal if needed | [ ] |
| 5.2.3 | Drag invalid transition | Drag card to invalid stage | Shows error modal | [ ] |
| 5.2.4 | Contract info modal | Drag to UnderContract | Shows contract info form | [ ] |
| 5.2.5 | Closing modal | Drag to Closed | Shows closing date form | [ ] |
| 5.2.6 | Contract failed modal | Drag UnderContract to Searching | Shows failed contract reason form | [ ] |
| 5.2.7 | Search pipeline | Enter search term | Filters cards | [ ] |
| 5.2.8 | Closed/MovedIn limits | View Closed and MovedIn columns | Shows max 10 recent, "View all" link | [ ] |

### 5.3 Stage Change from Detail Page

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 5.3.1 | Stage dropdown | Click dropdown next to stage tag | Shows valid transitions | [ ] |
| 5.3.2 | Change stage | Select new stage | Opens appropriate modal | [ ] |
| 5.3.3 | Invalid options hidden | Check dropdown options | Only shows valid transitions | [ ] |

### 5.4 Housing Preferences

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 5.4.1 | Update budget | Change budget amount | Saves and recalculates matches | [ ] |
| 5.4.2 | Update bedrooms | Change minimum bedrooms | Saves and recalculates matches | [ ] |
| 5.4.3 | Update city preference | Select preferred city | Saves correctly | [ ] |
| 5.4.4 | Update features | Add/remove required features | Saves correctly | [ ] |
| 5.4.5 | Update shul proximity | Change max walking distance | Saves correctly | [ ] |

---

## 6. Document Management

**Related PRs:** #16 (S3 Upload), #17 (Document Types Refactoring), #29 (S3 Image Proxy)

### 6.1 Document Upload

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 6.1.1 | Upload PDF | Upload a PDF document | Saves to S3, shows in list | [ ] |
| 6.1.2 | Upload image | Upload JPG/PNG | Saves to S3, shows in list | [ ] |
| 6.1.3 | File too large | Upload >10MB file | Shows error message | [ ] |
| 6.1.4 | Invalid file type | Upload .exe or .zip | Shows error message | [ ] |
| 6.1.5 | Document naming | Upload a document | Named as {DocType}_{FamilyName}_{timestamp}.{ext} | [ ] |

### 6.2 Document Viewing

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 6.2.1 | View document | Click View/Download button | Opens document (pre-signed URL) | [ ] |
| 6.2.2 | View property photos | Go to property detail | Photos load via S3 proxy | [ ] |
| 6.2.3 | Delete document | Click Delete on document | Document removed from list | [ ] |

### 6.3 Stage Requirements

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 6.3.1 | Required documents shown | Try to transition stage | Shows required documents modal | [ ] |
| 6.3.2 | Block without docs | Try to proceed without required docs | Transition blocked until uploaded | [ ] |
| 6.3.3 | Auto-transition | Upload last required document | Automatically offers to proceed | [ ] |

---

## 7. Property Management

**Related PRs:** #19 (Property CRUD), #28 (Primary Photo)

### 7.1 Create Property

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 7.1.1 | Create with required fields | Enter address, city, price | Property created | [ ] |
| 7.1.2 | All fields | Enter all details including features | All data saved | [ ] |
| 7.1.3 | Upload photos | Add property photos | Photos uploaded to S3 | [ ] |
| 7.1.4 | Photo limit | Try to upload more than 50 photos | Limited to 50 | [ ] |

### 7.2 Property List

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 7.2.1 | View property list | Navigate to Properties page | Shows paginated list | [ ] |
| 7.2.2 | Filter by city | Select city filter | Shows only matching properties | [ ] |
| 7.2.3 | Filter by status | Select status filter | Shows only matching status | [ ] |
| 7.2.4 | Filter by price range | Enter min/max price | Filters by price | [ ] |
| 7.2.5 | Filter by bedrooms | Select bedrooms | Filters by bedrooms | [ ] |
| 7.2.6 | Search by address | Enter address search | Filters by address | [ ] |

### 7.3 Property Details

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 7.3.1 | View details | Click property row | Shows all property info | [ ] |
| 7.3.2 | Photo gallery | View photos section | Shows all photos, primary highlighted | [ ] |
| 7.3.3 | Walking distances | View "Walking to Shuls" section | Shows distances sorted by proximity | [ ] |
| 7.3.4 | Matched applicants | View matches section | Shows families matched to this property | [ ] |

### 7.4 Update Property

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 7.4.1 | Edit property | Click Edit, change fields | Saves successfully | [ ] |
| 7.4.2 | Change status | Update status to Sold | Status changes, removed from active matching | [ ] |
| 7.4.3 | Set primary photo | Click star on a photo | Photo becomes primary | [ ] |
| 7.4.4 | Delete photo | Delete a photo | Photo removed | [ ] |

### 7.5 Delete Property (Soft Delete)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 7.5.1 | Delete property | Click Delete, confirm | Property removed from list | [ ] |

---

## 8. Property Matching

**Related PRs:** #28 (Property Matching), #29 (Algorithm Adjustments), #30 (Score Threshold)

### 8.1 Auto-Matching

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 8.1.1 | New property triggers match | Create new property | Matches created for active searches with score > 70 | [ ] |
| 8.1.2 | Preference update triggers recalc | Update housing preferences | Match scores recalculated | [ ] |
| 8.1.3 | No-preference baseline | Applicant with no preferences | Scores around 46 (not 59) | [ ] |
| 8.1.4 | High score threshold | Property scores 75 | Auto-matched | [ ] |
| 8.1.5 | Low score not auto-matched | Property scores 65 | Not auto-matched (can manually match) | [ ] |

### 8.2 Match Score Calculation

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 8.2.1 | Budget fit (30 pts max) | Property at budget | Full 30 points | [ ] |
| 8.2.2 | Budget over | Property exceeds budget by 20% | Reduced points | [ ] |
| 8.2.3 | Bedrooms match (20 pts max) | Property meets bedroom requirement | Full 20 points | [ ] |
| 8.2.4 | Bathrooms match (15 pts max) | Property meets bathroom requirement | Full 15 points | [ ] |
| 8.2.5 | City match (20 pts max) | Property in preferred city | Full 20 points | [ ] |
| 8.2.6 | Features match (15 pts max) | Property has required features | Proportional points | [ ] |

### 8.3 Manual Matching

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 8.3.1 | Create manual match | Manually match property to search | Match created with calculated score | [ ] |
| 8.3.2 | Match low-score property | Match property with score < 70 | Works (manual override) | [ ] |
| 8.3.3 | View matches | Open matches for housing search | Shows all matches sorted by score | [ ] |

### 8.4 Match Status Workflow

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 8.4.1 | MatchIdentified → ShowingRequested | Request showing | Status changes | [ ] |
| 8.4.2 | ShowingRequested → ApplicantInterested | Mark interested after showing | Status changes | [ ] |
| 8.4.3 | ShowingRequested → Rejected | Mark not interested | Status changes | [ ] |
| 8.4.4 | ApplicantInterested → OfferMade | Make offer | Status changes | [ ] |

---

## 9. Showings

**Related PRs:** #28 (Showings Feature)

### 9.1 Schedule Showing

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 9.1.1 | Schedule new showing | Select applicant, property, date/time | Showing created | [ ] |
| 9.1.2 | Auto-create match | Schedule showing without existing match | Creates PropertyMatch automatically | [ ] |
| 9.1.3 | Past date validation | Try to schedule in the past | Shows error | [ ] |
| 9.1.4 | Add notes | Add showing notes | Notes saved | [ ] |

### 9.2 View Showings

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 9.2.1 | Showings list | Navigate to Showings page | Shows all showings | [ ] |
| 9.2.2 | Filter by status | Select status filter | Filters showings | [ ] |
| 9.2.3 | Filter by date | Select date range | Filters showings | [ ] |

### 9.3 Showing Status Updates

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 9.3.1 | Mark completed | Mark showing as Completed | Status updates | [ ] |
| 9.3.2 | Mark cancelled | Cancel showing | Status updates to Cancelled | [ ] |
| 9.3.3 | Mark no-show | Mark as NoShow | Status updates | [ ] |
| 9.3.4 | Reschedule | Reschedule showing | New date saved, audit trail preserved | [ ] |

---

## 10. Shul & Walking Distances

**Related PRs:** #28 (Shul Distance Feature)

### 10.1 Shul Management

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 10.1.1 | Create shul | Enter name and address | Shul created | [ ] |
| 10.1.2 | Auto-geocode | Create shul without coordinates | Address geocoded automatically | [ ] |
| 10.1.3 | Distance calculation | Create shul | Distances calculated for all properties | [ ] |
| 10.1.4 | Update shul | Edit shul address | Distances recalculated | [ ] |
| 10.1.5 | Delete shul | Delete a shul | Shul and distances removed | [ ] |

### 10.2 Distance Display

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 10.2.1 | Property distances | View property detail | Shows walking times to all shuls | [ ] |
| 10.2.2 | Sorted by proximity | View distance list | Sorted closest first | [ ] |
| 10.2.3 | New property distances | Create new property | Distances calculated to all shuls | [ ] |

---

## 11. Reminders

**Related PRs:** #23 (Reminders Backend), #24 (Reminders Frontend)

### 11.1 Create Reminder

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 11.1.1 | Create general reminder | Create reminder without entity | Reminder created | [ ] |
| 11.1.2 | Create applicant reminder | Create from applicant page | Entity context auto-filled | [ ] |
| 11.1.3 | Set priority | Set priority to Urgent | Priority saved | [ ] |
| 11.1.4 | Set due time | Set specific time | Time saved | [ ] |
| 11.1.5 | Past date validation | Try to set past due date | Shows error | [ ] |
| 11.1.6 | Email notification | Check send email option | Option saved | [ ] |

### 11.2 View Reminders

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 11.2.1 | Reminders page | Navigate to /reminders | Shows reminder list | [ ] |
| 11.2.2 | Status tabs | Switch between All/Open/Snoozed/Completed | Filters correctly | [ ] |
| 11.2.3 | Filter by priority | Select priority filter | Filters by priority | [ ] |
| 11.2.4 | Filter by entity type | Select entity type | Filters correctly | [ ] |
| 11.2.5 | Overdue highlighting | View overdue reminder | Shows in red | [ ] |
| 11.2.6 | Due today highlighting | View reminder due today | Shows in orange | [ ] |

### 11.3 Reminder Actions

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 11.3.1 | Complete reminder | Click Complete | Status changes to Completed | [ ] |
| 11.3.2 | Snooze - Tomorrow | Snooze until tomorrow | Due date updated | [ ] |
| 11.3.3 | Snooze - Next week | Snooze 1 week | Due date updated | [ ] |
| 11.3.4 | Snooze - Custom | Snooze to custom date | Due date updated | [ ] |
| 11.3.5 | Dismiss reminder | Click Dismiss | Status changes to Dismissed | [ ] |
| 11.3.6 | Reopen reminder | Reopen completed reminder | Status changes to Open | [ ] |

### 11.4 Print View

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 11.4.1 | Print due reminders | Click Print button | Opens print view | [ ] |
| 11.4.2 | Grouped by priority | View print layout | Grouped by Urgent/High/Normal/Low | [ ] |
| 11.4.3 | Print format | Print the page | Prints cleanly | [ ] |

---

## 12. Activity Logging

**Related PRs:** #13 (Audit Log), #19 (Activity Log), #25 (Communication Logging)

### 12.1 Automatic Activity Logging

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 12.1.1 | Applicant created | Create an applicant | Activity logged | [ ] |
| 12.1.2 | Applicant updated | Update an applicant | Activity logged | [ ] |
| 12.1.3 | Board decision | Set board decision | Activity logged | [ ] |
| 12.1.4 | Stage changed | Change housing search stage | Activity logged | [ ] |
| 12.1.5 | Property created | Create a property | Activity logged | [ ] |
| 12.1.6 | Property updated | Update a property | Activity logged | [ ] |

### 12.2 Manual Activity Logging

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 12.2.1 | Log phone call | Click "Log Activity", select Phone Call | Activity logged with duration/outcome | [ ] |
| 12.2.2 | Log note | Log a note | Activity logged | [ ] |
| 12.2.3 | Create follow-up | Check "Create follow-up" when logging | Reminder created | [ ] |
| 12.2.4 | Call outcome | Select call outcome (Connected, Voicemail, etc.) | Saved correctly | [ ] |

### 12.3 View Activity

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 12.3.1 | Activity tab | View Activity tab on applicant | Shows timeline | [ ] |
| 12.3.2 | Activity icons | View different activity types | Shows appropriate icons | [ ] |
| 12.3.3 | Recent activities | View dashboard | Shows recent activity feed | [ ] |

### 12.4 Audit Log (Admin)

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 12.4.1 | View audit history | View History tab on applicant (Admin) | Shows all changes | [ ] |
| 12.4.2 | Old/new values | Expand audit entry | Shows what changed | [ ] |
| 12.4.3 | Filter by date | Filter audit logs by date range | Filters correctly | [ ] |

---

## 13. Dashboard

**Related PRs:** #19 (Dashboard API)

### 13.1 Dashboard Statistics

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 13.1.1 | View dashboard | Navigate to /dashboard | Shows statistics cards | [ ] |
| 13.1.2 | Applicant counts by stage | View stage breakdown | Shows correct counts | [ ] |
| 13.1.3 | Board decision counts | View board status | Shows correct counts | [ ] |
| 13.1.4 | Property counts | View property stats | Shows correct counts by status | [ ] |

### 13.2 Recent Activity Feed

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 13.2.1 | Recent activities | View activity feed | Shows recent system activity | [ ] |
| 13.2.2 | Activity links | Click activity item | Navigates to related entity | [ ] |

---

## 14. Email Notifications

**Related PRs:** #19 (Email Notifications)

### 14.1 Application Emails

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 14.1.1 | Application received | Submit new application | Confirmation email sent (if SES verified) | [ ] |
| 14.1.2 | Board approved | Approve applicant | Approval email sent | [ ] |
| 14.1.3 | Board rejected | Reject applicant | Rejection email sent | [ ] |

### 14.2 Stage Change Emails

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 14.2.1 | Under contract | Move to UnderContract | Notification email sent | [ ] |
| 14.2.2 | Closed | Move to Closed | Congratulations email sent | [ ] |

**Note:** SES requires email verification in sandbox mode. Production requires SES production access.

---

## 15. Settings / Configuration

**Related PRs:** #17 (Document Types & Stage Requirements)

### 15.1 Document Types

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 15.1.1 | View document types | Navigate to Settings → Document Types | Shows all types | [ ] |
| 15.1.2 | Create document type | Add new type | Type created | [ ] |
| 15.1.3 | Edit document type | Edit existing type | Changes saved | [ ] |
| 15.1.4 | Deactivate type | Deactivate a type | Type no longer available for upload | [ ] |
| 15.1.5 | System types protected | Try to delete system type | Should be prevented | [ ] |

### 15.2 Stage Requirements

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 15.2.1 | View requirements | Navigate to Settings → Stage Requirements | Shows requirements by stage | [ ] |
| 15.2.2 | Add requirement | Add document type to stage | Requirement added | [ ] |
| 15.2.3 | Delete requirement | Remove requirement | Requirement removed | [ ] |
| 15.2.4 | Test enforcement | Try stage transition | Requires documents per config | [ ] |

---

## 16. Security & Error Handling

**Related PRs:** #22 (Code Review Fixes), #27 (Security Fixes)

### 16.1 Error Handling

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 16.1.1 | Validation error | Submit invalid data | Returns 400 with friendly message (no stack trace) | [ ] |
| 16.1.2 | Not found | Request non-existent ID | Returns 404 with message | [ ] |
| 16.1.3 | Unauthorized | Access without token | Returns 401 | [ ] |
| 16.1.4 | Forbidden | Access without required role | Returns 403 | [ ] |

### 16.2 Input Validation

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 16.2.1 | Required fields | Submit without required fields | Validation errors shown | [ ] |
| 16.2.2 | Email format | Enter invalid email | Validation error | [ ] |
| 16.2.3 | Empty GUID | Submit empty GUID | Validation error | [ ] |

### 16.3 Pagination Limits

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 16.3.1 | Max page size | Request pageSize=1000 | Limited to 100 | [ ] |
| 16.3.2 | Max page number | Request page=5000 | Limited to 1000 | [ ] |

### 16.4 CORS

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 16.4.1 | Allowed origin | Request from dev.unionvaad.com | CORS allowed | [ ] |
| 16.4.2 | Disallowed origin | Request from unknown origin (prod) | CORS blocked | [ ] |

---

## 17. Infrastructure & Deployment

**Related PRs:** #31 (Deployment Config)

### 17.1 DNS & SSL

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 17.1.1 | DNS resolution | Visit dev.unionvaad.com | Site loads | [ ] |
| 17.1.2 | SSL certificate | Check certificate | Valid SSL (ACM) | [ ] |
| 17.1.3 | HTTPS redirect | Visit http://dev.unionvaad.com | Redirects to HTTPS | [ ] |

### 17.2 Health Checks

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 17.2.1 | API health | GET /health | Returns "Healthy" | [ ] |
| 17.2.2 | Database connectivity | Check health endpoint | Database connected | [ ] |

### 17.3 Deployment

| # | Test Case | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 17.3.1 | Push to develop | Push code to develop branch | GitHub Actions deploys to dev | [ ] |
| 17.3.2 | Frontend deployed | Check CloudFront | New build deployed | [ ] |
| 17.3.3 | API deployed | Check EC2 | New Docker image running | [ ] |

---

## Test Execution Summary

| Section | Total Tests | Passed | Failed | Blocked |
|---------|-------------|--------|--------|---------|
| 1. Authentication & Login | 17 | | | |
| 2. User Management & RBAC | 16 | | | |
| 3. Applicant Management | 25 | | | |
| 4. Board Review Workflow | 11 | | | |
| 5. Housing Search & Stage Transitions | 20 | | | |
| 6. Document Management | 11 | | | |
| 7. Property Management | 18 | | | |
| 8. Property Matching | 13 | | | |
| 9. Showings | 11 | | | |
| 10. Shul & Walking Distances | 8 | | | |
| 11. Reminders | 16 | | | |
| 12. Activity Logging | 12 | | | |
| 13. Dashboard | 4 | | | |
| 14. Email Notifications | 5 | | | |
| 15. Settings / Configuration | 9 | | | |
| 16. Security & Error Handling | 9 | | | |
| 17. Infrastructure & Deployment | 6 | | | |
| **TOTAL** | **201** | | | |

---

## Bug Report Template

When a test fails, document using this format:

```
### Bug #XXX: [Brief Title]

**Test Case:** [Test case number and name]
**Severity:** Critical / High / Medium / Low
**Environment:** dev.unionvaad.com / local

**Steps to Reproduce:**
1. Step one
2. Step two
3. ...

**Expected Result:**
What should happen

**Actual Result:**
What actually happened

**Screenshots:**
[If applicable]

**Additional Notes:**
[Browser, any error messages, etc.]
```

---

*Last Updated: January 25, 2026*
*Version: v0.1.0-dev*
