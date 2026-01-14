# P0 STORIES SUMMARY
## MVP User Stories (High-Level Overview)

**Total P0 Stories:** ~35 stories  
**Total Points:** ~140 points  
**Estimated Duration:** 12-16 weeks (3-4 sprints)  

---

## 📋 STORY BREAKDOWN BY EPIC

### EPIC 1: Foundation & Setup (13 points) - SPRINT 1
✅ **Detailed in SPRINT_1_DETAILED_STORIES.md**

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-001 | Set up Visual Studio solution structure | 5 | Sprint 1 |
| US-002 | Configure AWS Cognito authentication | 5 | Sprint 1 |
| US-003 | Set up PostgreSQL + EF Core | 3 | Sprint 1 |

---

### EPIC 2: Domain Model (13 points) - SPRINT 1
✅ **Detailed in SPRINT_1_DETAILED_STORIES.md**

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-004 | Implement core domain entities | 8 | Sprint 1 |
| US-005 | Implement value objects | 5 | Sprint 1 |

---

### EPIC 3: Applicant CRUD (16 points) - SPRINT 1
✅ **Detailed in SPRINT_1_DETAILED_STORIES.md**

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-006 | Create applicant (coordinator) | 5 | Sprint 1 |
| US-007 | View applicant details | 3 | Sprint 1 |
| US-008 | Update applicant basic info | 3 | Sprint 1 |
| US-009 | List applicants with search/filter | 5 | Sprint 1 |

**Sprint 1 Total: 42 points**

---

### EPIC 4: Public Application Form (12 points) - SPRINT 2

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-010 | Create public application form (no auth) | 8 | Sprint 2 |
| US-011 | Submit application creates applicant | 5 | Sprint 2 |

**Description:** Public-facing form for families to apply. No authentication required. Creates applicant + application on submit. Sends confirmation email.

---

### EPIC 5: Application Management (18 points) - SPRINT 2

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-012 | View application details | 3 | Sprint 2 |
| US-013 | View application pipeline (Kanban) | 8 | Sprint 2 |
| US-014 | Change application stage | 5 | Sprint 2 |
| US-015 | Put application on hold | 2 | Sprint 2 |

**Description:** Coordinators can view all applications in a Kanban board (Submitted → Approved → House Hunting → Under Contract → Closing → Moved In). Can drag/drop to change stages.

---

### EPIC 6: Housing Preferences (8 points) - SPRINT 2

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-016 | Update housing preferences | 5 | Sprint 2 |
| US-017 | Calculate monthly payment estimate | 3 | Sprint 2 |

**Description:** Coordinators can set/update family's housing preferences (budget, bedrooms, bathrooms, cities, features, shul proximity, move timeline).

---

### EPIC 7: Board Review (13 points) - SPRINT 3

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-018 | Submit applicant for board review | 3 | Sprint 3 |
| US-019 | Record board decision | 5 | Sprint 3 |
| US-020 | View applicants pending board review | 3 | Sprint 3 |
| US-021 | Send board decision email | 2 | Sprint 3 |

**Description:** Coordinators submit applicants for board review. Board members review and approve/reject. Decision recorded at APPLICANT level (not application). Email sent with decision.

---

### EPIC 8: Property Management - Basic (21 points) - SPRINT 3

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-022 | Create property listing | 5 | Sprint 3 |
| US-023 | View property details | 3 | Sprint 3 |
| US-024 | Update property listing | 3 | Sprint 3 |
| US-025 | Upload property photos | 5 | Sprint 3 |
| US-026 | Update property status | 2 | Sprint 3 |
| US-027 | List properties with filters | 3 | Sprint 3 |

**Description:** Coordinators can add/edit property listings. Upload photos to S3. Track status (Active, Under Contract, Sold, Off Market). Filter by city, status, bedrooms, price range.

---

### EPIC 9: Email Notifications (8 points) - SPRINT 3

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-028 | Send application received email | 2 | Sprint 3 |
| US-029 | Send board decision email | 2 | Sprint 3 |
| US-030 | Send application status change email | 2 | Sprint 3 |
| US-031 | Configure AWS SES | 2 | Sprint 3 |

**Description:** Automated emails via AWS SES. Templates for: application received, board decision (approved/rejected), status changes (approved, under contract, closing). HTML email templates.

---

### EPIC 10: Dashboard (8 points) - SPRINT 4

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-032 | View dashboard summary | 5 | Sprint 4 |
| US-033 | View recent activity timeline | 3 | Sprint 4 |

**Description:** Dashboard shows key metrics: applications pending review, active applications by stage, properties available, recent activity feed.

---

### EPIC 11: Activity Tracking (8 points) - SPRINT 3

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-034 | Log activity/interaction | 3 | Sprint 3 |
| US-035 | View activity timeline | 3 | Sprint 3 |
| US-036 | Filter activity by type | 2 | Sprint 3 |

**Description:** Track all interactions (phone calls, emails, meetings, status changes, documents). View full timeline on applicant/application pages. Filter by type.

---

### EPIC 12: Follow-Up Reminders (10 points) - SPRINT 3

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-037 | Create follow-up reminder | 3 | Sprint 3 |
| US-038 | View reminders dashboard | 5 | Sprint 3 |
| US-039 | Print reminders list | 2 | Sprint 3 |

