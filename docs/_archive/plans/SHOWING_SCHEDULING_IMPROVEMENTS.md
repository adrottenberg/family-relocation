# Showing Scheduling Improvements Plan

## Approved Approach: Calendar-Based Drag-and-Drop (Phased)

### Phase 1: Single Appointment Drag-and-Drop (Current Sprint)
- Calendar view accessible from Applicant and Property pages
- Drag matches from sidebar onto calendar to schedule
- Shows all showings for visibility/conflict awareness

### Phase 2: Bulk Scheduling (Backlog - Sprint 5)
- Quick actions to schedule multiple at once
- Auto-increment times with 30-minute default interval
- One applicant per batch
- Same broker for all in batch
- Conflict warnings (non-blocking)

---

## Phase 1 Detailed Design

### Access Points

**From Applicant Page:**
- Button: "Schedule Showings" in Suggested Listings tab
- Left sidebar: Matches for this applicant (status: `ShowingRequested`)
- Calendar: Shows ALL showings (all applicants) for context

**From Property Page:**
- Button: "Schedule Showings" in Property Matches section
- Left sidebar: Matches for this property (status: `ShowingRequested`)
- Calendar: Shows ALL showings (all properties) for context

### UI Design

```
┌──────────────────────────────────────────────────────────────────────────┐
│ Schedule Showings                                                   [X]  │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│ ┌─────────────────────┐  ┌─────────────────────────────────────────────┐│
│ │ Pending Matches (3) │  │  ◀  January 2026  ▶                         ││
│ │                     │  │                                              ││
│ │ ┌─────────────────┐ │  │  Sun   Mon   Tue   Wed   Thu   Fri   Sat   ││
│ │ │ ≡ 123 Main St   │ │  │                                              ││
│ │ │   Newark        │ │  │   25    26    27    28    29    30    31    ││
│ │ │   Score: 85%    │ │  │         ●     ●●                            ││
│ │ │   [Drag to cal] │ │  │                                              ││
│ │ └─────────────────┘ │  │   1     2     3     4     5     6     7     ││
│ │                     │  │                                              ││
│ │ ┌─────────────────┐ │  ├─────────────────────────────────────────────┤│
│ │ │ ≡ 456 Oak Ave   │ │  │  Monday, January 27                         ││
│ │ │   Union         │ │  │                                              ││
│ │ │   Score: 78%    │ │  │  09:00  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  ││
│ │ └─────────────────┘ │  │  09:30  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  ││
│ │                     │  │  10:00  ┌─────────────────────────────────┐ ││
│ │ ┌─────────────────┐ │  │         │ 789 Elm St - Cohen (existing)   │ ││
│ │ │ ≡ 789 Elm St    │ │  │         └─────────────────────────────────┘ ││
│ │ │   Roselle Park  │ │  │  10:30  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  ││
│ │ │   Score: 72%    │ │  │  11:00  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  ││
│ │ └─────────────────┘ │  │  11:30  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  ││
│ │                     │  │  12:00  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  ││
│ │                     │  │  ...                                        ││
│ └─────────────────────┘  └─────────────────────────────────────────────┘│
│                                                                          │
│ Drop a match onto a time slot to schedule a showing                      │
└──────────────────────────────────────────────────────────────────────────┘
```

### Component Structure

```
ShowingSchedulerModal/
├── ShowingSchedulerModal.tsx      # Main modal container
├── PendingMatchesSidebar.tsx      # Left sidebar with draggable matches
├── SchedulerCalendar.tsx          # Month view calendar with date selection
├── DayScheduleView.tsx            # Day view with time slots and existing showings
├── DraggableMatchCard.tsx         # Draggable match item
└── TimeSlot.tsx                   # Droppable time slot component
```

### User Flow

1. User clicks "Schedule Showings" button on Applicant or Property page
2. Modal opens with:
   - Left sidebar: Pending matches (draggable cards)
   - Right side: Month calendar (top) + Day schedule (bottom)
3. User clicks a date on the calendar → Day schedule shows that day
4. User drags a match card from sidebar onto a time slot
5. Match is scheduled → Card removed from sidebar → Showing appears on calendar
6. User can continue scheduling more or close modal

### Drag and Drop Behavior

- **Drag source:** Match cards in sidebar
- **Drop targets:** Time slots in day view
- **Drop effect:**
  1. Call `POST /api/showings` with match ID, date, and time
  2. Remove card from sidebar
  3. Add showing to day view
  4. Show success toast
- **Snap:** 15-minute intervals (9:00, 9:15, 9:30, etc.)
- **Time range:** 8:00 AM - 8:00 PM
- **Visual feedback:** Highlight slot on hover, show preview card

### Conflict Handling

- Existing showings displayed as blocks on the day view
- If user drops on occupied slot → Show warning: "There's already a showing at 10:00. Schedule anyway?"
- User can confirm or choose different time
- Different color for:
  - Current applicant/property showings (blue)
  - Other showings (gray)

### Data Requirements

**Sidebar needs:**
- Property matches with `ShowingRequested` status
- Filtered by applicant (from applicant page) or property (from property page)
- Display: Property address, city, match score

**Calendar needs:**
- All showings within visible date range
- Display: Property address, applicant name, time, status
- Color-coded by status (Scheduled=blue, Completed=green, Cancelled=gray)

### API Usage

**Existing endpoints (no changes needed):**
- `GET /api/property-matches?housingSearchId={id}` - For applicant page
- `GET /api/property-matches?propertyId={id}` - For property page
- `GET /api/showings?fromDate={date}&toDate={date}` - For calendar
- `POST /api/showings` - To create showing on drop

---

## Implementation Plan

### Step 1: Create Base Components

