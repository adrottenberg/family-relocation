# USER STORIES - COMPLETE LIST WITH SPRINT ASSIGNMENTS
## Family Relocation CRM System

**Total: 68 User Stories across 11 Epics**
**Total Effort: ~280 Story Points**
**Timeline: 19-24 weeks**

---

## SPRINT OVERVIEW

| Sprint | Duration | Focus | Stories | Points |
|--------|----------|-------|---------|--------|
| Sprint 1 | 2 weeks | Foundation + Auth + Basic CRUD | 9 | ~35 |
| Sprint 2 | 2 weeks | Public Application + Pipeline + Frontend Foundation | 11 | 34 |
| Sprint 3 | 2 weeks | Property Management + Matching | TBD | TBD |
| Sprint 4 | 2 weeks | Showings + Calendar | TBD | TBD |
| Sprint 5 | 2 weeks | Dashboard + Reports | TBD | TBD |
| Sprint 6+ | Ongoing | Notifications, Admin, Polish | TBD | TBD |

---

## ✅ SPRINT 1 - COMPLETE (9 Stories, ~35 points)

| ID | Story Title | Points | Status |
|----|-------------|--------|--------|
| US-001 | Set up VS solution structure (Clean Architecture) | 3 | ✅ Done |
| US-002 | Configure AWS Cognito authentication | 5 | ✅ Done |
| US-003 | Set up PostgreSQL + EF Core with migrations | 5 | ✅ Done |
| US-004 | Implement core domain entities (Applicant, HousingSearch) | 8 | ✅ Done |
| US-005 | Implement value objects (Address, PersonInfo, etc.) | 5 | ✅ Done |
| US-006 | Create applicant endpoint (POST /api/applicants) | 3 | ✅ Done |
| US-007 | View applicant details (GET /api/applicants/{id}) | 3 | ✅ Done |
| US-008 | Update applicant basic info (PUT /api/applicants/{id}) | 3 | ✅ Done |
| US-009 | List applicants with search/filter (GET /api/applicants) | 5 | ✅ Done |

**Sprint 1 Result:** 291 tests passing (196 Domain + 83 API + 12 Integration)

---

## 🔄 SPRINT 2 - PLANNED (11 Stories, 34 points)

### Backend (17 points)

| ID | Story Title | Points | Status |
|----|-------------|--------|--------|
| US-010 | Modify applicant creation to also create HousingSearch | 3 | Planned |
| US-014 | View applicant pipeline (GET /api/applicants/pipeline) | 5 | Planned |
| US-015 | Change HousingSearch stage (PUT /api/applicants/{id}/housing-search/stage) | 2 | Planned |
| US-016 | Update housing preferences | 2 | Planned |
| US-018 | Implement audit log feature (EF Core interceptor + API) | 5 | Planned |

### Frontend (17 points)

| ID | Story Title | Points | Status |
|----|-------------|--------|--------|
| US-F01 | React project setup with design system | 3 | Planned |
| US-F02 | Authentication flow (login page) | 5 | Planned |
| US-F03 | App shell & navigation | 3 | Planned |
| US-F04 | Applicant list page | 3 | Planned |
| US-F05 | Applicant detail page | 3 | Planned |
| US-F06 | Pipeline Kanban board | 5 | Planned |

### Deferred from Sprint 2

| ID | Story Title | Reason | Deferred To |
|----|-------------|--------|-------------|
| US-011 | Application confirmation email | Need proper editable templates | Sprint 4+ |
| US-012 | Create HousingSearch for applicant | Now handled by US-010 | Removed |
| US-013 | View HousingSearch details | Already in Applicant response | Removed |
| US-017 | Calculate monthly payment estimate | Nice-to-have | P3 |

---

## 📋 BACKLOG - FUTURE SPRINTS

### EPIC: Board Review & Approval (P0)

| ID | Story Title | Points | Priority | Target |
|----|-------------|--------|----------|--------|
| US-019 | Board reviews application (set decision) | 5 | P0 | Sprint 3 |
| US-020 | Auto-move to Rejected when board disapproves | 3 | P0 | Sprint 3 |
| US-021 | Approve application and trigger next steps | 5 | P0 | Sprint 3 |

### EPIC: Property Management (P1)

| ID | Story Title | Points | Priority | Target |
|----|-------------|--------|----------|--------|
| US-022 | Add property listing | 5 | P1 | Sprint 3 |
| US-023 | View property list with filters | 5 | P1 | Sprint 3 |
| US-024 | View property details | 5 | P1 | Sprint 3 |
| US-025 | Update property status | 2 | P1 | Sprint 3 |
| US-026 | Bulk import properties from CSV | 5 | P2 | Sprint 5+ |
| US-027 | Upload property photos | 3 | P1 | Sprint 3 |

### EPIC: Property Matching (P1)

| ID | Story Title | Points | Priority | Target |
|----|-------------|--------|----------|--------|
| US-028 | Auto-generate property matches (algorithm) | 8 | P1 | Sprint 4 |
| US-029 | View property matches for family | 5 | P1 | Sprint 4 |
| US-030 | Send matches to family (email) | 3 | P1 | Sprint 4 |
| US-031 | Manual property match override | 3 | P2 | Sprint 5 |
| US-032 | Update match scores when preferences change | 5 | P2 | Sprint 5 |

