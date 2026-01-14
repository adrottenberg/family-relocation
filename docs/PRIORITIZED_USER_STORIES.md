# PRIORITIZED USER STORIES - FAMILY RELOCATION CRM
## MoSCoW Prioritization Framework

**Priority Levels:**
- **P0 - MUST HAVE (MVP)** - Cannot launch without these
- **P1 - SHOULD HAVE (Phase 2)** - Important but can launch without
- **P2 - COULD HAVE (Phase 3)** - Nice to have, add later
- **P3 - WON'T HAVE (Now)** - Future consideration

---

## EPIC 1: APPLICATION MANAGEMENT

### ✅ P0 - MUST HAVE (MVP)

**1.1 Submit Application - P0**
- **Why P0:** Core function - replaces Google Form
- **Dependencies:** None
- **Sprint:** 1
- **Effort:** 5 points

**1.2 View Applications Under Review - P0**
- **Why P0:** Staff needs to see applications
- **Dependencies:** 1.1
- **Sprint:** 1
- **Effort:** 3 points

**1.3 Board Reviews Application - P0**
- **Why P0:** Core approval workflow
- **Dependencies:** 1.2
- **Sprint:** 2
- **Effort:** 5 points

**1.4 Auto-Reject on Board Disapproval - P0**
- **Why P0:** Prevents manual work, ensures consistency
- **Dependencies:** 1.3
- **Sprint:** 2
- **Effort:** 3 points

**1.5 Approve Application and Send Paperwork - P0**
- **Why P0:** Completes approval workflow
- **Dependencies:** 1.3
- **Sprint:** 2
- **Effort:** 5 points

**Epic 1 Total: P0 - 5 stories, 21 points**

---

## EPIC 2: CONTACT MANAGEMENT

### ✅ P0 - MUST HAVE (MVP)

**2.1 View Contact List - P0**
- **Why P0:** Basic navigation requirement
- **Dependencies:** 1.1
- **Sprint:** 1
- **Effort:** 3 points

**2.2 View Contact Details - P0**
- **Why P0:** Must see complete family info
- **Dependencies:** 2.1
- **Sprint:** 1
- **Effort:** 5 points

**2.3 Edit Contact Information - P0**
- **Why P0:** Info changes, must update
- **Dependencies:** 2.2
- **Sprint:** 2
- **Effort:** 3 points

### 🟡 P1 - SHOULD HAVE (Phase 2)

**2.4 Collect Housing Preferences - P1**
- **Why P1:** Important but can do manually initially
- **Dependencies:** 2.3
- **Sprint:** 4
- **Effort:** 5 points
- **Workaround:** Coordinator can enter manually

**2.5 Add Notes to Contact - P1**
- **Why P1:** Helpful but not blocking
- **Dependencies:** 2.2
- **Sprint:** 5
- **Effort:** 3 points
- **Workaround:** Use email/external notes initially

**Epic 2 Total:**
- **P0:** 3 stories, 11 points
- **P1:** 2 stories, 8 points

---

## EPIC 3: DEAL PIPELINE MANAGEMENT

### ✅ P0 - MUST HAVE (MVP)

**3.1 View Deal Pipeline (Kanban) - P0**
- **Why P0:** Core UX for managing deals
- **Dependencies:** 1.1, 1.3
- **Sprint:** 2
- **Effort:** 8 points
- **Note:** Most complex UI component

**3.2 Change Deal Stage - P0**
- **Why P0:** Core workflow
- **Dependencies:** 3.1
- **Sprint:** 2
- **Effort:** 5 points

**3.3 View Deal Details - P0**
- **Why P0:** Must see all deal info
- **Dependencies:** 3.2
- **Sprint:** 2
- **Effort:** 5 points

### 🟡 P1 - SHOULD HAVE (Phase 2)

**3.4 Track Commission - P1**
- **Why P1:** Important but can track in spreadsheet initially
- **Dependencies:** 3.3
- **Sprint:** 6
- **Effort:** 5 points
- **Workaround:** Excel for commission tracking

**3.5 Close Deal and Track Move-In - P1**
- **Why P1:** Post-closing tracking less urgent
- **Dependencies:** 3.2
- **Sprint:** 6
- **Effort:** 3 points
- **Workaround:** Mark as closed, track move-in manually

**Epic 3 Total:**
- **P0:** 3 stories, 18 points
- **P1:** 2 stories, 8 points

---

## EPIC 4: PROPERTY LISTINGS MANAGEMENT

### 🟡 P1 - SHOULD HAVE (Phase 2)