**Description:** Coordinators set reminders for follow-ups. Dashboard shows all due reminders (overdue, today, this week). Can filter by assigned coordinator. Print view for paper workflow.

---

## 📊 MVP SCOPE SUMMARY

### Phase 1 (MVP) - Sprints 1-4 (12-16 weeks)

**What's Included:**
- ✅ Complete applicant management
- ✅ Public application form
- ✅ Application workflow (all stages)
- ✅ Board review process
- ✅ Basic property tracking
- ✅ Email notifications
- ✅ Activity/interaction tracking
- ✅ Follow-up reminders
- ✅ Dashboard with metrics

**What's NOT Included (Phase 2):**
- ❌ Property matching algorithm
- ❌ Applicant portal (Google OAuth)
- ❌ Showing scheduler
- ❌ Broker management
- ❌ Commission tracking
- ❌ Prospect management
- ❌ Advanced reports
- ❌ Bulk import

**Deliverable:** Working CRM for core workflow (application submission → board review → property assignment → closing)

---

## 🎯 SPRINT ALLOCATION

### Sprint 1 (2 weeks) - 42 points ✅ PLANNED
**Focus:** Foundation + Domain + Basic CRUD
- Visual Studio solution
- Authentication (Cognito)
- Database (PostgreSQL + EF Core)
- Domain entities and value objects
- Applicant CRUD API

**Deliverable:** Can create/view/update applicants via API

---

### Sprint 2 (2 weeks) - ~40 points ⏳ NOT PLANNED YET
**Focus:** Application Workflow + Public Form
- Public application form
- Application CRUD
- Application pipeline (Kanban)
- Stage changes (workflow)
- Housing preferences
- On Hold workflow

**Deliverable:** Families can apply, coordinators can track applications

---

### Sprint 3 (2 weeks) - ~45 points ⏳ NOT PLANNED YET
**Focus:** Board Review + Properties + Email
- Board review workflow
- Property management (CRUD)
- Property photos (S3 upload)
- Email notifications (AWS SES)
- Activity tracking
- Follow-up reminders

**Deliverable:** Complete workflow from application → board → properties

---

### Sprint 4 (2 weeks) - ~35 points ⏳ NOT PLANNED YET
**Focus:** Dashboard + Polish
- Dashboard with metrics
- Recent activity feed
- Reminders dashboard with print
- Bug fixes
- Performance optimization
- Documentation

**Deliverable:** MVP complete, production-ready

**Total MVP: ~162 points over 12-16 weeks**

---

## 🔄 STORY PRIORITIZATION

### Must Have (P0) - 35 stories
All stories listed above. These are REQUIRED for MVP.

### Should Have (P1) - Phase 2
- Property matching algorithm
- Applicant portal
- Showing scheduler
- Walking distance calculation
- Open house tracking
- Failed contracts tracking
- Prospect pipeline

### Could Have (P2) - Phase 3
- SMS notifications
- Email marketing blasts
- Advanced reports
- Bulk property import
- Enhanced feature matching
- Commission tracking

### Won't Have (This Release)
- Mobile app
- Multi-language support
- Integration with external CRMs

---

## 📝 STORY TEMPLATE

For reference when creating new stories:

```markdown
### US-XXX: [Story Title]

**As a** [user role]
**I want to** [action]
**So that** [benefit]

**Priority:** P0/P1/P2
**Effort:** X points
**Sprint:** Sprint X

#### Acceptance Criteria

1. [Criterion 1]
2. [Criterion 2]
...

#### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| [Scenario] | [Given] | [When] | [Then] |

#### Technical Implementation

[Code samples, API endpoints, etc.]

#### Definition of Done

- [ ] Code written
- [ ] Tests passing
- [ ] API tested
- [ ] Committed to Git
```

---

## 🎯 NEXT STEPS

1. **Complete Sprint 1** (current)
   - Follow SPRINT_1_DETAILED_STORIES.md
   - Use SPRINT_1_PLANNING.md for day-by-day guidance

2. **Plan Sprint 2** (after Sprint 1 retrospective)
   - Detail stories US-010 through US-017
   - Create SPRINT_2_DETAILED_STORIES.md
   - Adjust based on Sprint 1 learnings

3. **Plan Sprint 3** (after Sprint 2)
   - Detail stories US-018 through US-037
   - Focus on board review + properties

4. **Plan Sprint 4** (after Sprint 3)
   - Detail stories US-038 and US-039 (if not in Sprint 3)
   - Polish and bug fixes
   - Prepare for MVP launch

---

## 📚 RELATED DOCUMENTS

- **SPRINT_1_DETAILED_STORIES.md** - Full detail for Sprint 1 (9 stories)
- **SPRINT_1_PLANNING.md** - Sprint 1 planning and timeline
- **SPRINT_1_JIRA_IMPORT.csv** - Import Sprint 1 to Jira
- **MASTER_REQUIREMENTS_v3.md** - Complete requirements
- **CONVERSATION_MEMORY_LOG.md** - Full context and decisions

---

**P0 Stories Summary Complete - MVP Scope Defined! 🎯**