### EPIC: Showing Management (P1)

| ID | Story Title | Points | Priority | Target |
|----|-------------|--------|----------|--------|
| US-033 | Schedule showing | 5 | P1 | Sprint 4 |
| US-034 | View showing calendar | 5 | P1 | Sprint 4 |
| US-035 | Log showing feedback | 3 | P1 | Sprint 4 |
| US-036 | Cancel or reschedule showing | 3 | P2 | Sprint 5 |
| US-037 | Track broker attendance | 2 | P2 | Sprint 5 |

### EPIC: Deal Workflow (P1)

| ID | Story Title | Points | Priority | Target |
|----|-------------|--------|----------|--------|
| US-038 | Put search on hold | 5 | P1 | Sprint 4 |
| US-039 | Handle failed contract | 8 | P1 | Sprint 4 |
| US-040 | Close deal and track move-in | 3 | P1 | Sprint 4 |
| US-041 | Track commission | 5 | P1 | Sprint 5 |

### EPIC: Dashboard & Reporting (P1)

| ID | Story Title | Points | Priority | Target |
|----|-------------|--------|----------|--------|
| US-042 | Main dashboard with key metrics | 8 | P0 | Sprint 5 |
| US-043 | Pipeline report (chart) | 5 | P1 | Sprint 5 |
| US-044 | Commission report | 3 | P1 | Sprint 5 |
| US-045 | Monthly activity report | 5 | P2 | Sprint 6 |
| US-046 | Failed contract report | 5 | P2 | Sprint 6 |
| US-047 | Custom reports builder | 8 | P3 | Future |

### EPIC: Notifications & Communication (P1)

| ID | Story Title | Points | Priority | Target |
|----|-------------|--------|----------|--------|
| US-048 | Email templates (CRUD) | 5 | P1 | Sprint 4 |
| US-049 | Automated notifications (events) | 5 | P1 | Sprint 4 |
| US-050 | In-app notifications | 5 | P1 | Sprint 5 |
| US-051 | SMS notifications | 5 | P3 | Future |

### EPIC: Follow-up & Tasks (P1)

| ID | Story Title | Points | Priority | Target |
|----|-------------|--------|----------|--------|
| US-052 | Create follow-up reminder | 5 | P1 | Sprint 5 |
| US-053 | View my reminders/tasks | 3 | P1 | Sprint 5 |
| US-054 | Smart reminder suggestions | 8 | P2 | Sprint 6 |

### EPIC: System Administration (P1)

| ID | Story Title | Points | Priority | Target |
|----|-------------|--------|----------|--------|
| US-055 | User management (invite, roles) | 5 | P0 | Sprint 3 |
| US-056 | Data backup & export | 3 | P2 | Sprint 6 |
| US-057 | System configuration | 3 | P2 | Sprint 6 |
| US-058 | System health monitoring | 5 | P2 | Sprint 6 |

### EPIC: Applicant Portal (P2)

| ID | Story Title | Points | Priority | Target |
|----|-------------|--------|----------|--------|
| US-059 | Applicant login (Google auth) | 5 | P2 | Sprint 6 |
| US-060 | Applicant dashboard (status view) | 5 | P2 | Sprint 6 |
| US-061 | Housing preferences form (self-service) | 5 | P2 | Sprint 6 |
| US-062 | View property matches (applicant) | 3 | P2 | Sprint 6 |
| US-063 | Document upload (applicant) | 3 | P2 | Sprint 6 |

### EPIC: Additional Features (P2-P3)

| ID | Story Title | Points | Priority | Target |
|----|-------------|--------|----------|--------|
| US-064 | Notes/activity log on applicant | 3 | P1 | Sprint 4 |
| US-065 | Delete/archive applicant | 2 | P2 | Sprint 5 |
| US-066 | Duplicate applicant detection | 3 | P2 | Sprint 5 |
| US-067 | Export applicants to Excel | 3 | P2 | Sprint 6 |
| US-068 | Import applicants from CSV | 5 | P3 | Future |

---

## PRIORITY LEGEND

| Priority | Meaning | Timeline |
|----------|---------|----------|
| P0 | Must Have (MVP) | Sprints 1-3 |
| P1 | Should Have | Sprints 3-5 |
| P2 | Could Have | Sprints 5-6 |
| P3 | Won't Have Now | Future |

---

## SUMMARY BY PRIORITY

| Priority | Stories | Points |
|----------|---------|--------|
| P0 (MVP) | ~18 | ~85 |
| P1 (Phase 2) | ~30 | ~120 |
| P2 (Phase 3) | ~15 | ~55 |
| P3 (Future) | ~5 | ~20 |
| **Total** | **68** | **~280** |

---

## NOTES

1. **Sprint 1 Complete** - Foundation laid, 291 tests passing
2. **Sprint 2 In Progress** - Backend API + Frontend foundation
3. **Stories may shift** between sprints based on dependencies and discoveries
4. **Frontend stories (US-F##)** are separate from backend stories
5. **Some stories removed** during Sprint 2 planning (US-012, US-013 merged into US-010)

---

*Last Updated: January 19, 2026*