**4.1 Add Property Listing - P1**
- **Why P1:** Can launch with manual matching first
- **Dependencies:** None
- **Sprint:** 4
- **Effort:** 5 points
- **Note:** Need this before auto-matching

**4.2 View Property List - P1**
- **Why P1:** Needed for property management
- **Dependencies:** 4.1
- **Sprint:** 4
- **Effort:** 5 points

**4.3 View Property Details - P1**
- **Why P1:** Needed to evaluate properties
- **Dependencies:** 4.2
- **Sprint:** 4
- **Effort:** 5 points

### 🟠 P2 - COULD HAVE (Phase 3)

**4.4 Bulk Import Properties from CSV - P2**
- **Why P2:** Nice to have, can add one-by-one initially
- **Dependencies:** 4.1
- **Sprint:** 7
- **Effort:** 5 points
- **Workaround:** Manual entry (20-30 properties manageable)

**4.5 Update Property Status - P2**
- **Why P2:** Can manually mark properties
- **Dependencies:** 4.3
- **Sprint:** 5
- **Effort:** 2 points
- **Note:** Simple but not MVP-critical

**Epic 4 Total:**
- **P1:** 3 stories, 15 points
- **P2:** 2 stories, 7 points

---

## EPIC 5: PROPERTY MATCHING

### 🟡 P1 - SHOULD HAVE (Phase 2)

**5.1 Auto-Generate Property Matches - P1**
- **Why P1:** Big time-saver but can match manually first
- **Dependencies:** 4.1, 2.4
- **Sprint:** 5
- **Effort:** 8 points
- **Note:** Complex algorithm, key feature

**5.2 View Property Matches for Family - P1**
- **Why P1:** Needed to review matches
- **Dependencies:** 5.1
- **Sprint:** 5
- **Effort:** 5 points

**5.3 Send Matches to Family - P1**
- **Why P1:** Can email manually initially
- **Dependencies:** 5.2
- **Sprint:** 5
- **Effort:** 3 points
- **Workaround:** Copy property links, email manually

### 🟠 P2 - COULD HAVE (Phase 3)

**5.4 Manual Property Match Override - P2**
- **Why P2:** Nice refinement to algorithm
- **Dependencies:** 5.1
- **Sprint:** 6
- **Effort:** 3 points

**5.5 Update Match Scores When Preferences Change - P2**
- **Why P2:** Helpful but can regenerate manually
- **Dependencies:** 5.1
- **Sprint:** 6
- **Effort:** 5 points

**Epic 5 Total:**
- **P1:** 3 stories, 16 points
- **P2:** 2 stories, 8 points

---

## EPIC 6: SHOWING MANAGEMENT

### 🟡 P1 - SHOULD HAVE (Phase 2)

**6.1 Schedule Showing - P1**
- **Why P1:** Important workflow, but Calendly works initially
- **Dependencies:** 4.1, 2.1
- **Sprint:** 6
- **Effort:** 5 points
- **Workaround:** Use Calendly + manual entry

**6.2 View Showing Calendar - P1**
- **Why P1:** Nice to have centralized view
- **Dependencies:** 6.1
- **Sprint:** 6
- **Effort:** 5 points
- **Workaround:** Google Calendar

**6.3 Log Showing Feedback - P1**
- **Why P1:** Important for tracking but can use notes
- **Dependencies:** 6.1
- **Sprint:** 6
- **Effort:** 3 points

### 🟠 P2 - COULD HAVE (Phase 3)

**6.4 Cancel or Reschedule Showing - P2**
- **Why P2:** Helpful but manual communication works
- **Dependencies:** 6.1
- **Sprint:** 7
- **Effort:** 3 points

**6.5 Track Broker Attendance - P2**
- **Why P2:** Nice to have, not critical
- **Dependencies:** 6.1
- **Sprint:** 7
- **Effort:** 2 points

**Epic 6 Total:**
- **P1:** 3 stories, 13 points
- **P2:** 2 stories, 5 points

---

## EPIC 7: DASHBOARD & REPORTING

### ✅ P0 - MUST HAVE (MVP)

**7.1 Main Dashboard - P0**
- **Why P0:** Essential for daily work visibility
- **Dependencies:** 3.1
- **Sprint:** 3
- **Effort:** 8 points
- **Note:** Shows pipeline status, key metrics

### 🟡 P1 - SHOULD HAVE (Phase 2)

**7.2 Pipeline Report - P1**
- **Why P1:** Useful for board meetings but can export data
- **Dependencies:** 7.1
- **Sprint:** 6
- **Effort:** 5 points

