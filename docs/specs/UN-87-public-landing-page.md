# UN-87: Public Landing Page with Open Listings

**Jira Ticket:** [UN-87](https://adrottenberg.atlassian.net/browse/UN-87)
**Status:** Draft
**Author:** Claude
**Date:** 2026-01-27

## Overview

Create a public-facing landing page that displays available property listings without requiring authentication. This allows potential applicants to browse opportunities before applying.

## Related Tickets

| Ticket | Summary | Dependency |
|--------|---------|------------|
| UN-87 | Public landing page UI | This ticket |
| UN-88 | Public listings API endpoint | Blocker for UN-87 |
| UN-89 | Role-based login redirect | Independent |

## User Stories

**Primary:**
As a potential applicant visiting the site, I want to see available property listings without logging in, so that I can browse opportunities before applying.

**Secondary:**
As an internal staff member, I want a login link on the public page, so that I can access the dashboard.

## Acceptance Criteria

- [ ] Home page (/) displays public listings instead of redirecting to dashboard
- [ ] Property cards displayed in responsive grid layout
- [ ] Each card shows: photo, address, price, bedrooms, bathrooms, city
- [ ] Prominent "Apply Now" button linking to /apply
- [ ] "Login" link for staff (top right or header)
- [ ] No authentication required
- [ ] Mobile-friendly (basic responsiveness)

## Technical Approach

### Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Public Landing Page                   │
│  ┌─────────────────────────────────────────────────────┐│
│  │  Header: Logo | [Apply Now] | [Login]               ││
│  └─────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────┐│
│  │  Hero Section (optional): Welcome message           ││
│  └─────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────┐│
│  │  Property Grid                                       ││
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐               ││
│  │  │ Card 1  │ │ Card 2  │ │ Card 3  │               ││
│  │  │ [Photo] │ │ [Photo] │ │ [Photo] │               ││
│  │  │ Address │ │ Address │ │ Address │               ││
│  │  │ $Price  │ │ $Price  │ │ $Price  │               ││
│  │  │ 3BR 2BA │ │ 4BR 2BA │ │ 3BR 1BA │               ││
│  │  └─────────┘ └─────────┘ └─────────┘               ││
│  └─────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────┐│
│  │  Footer: Contact info, links                        ││
│  └─────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────┘
```

### API Endpoint (UN-88)

```
GET /api/public/listings
```

**Response:**
```json
{
  "listings": [
    {
      "id": "guid",
      "address": {
        "street": "123 Main St",
        "city": "Union",
        "state": "NJ",
        "zipCode": "07083"
      },
      "price": 450000,
      "bedrooms": 3,
      "bathrooms": 2,
      "primaryPhotoUrl": "https://s3.../photo.jpg",
      "status": "Available"
    }
  ],
  "total": 10
}
```

### Frontend Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `PublicLandingPage` | `src/pages/PublicLandingPage.tsx` | Main page component |
| `PublicHeader` | `src/components/public/PublicHeader.tsx` | Header with logo, Apply, Login |
| `PropertyCard` | `src/components/public/PropertyCard.tsx` | Individual listing card |
| `PropertyGrid` | `src/components/public/PropertyGrid.tsx` | Grid container |

### Routing Changes

**Current (`src/App.tsx` or router config):**
```tsx
// Current: root redirects to dashboard
<Route path="/" element={<Navigate to="/dashboard" />} />
```

**New:**
```tsx
// New: root shows public landing page
<Route path="/" element={<PublicLandingPage />} />
```

### Public vs Authenticated Routes

```
Public (no auth required):
  /                    → PublicLandingPage
  /apply               → PublicApplicationForm (existing)
  /login               → LoginPage

Authenticated (auth required):
  /dashboard           → Dashboard
  /applicants          → ApplicantsList
  /properties          → PropertiesList
  ... (all existing routes)
```

## UI/UX Details

### Property Card Design

```
┌────────────────────────────┐
│ ┌────────────────────────┐ │
│ │                        │ │
│ │       [Photo]          │ │
│ │                        │ │
│ └────────────────────────┘ │
│ 123 Main Street            │
│ Union, NJ 07083            │
│                            │
│ $450,000                   │
│ 3 bed · 2 bath             │
└────────────────────────────┘
```

- Photo: 16:9 aspect ratio, placeholder if none
- Price: Bold, prominent
- Card: Subtle shadow, hover effect
- Responsive: 3 columns desktop, 2 tablet, 1 mobile

### Color Scheme

Use existing Ant Design theme colors. Apply Now button should be primary color (prominent).

## Dependencies

- **UN-88** must be completed first (API endpoint)
- Existing S3 photo URLs must be publicly accessible or use signed URLs

## Open Questions

- [x] Which fields to show? → address, price, beds, baths, photo, city
- [x] Grid or list? → Grid
- [x] Filtering? → Not for MVP
- [ ] Should clicking a card show more details? → TBD (could be future enhancement)
- [ ] Hero section content? → TBD (can start without)

## Future Enhancements

- Property detail modal/page (click card for more info)
- Filtering by price, city, bedrooms
- Map view
- Save favorites (requires applicant accounts)

## Implementation Order

1. **UN-88**: Create public API endpoint
2. **UN-87**: Create frontend components and routing
3. **UN-89**: (Optional) Role-based redirect improvements

## Decision Log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-01-27 | Grid layout over list | More visual, better for property browsing |
| 2026-01-27 | No filtering for MVP | Keep scope small, add later if needed |
| 2026-01-27 | Split into 3 tickets | Separation of concerns, can parallelize |