**1.1 ShowingSchedulerModal.tsx**
- Props: `open`, `onClose`, `mode` ('applicant' | 'property'), `applicantId?`, `propertyId?`
- Manages state for selected date, pending matches, showings
- Coordinates drag-and-drop context

**1.2 PendingMatchesSidebar.tsx**
- Props: `matches`, `onMatchScheduled`
- Renders list of DraggableMatchCard components
- Shows "All scheduled!" message when empty

**1.3 DraggableMatchCard.tsx**
- Props: `match` (PropertyMatchListDto)
- Uses `@dnd-kit/core` `useDraggable`
- Displays property address, city, score

### Step 2: Create Calendar Components

**2.1 SchedulerCalendar.tsx**
- Props: `selectedDate`, `onDateSelect`, `showingCounts` (map of date → count)
- Month view with navigation
- Dots/badges showing days with showings
- Highlights selected date

**2.2 DayScheduleView.tsx**
- Props: `date`, `showings`, `onDrop`
- Time slots from 8 AM to 8 PM (15-min intervals)
- Renders existing showings as blocks
- Each slot is a drop target

**2.3 TimeSlot.tsx**
- Props: `time`, `showing?`, `isDropTarget`, `onDrop`
- Uses `@dnd-kit/core` `useDroppable`
- Visual states: empty, occupied, hover/active

### Step 3: Integrate Drag-and-Drop

**3.1 Set up DndContext**
- Wrap modal content in `DndContext` from `@dnd-kit/core`
- Handle `onDragEnd` to trigger API call

**3.2 Implement drop handling**
```typescript
const handleDragEnd = async (event: DragEndEvent) => {
  const { active, over } = event;
  if (!over) return;

  const matchId = active.id as string;
  const timeSlot = over.id as string; // e.g., "2026-01-27T10:00"

  // Parse date and time from slot ID
  const [date, time] = parseTimeSlot(timeSlot);

  // Call API
  await scheduleShowing({ propertyMatchId: matchId, scheduledDate: date, scheduledTime: time });

  // Update local state
  removeMatchFromSidebar(matchId);
  refetchShowings();
};
```

### Step 4: Add to Applicant and Property Pages

**4.1 ApplicantDetailPage.tsx**
- Add "Schedule Showings" button in Suggested Listings tab header
- Button enabled when there are `ShowingRequested` matches
- Opens ShowingSchedulerModal in 'applicant' mode

**4.2 PropertyDetailPage.tsx**
- Add "Schedule Showings" button in Property Matches section
- Opens ShowingSchedulerModal in 'property' mode

### Step 5: Polish and Edge Cases

- Loading states while fetching data
- Error handling for failed API calls
- Empty states (no pending matches, no showings)
- Keyboard accessibility for drag-and-drop
- Mobile fallback (click to select, click slot to schedule)

---

## Technical Decisions

### Drag-and-Drop Library: @dnd-kit/core

**Why @dnd-kit:**
- Modern React-first design (hooks-based)
- Excellent TypeScript support
- Lightweight (~10KB gzipped)
- Accessible by default
- Used by many Ant Design projects

**Installation:**
```bash
npm install @dnd-kit/core @dnd-kit/utilities
```

### Time Slot Representation

- Slot IDs: ISO datetime string (e.g., `"2026-01-27T10:00:00"`)
- Easy to parse and compare
- Works with existing API date/time format

### State Management

- Use React Query for showings data (already in place)
- Local state for selected date and drag state
- Optimistic updates on drop (remove from sidebar immediately)

---

## Files to Create

| File | Description |
|------|-------------|
| `src/features/showings/scheduler/ShowingSchedulerModal.tsx` | Main modal component |
| `src/features/showings/scheduler/PendingMatchesSidebar.tsx` | Sidebar with draggable matches |
| `src/features/showings/scheduler/DraggableMatchCard.tsx` | Draggable match card |
| `src/features/showings/scheduler/SchedulerCalendar.tsx` | Month calendar view |
| `src/features/showings/scheduler/DayScheduleView.tsx` | Day schedule with time slots |
| `src/features/showings/scheduler/TimeSlot.tsx` | Individual time slot (droppable) |
| `src/features/showings/scheduler/index.ts` | Barrel export |

## Files to Modify

| File | Change |
|------|--------|
| `src/features/applicants/ApplicantDetailPage.tsx` | Add "Schedule Showings" button |
| `src/features/properties/PropertyDetailPage.tsx` | Add "Schedule Showings" button |
| `package.json` | Add @dnd-kit dependencies |

---

## Phase 2 Backlog (Sprint 5)

Future enhancements for bulk scheduling:
- [ ] Quick actions bar: "Schedule all for same day"
- [ ] Start time + 30-min interval auto-fill
- [ ] Multi-select matches and drag as group
- [ ] Batch API endpoint: `POST /api/showings/batch`
- [ ] Broker assignment (same for all)
- [ ] Conflict warnings with "Schedule anyway" option

---

## Acceptance Criteria (Phase 1)

- [ ] "Schedule Showings" button visible on Applicant page (Suggested Listings tab)
- [ ] "Schedule Showings" button visible on Property page (Matches section)
- [ ] Modal opens with pending matches in sidebar
- [ ] Month calendar displays with showing count indicators
- [ ] Clicking date shows day schedule view
- [ ] Existing showings visible in day view
- [ ] Can drag match from sidebar to time slot
- [ ] Dropping creates showing via API
- [ ] Match removed from sidebar after scheduling
- [ ] New showing appears in day view
- [ ] Success toast shown after scheduling
- [ ] Can schedule multiple matches in one session
- [ ] Modal can be closed and reopened
- [ ] Works for both applicant and property contexts