**7.3 Commission Report - P1**
- **Why P1:** Important financially but can use Excel
- **Dependencies:** 3.4
- **Sprint:** 6
- **Effort:** 3 points

### 🟠 P2 - COULD HAVE (Phase 3)

**7.4 Monthly Activity Report - P2**
- **Why P2:** Nice for board but manual reports work
- **Dependencies:** 7.1
- **Sprint:** 8
- **Effort:** 5 points

**7.5 Custom Reports - P2**
- **Why P2:** Power user feature, not essential
- **Dependencies:** 7.1
- **Sprint:** 9
- **Effort:** 8 points
- **Note:** Complex feature, low ROI initially

**Epic 7 Total:**
- **P0:** 1 story, 8 points
- **P1:** 2 stories, 8 points
- **P2:** 2 stories, 13 points

---

## EPIC 8: NOTIFICATIONS & COMMUNICATION

### ✅ P0 - MUST HAVE (MVP)

**8.2 Automated Notifications - P0**
- **Why P0:** Key for family communication
- **Dependencies:** 1.1, 1.5
- **Sprint:** 3
- **Effort:** 5 points
- **Note:** Application received, approval emails

### 🟡 P1 - SHOULD HAVE (Phase 2)

**8.1 Email Templates - P1**
- **Why P1:** Important for consistency
- **Dependencies:** 8.2
- **Sprint:** 3
- **Effort:** 5 points
- **Note:** Can hardcode templates initially, make editable later

**8.3 In-App Notifications - P1**
- **Why P1:** Nice UX but email works
- **Dependencies:** 8.2
- **Sprint:** 7
- **Effort:** 5 points

### 🔵 P3 - WON'T HAVE (Now)

**8.4 SMS Notifications - P3**
- **Why P3:** Costs money, email sufficient
- **Dependencies:** 8.2
- **Sprint:** N/A
- **Effort:** 5 points
- **Note:** Consider in 6-12 months

**Epic 8 Total:**
- **P0:** 1 story, 5 points
- **P1:** 2 stories, 10 points
- **P3:** 1 story, 5 points

---

## EPIC 9: SYSTEM ADMINISTRATION

### ✅ P0 - MUST HAVE (MVP)

**9.1 User Management - P0**
- **Why P0:** Team needs to log in
- **Dependencies:** None
- **Sprint:** 1
- **Effort:** 5 points
- **Note:** Use AWS Cognito

### 🟡 P1 - SHOULD HAVE (Phase 2)

**9.2 Audit Logging - P1**
- **Why P1:** Important for accountability but not blocking
- **Dependencies:** All entities
- **Sprint:** 4
- **Effort:** 5 points
- **Note:** Add middleware, log to CloudWatch

### 🟠 P2 - COULD HAVE (Phase 3)

**9.3 Data Backup & Export - P2**
- **Why P2:** Important but AWS has auto-backups
- **Dependencies:** None
- **Sprint:** 8
- **Effort:** 3 points
- **Note:** RDS automated backups sufficient initially

**9.4 System Configuration - P2**
- **Why P2:** Can hardcode settings initially
- **Dependencies:** None
- **Sprint:** 8
- **Effort:** 3 points

**9.5 System Health & Monitoring - P2**
- **Why P2:** CloudWatch provides basics
- **Dependencies:** None
- **Sprint:** 9
- **Effort:** 5 points
- **Note:** AWS health checks sufficient initially

**Epic 9 Total:**
- **P0:** 1 story, 5 points
- **P1:** 1 story, 5 points
- **P2:** 3 stories, 11 points

---

## EPIC 10: MOBILE & RESPONSIVENESS

### ✅ P0 - MUST HAVE (MVP)

**10.1 Mobile-Responsive UI - P0**
- **Why P0:** Team uses phones, families submit on mobile
- **Dependencies:** All UI
- **Sprint:** Continuous (built-in from start)
- **Effort:** 0 points (use responsive framework)
- **Note:** Bootstrap/Tailwind handles this

### 🟠 P2 - COULD HAVE (Phase 3)

**10.2 Offline Support (PWA) - P2**
- **Why P2:** Nice to have but online sufficient
- **Dependencies:** 10.1
- **Sprint:** 10
- **Effort:** 8 points
- **Note:** Consider in 6 months

**Epic 10 Total:**
- **P0:** 1 story, 0 points (continuous)
- **P2:** 1 story, 8 points

---

