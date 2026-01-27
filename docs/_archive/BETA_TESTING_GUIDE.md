# FamilyRelocation CRM - Beta Testing Guide

## Overview

FamilyRelocation is a CRM for managing Chasidishe family relocation to Union, NJ. It helps the Vaad track applicants through their entire relocation journey - from initial application through board approval, house hunting, and successful placement.

## Access

**URL:** https://dev.unionvaad.com

**Login:** Use your provided Cognito credentials (email/password)

---

## Typical Workflows

### 1. New Family Application

**When a family contacts the Vaad about relocating:**

1. Navigate to **Applicants** → **Add New Applicant**
2. Enter family information:
   - Husband's name, email, phone
   - Wife's name, email, phone
   - Current location
   - Number of children
3. Record their initial housing preferences:
   - Budget range
   - Minimum bedrooms/bathrooms
   - City preference (Union, Roselle Park, or no preference)
   - Shabbos shul preference
4. Save the applicant - they start in **New** stage

### 2. Board Review Process

**When the board needs to review an applicant:**

1. Go to **Applicants** → select the family
2. Review their information and any notes
3. After the board meeting, update the **Board Decision**:
   - **Approved** - family can proceed to house hunting
   - **Waitlist** - pending further review
   - **Declined** - with reason noted
4. Approved families automatically move to **House Hunting** stage

### 3. House Hunting

**Helping an approved family find a home:**

1. The system automatically matches properties to the family's preferences
2. View matches in the family's **Housing Search** tab
3. High-scoring matches (70%+) are highlighted
4. For each potential property:
   - Review the match score breakdown
   - Schedule a showing if interested
   - Record family feedback after viewing

### 4. Scheduling & Managing Showings

**When a family wants to see a property:**

1. Go to **Showings** → **Schedule New Showing**
2. Select the applicant and property
3. Set date/time and any notes
4. After the showing, update the status:
   - **Interested** - family wants to proceed
   - **Not Interested** - with feedback noted
   - **Cancelled** - showing didn't happen

### 5. Property Management

**Adding new properties to the system:**

1. Navigate to **Properties** → **Add Property**
2. Enter property details:
   - Address and city
   - Price, bedrooms, bathrooms
   - Features (garage, basement, yard, etc.)
   - Upload photos
3. Save - the system will automatically calculate matches with all active applicants

### 6. Successful Placement

**When a family goes under contract:**

1. Update the Housing Search to **Under Contract** stage
2. Record the property and contract details
3. If the contract falls through:
   - The failed contract is preserved in history
   - Family returns to **House Hunting** stage
4. Upon closing, mark as **Placed** - congratulations!

---

## Key Concepts

| Term | Definition |
|------|------------|
| **Applicant** | A family applying to relocate |
| **Housing Search** | The journey record for a family's house hunting |
| **Property** | A home listing in the system |
| **Showing** | A scheduled property viewing |
| **Match Score** | Percentage indicating how well a property fits a family's preferences |
| **Stage** | Where a family is in the process: New → Board Review → House Hunting → Under Contract → Placed |

---

## Tips for Beta Testers

- **Test the full workflow** - Create a test applicant and walk them through the entire process
- **Try the property matching** - Add different preferences and see how scores change
- **Check mobile view** - The app is desktop-first, but let us know if you need mobile access
- **Note any confusing UI** - If something isn't intuitive, that's valuable feedback

## Reporting Issues

Please report any bugs or feedback to: [your email/issue tracker]

Include:
- What you were trying to do
- What happened instead
- Screenshots if applicable
- Browser you're using

---

*This is a beta release (v0.1.0-dev). Features may change based on your feedback.*
