# Product Roadmap

> **Last Updated:** January 27, 2026
> **Current Version:** 1.0.0-beta (Development)

---

## Overview

The Family Relocation CRM is approximately **85% complete** for MVP. The core workflow (applicant → approval → searching → contract → closing → moved-in) is fully functional. This roadmap prioritizes remaining work to reach production-ready status.

---

## Priority Framework

| Priority | Definition | Timeline |
|----------|------------|----------|
| **P0 - Critical** | Blocks production launch | This week |
| **P1 - High** | Required for MVP | Next 2 weeks |
| **P2 - Medium** | Important but can wait | Next month |
| **P3 - Low** | Nice to have | Future |

---

## Current Sprint: Production Readiness

### P0 - Critical (This Week)

These items must be completed before production launch.

| # | Task | Description | Effort |
|---|------|-------------|--------|
| 1 | **Production Deployment** | Set up production environment on AWS | 1 day |
| 2 | **Data Migration Plan** | Plan for migrating any existing data | 0.5 day |
| 3 | **SSL/Domain Setup** | Configure production domain and certificates | 0.5 day |
| 4 | **Smoke Testing** | Run through COMPREHENSIVE_TEST_PLAN.md on dev | 1 day |
| 5 | **Bug Fixes from Testing** | Fix any critical bugs found | 1-2 days |

**Total P0 Effort:** ~4-5 days

---

### P1 - High Priority (Next 2 Weeks)

Required for comfortable MVP launch.

| # | Task | Description | Effort | Status |
|---|------|-------------|--------|--------|
| 1 | **Audit Log UI** | Add audit log viewer to admin section | 1 day | Not Started |
| 2 | **Property-Shul Distance Display** | Show walking distances on property detail page | 0.5 day | Not Started |
| 3 | **Error Handling Polish** | Improve error messages and edge case handling | 1 day | Not Started |
| 4 | **Loading States** | Add loading indicators where missing | 0.5 day | Not Started |
| 5 | **Mobile Responsiveness** | Basic mobile support (not full mobile app) | 1 day | Not Started |
| 6 | **Email Templates** | Create professional email templates for notifications | 1 day | Partial |
| 7 | **User Documentation** | Basic user guide for staff | 1 day | Not Started |

**Total P1 Effort:** ~6-7 days

---

### P2 - Medium Priority (Next Month)

Important features that enhance the system.

| # | Task | Description | Effort | Status |
|---|------|-------------|--------|--------|
| 1 | **Open House Management** | Create OpenHouse entity, API, and UI | 3 days | Not Started |
| 2 | **Bulk Property Import** | Import properties from CSV/MLS feed | 2 days | Not Started |
| 3 | **Advanced Reporting** | Export reports, analytics dashboard | 2 days | Not Started |
| 4 | **Email Notification Preferences** | Let users configure which emails they receive | 1 day | Not Started |
| 5 | **Search Improvements** | Full-text search across all entities | 1 day | Not Started |
| 6 | **Keyboard Shortcuts** | Power user keyboard navigation | 1 day | Not Started |
| 7 | **Batch Operations** | Bulk status updates, bulk assignments | 1 day | Not Started |

**Total P2 Effort:** ~11 days

---

### P3 - Low Priority (Future)

Nice-to-have features for future development.

| # | Task | Description | Effort | Status |
|---|------|-------------|--------|--------|
| 1 | **Email Blast System** | Bulk email campaigns to applicants | 5 days | Not Started |
| 2 | **SMS Notifications** | Twilio integration for SMS | 2 days | Not Started |
| 3 | **Mobile App** | React Native mobile application | 20+ days | Not Started |
| 4 | **MLS Integration** | Direct feed from GSMLS | 5 days | Not Started |
| 5 | **Document E-Signatures** | DocuSign/HelloSign integration | 3 days | Not Started |
| 6 | **Calendar Sync** | Google/Outlook calendar integration | 2 days | Not Started |
| 7 | **Applicant Portal** | Self-service portal for applicants | 10 days | Not Started |
| 8 | **Advanced Analytics** | Conversion funnels, time-to-close metrics | 3 days | Not Started |

---

## Technical Debt

Items from [TECHNICAL_DEBT.md](TECHNICAL_DEBT.md) that should be addressed:

| # | Issue | Impact | Effort | Priority |
|---|-------|--------|--------|----------|
| 1 | EF Core refs in Application layer | Architecture violation | 2 days | P2 |
| 2 | Missing unit tests | Lower confidence | 5 days | P2 |
| 3 | Hardcoded strings | Maintainability | 1 day | P3 |

---

## Release Plan

### v1.0.0 - Production Launch
**Target:** February 2026

Includes:
- All P0 items complete
- Critical P1 items complete
- Production environment live
- Staff trained on system

### v1.1.0 - Polish Release
**Target:** March 2026

Includes:
- Remaining P1 items
- Key P2 items (Open House, Bulk Import)
- Performance optimizations

### v1.2.0 - Feature Release
**Target:** Q2 2026

Includes:
- Advanced reporting
- Email campaigns (P3)
- Additional integrations

---

## Feature Requests Backlog

Items mentioned in requirements but not yet prioritized:

| Feature | Source | Notes |
|---------|--------|-------|
| Default broker assignment | FINAL_CORRECTIONS | System config for default broker |
| Walking distance map visualization | Requirements | Show map with shul walking routes |
| Property comparison view | User request | Side-by-side property comparison |
| Saved searches | User request | Save and reuse search filters |
| Activity timeline export | User request | Export activity history to PDF |

---

## Completed Milestones

| Milestone | Date | Notes |
|-----------|------|-------|
| Sprint 1 Complete | Jan 2026 | Core entities, auth, basic CRUD |
| Sprint 2 Complete | Jan 2026 | Properties, matching, showings |
| Sprint 3 Complete | Jan 2026 | Reminders, activities, documents |
| Sprint 4 Complete | Jan 2026 | Pipeline, scheduler, polish |
| Beta Deployment | Jan 27, 2026 | dev.unionvaad.com live |

---

## Decision Log

Key decisions affecting the roadmap:

| Decision | Date | Rationale |
|----------|------|-----------|
| Desktop-first, no mobile optimization | Jan 2026 | Staff use desktop; saves 20+ days |
| AWS over Azure | Jan 2026 | Team expertise, cost |
| PostgreSQL over SQL Server | Jan 2026 | Cost, JSON support |
| Defer email campaigns | Jan 2026 | Not needed for MVP |
| Defer SMS | Jan 2026 | Email sufficient for MVP |

---

## How to Update This Roadmap

1. When completing a task, move it to "Completed" section with date
2. When adding new requests, add to "Feature Requests Backlog"
3. During sprint planning, promote backlog items to priority tiers
4. Update "Last Updated" date at top of document

---

## Questions for Stakeholder Review

Before finalizing the roadmap, confirm:

1. Is the P0 list correct for production launch?
2. Are there any P3 items that should be P1?
3. What is the target production launch date?
4. Who needs to be trained on the system?
5. Any compliance/security requirements not yet addressed?