## 📊 SUMMARY BY PRIORITY

### ✅ P0 - MUST HAVE (MVP) - 15 Stories

**Total Effort:** 79 story points (~6-8 weeks)

| Epic | Stories | Points |
|------|---------|--------|
| Epic 1: Application Management | 5 | 21 |
| Epic 2: Contact Management | 3 | 11 |
| Epic 3: Deal Pipeline | 3 | 18 |
| Epic 7: Dashboard | 1 | 8 |
| Epic 8: Notifications | 1 | 5 |
| Epic 9: User Management | 1 | 5 |
| Epic 10: Mobile Responsive | 1 | 0* |
| **TOTAL P0** | **15** | **79** |

*Mobile responsive is continuous, not separate work

---

### 🟡 P1 - SHOULD HAVE (Phase 2) - 21 Stories

**Total Effort:** 103 story points (~8-10 weeks)

| Epic | Stories | Points |
|------|---------|--------|
| Epic 2: Contact Management | 2 | 8 |
| Epic 3: Deal Pipeline | 2 | 8 |
| Epic 4: Property Listings | 3 | 15 |
| Epic 5: Property Matching | 3 | 16 |
| Epic 6: Showing Management | 3 | 13 |
| Epic 7: Dashboard & Reporting | 2 | 8 |
| Epic 8: Notifications | 2 | 10 |
| Epic 9: Audit Logging | 1 | 5 |
| **TOTAL P1** | **18** | **103** |

---

### 🟠 P2 - COULD HAVE (Phase 3) - 14 Stories

**Total Effort:** 67 story points (~5-6 weeks)

| Epic | Stories | Points |
|------|---------|--------|
| Epic 4: Property Listings | 2 | 7 |
| Epic 5: Property Matching | 2 | 8 |
| Epic 6: Showing Management | 2 | 5 |
| Epic 7: Dashboard & Reporting | 2 | 13 |
| Epic 8: Notifications | 1 | 5 |
| Epic 9: Admin Features | 3 | 11 |
| Epic 10: PWA | 1 | 8 |
| **TOTAL P2** | **13** | **67** |

---

### 🔵 P3 - WON'T HAVE (Now) - 1 Story

**Total Effort:** 5 story points

| Epic | Stories | Points |
|------|---------|--------|
| Epic 8: SMS Notifications | 1 | 5 |
| **TOTAL P3** | **1** | **5** |

---

## 🎯 RECOMMENDED IMPLEMENTATION PLAN

### **Phase 1: MVP (6-8 weeks)**
**Goal:** Replace Google Form, enable basic workflow

**Sprint 1 (2 weeks) - 21 points:**
- Epic 1: Stories 1.1, 1.2 ✅
- Epic 2: Stories 2.1, 2.2 ✅
- Epic 9: Story 9.1 ✅
- **Deliverable:** Application form works, families can apply, staff can view

**Sprint 2 (2 weeks) - 26 points:**
- Epic 1: Stories 1.3, 1.4, 1.5 ✅
- Epic 2: Story 2.3 ✅
- Epic 3: Stories 3.1, 3.2 ✅
- **Deliverable:** Board can approve, deals move through pipeline

**Sprint 3 (2 weeks) - 18 points:**
- Epic 3: Story 3.3 ✅
- Epic 7: Story 7.1 ✅
- Epic 8: Stories 8.1, 8.2 ✅
- **Deliverable:** Dashboard shows status, emails auto-send

**Phase 1 Complete:**
- ✅ Families can apply online
- ✅ Board can review and approve
- ✅ Staff can manage deals
- ✅ Basic dashboard
- ✅ Email notifications
- **Result: CAN GO LIVE!** 🎉

---

### **Phase 2: Property Matching (6-8 weeks)**
**Goal:** Add property management and automated matching

**Sprint 4 (2 weeks) - 30 points:**
- Epic 2: Story 2.4 🟡
- Epic 4: Stories 4.1, 4.2, 4.3 🟡
- Epic 9: Story 9.2 🟡
- **Deliverable:** Property database operational

**Sprint 5 (2 weeks) - 19 points:**
- Epic 2: Story 2.5 🟡
- Epic 4: Story 4.5 🟡
- Epic 5: Stories 5.1, 5.2, 5.3 🟡
- **Deliverable:** Automated property matching works

**Sprint 6 (2 weeks) - 29 points:**
- Epic 3: Stories 3.4, 3.5 🟡
- Epic 5: Stories 5.4, 5.5 🟡
- Epic 6: Stories 6.1, 6.2, 6.3 🟡
- Epic 7: Stories 7.2, 7.3 🟡
- **Deliverable:** Complete workflow including showings and commission

**Phase 2 Complete:**
- ✅ Property database with photos
- ✅ Automated matching algorithm
- ✅ Showing scheduler
- ✅ Commission tracking
- **Result: FULL FEATURES!** 🚀

---

### **Phase 3: Polish & Enhancements (4-5 weeks)**
**Goal:** Nice-to-have features and optimizations

**Sprint 7-9 (6 weeks) - 67 points:**
- Bulk import 🟠
- Advanced reports 🟠
- In-app notifications 🟠
- PWA offline support 🟠
- Admin features 🟠
- Performance optimization
- User training

**Phase 3 Complete:**
- ✅ All features implemented
- ✅ System optimized
- ✅ Team fully trained
- **Result: WORLD-CLASS SYSTEM!** 🏆

---

## 🎯 CRITICAL PATH DEPENDENCIES

**Must do in order:**

```
Sprint 1:
- User Management (9.1) → FIRST
- Application Form (1.1) → Needs public access
- View Applications (1.2) → Needs 1.1
- Contact CRUD (2.1, 2.2) → Foundation

Sprint 2:
- Board Review (1.3) → Needs 1.2
- Deal Pipeline (3.1, 3.2) → Needs 1.3
- Edit Contacts (2.3) → Enhances 2.2

Sprint 3:
- Deal Details (3.3) → Needs 3.2
- Dashboard (7.1) → Needs 3.2
- Notifications (8.2) → Needs 1.3, 1.5

Sprint 4+:
- Properties (4.x) → Independent, can start anytime
- Matching (5.x) → Needs 4.1 + 2.4
- Showings (6.x) → Needs 4.1
```

---

## 💰 VALUE VS EFFORT ANALYSIS

### **Highest Value per Effort (Do First):**

1. **Application Form (1.1)** - 5 points, huge value (replaces Google Form)
2. **View Contacts/Applications (1.2, 2.1)** - 3 points each, essential
3. **Board Approval (1.3)** - 5 points, core workflow
4. **Dashboard (7.1)** - 8 points, high visibility
5. **Auto-reject (1.4)** - 3 points, saves time

### **High Value, High Effort (Do in Phase 2):**

1. **Deal Pipeline Kanban (3.1)** - 8 points, complex but essential UX
2. **Property Matching Algorithm (5.1)** - 8 points, key differentiator
3. **Custom Reports (7.5)** - 8 points, powerful but optional

### **Low Priority (Phase 3 or Later):**

1. **Bulk Import (4.4)** - 5 points, manual entry works for 30 properties
2. **PWA Offline (10.2)** - 8 points, nice but online sufficient
3. **SMS (8.4)** - 5 points, costs money, email sufficient

---

## ✅ MVP SCOPE (Can Launch After Sprint 3)

**Must have these 15 stories:**
- ✅ Public application form
- ✅ Contact management
- ✅ Deal pipeline (5 stages)
- ✅ Board review and approval
- ✅ Basic dashboard
- ✅ Email notifications
- ✅ User authentication

**Can launch without:**
- ⏸️ Property management (can match manually with Zillow links)
- ⏸️ Automated matching (coordinator can match manually)
- ⏸️ Showing scheduler (use Calendly for now)
- ⏸️ Commission tracking (use Excel for now)
- ⏸️ Reports (export to Excel for now)

**Minimum Viable Team:** 1 developer (you!) + 1 tester (staff member)

**Minimum Viable Launch:** After Sprint 3 (6 weeks of development)

---

## 🚀 QUICK START RECOMMENDATION

**Week 1-2 (Sprint 1):**
Focus on these 6 stories only:
1. User Management (9.1)
2. Submit Application (1.1)
3. View Applications (1.2)
4. View Contacts (2.1)
5. View Contact Details (2.2)
6. Mobile Responsive (10.1 - built-in)

**If these work, you're 30% done with MVP!**

---

## 📋 NEXT STEPS

1. **Review priorities** - Do you agree with P0/P1/P2 breakdown?
2. **Adjust if needed** - Want to move stories between priorities?
3. **Confirm MVP scope** - 15 P0 stories sufficient for launch?
4. **Start Sprint 1** - Ready to build?

**Once you approve priorities, I'll create:**
- ✅ CSV file for Jira import
- ✅ Technical specs for Phase 1
- ✅ Sample code for key features

---

**What would you like to adjust in the prioritization?**
