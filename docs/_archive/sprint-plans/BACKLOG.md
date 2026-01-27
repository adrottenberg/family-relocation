# PRODUCT BACKLOG
## Family Relocation System - Future User Stories

This document contains user stories that are prioritized for future sprints but not yet scheduled.

---

## BACKLOG ITEMS

### US-060: Public Forms Download Center (3 points) - MEDIUM

**As a** prospective applicant visiting the public application page
**I want to** download required forms (PDF/Word documents) before applying
**So that** I can review requirements and prepare necessary paperwork offline

**Description:**
Create a public-facing forms repository that allows anyone to download application-related documents without authentication. This should be accessible from the public application page.

**Acceptance Criteria:**
- [ ] Public endpoint `GET /api/public/forms` returns list of available forms
- [ ] Each form has: name, description, file type, file size, download URL
- [ ] Forms are stored in S3 with public read access (or presigned URLs)
- [ ] Admin can upload/manage forms via admin panel
- [ ] Public application page shows "Download Forms" section
- [ ] Forms download without requiring login
- [ ] Supports PDF, Word (.docx), and Excel (.xlsx) formats

**Technical Notes:**
- Use separate S3 bucket or folder for public forms
- Consider CDN for faster downloads
- Track download counts for analytics

**UI/UX:**
```
┌─────────────────────────────────────────┐
│  Download Required Forms                │
├─────────────────────────────────────────┤
│  📄 Application Checklist      [Download]│
│     PDF • 245 KB                        │
│                                         │
│  📄 Financial Disclosure Form  [Download]│
│     PDF • 156 KB                        │
│                                         │
│  📄 Reference Letter Template  [Download]│
│     DOCX • 45 KB                        │
└─────────────────────────────────────────┘
```

**API Design:**
```
GET /api/public/forms
Response:
{
  "forms": [
    {
      "id": "guid",
      "name": "Application Checklist",
      "description": "List of all required documents",
      "fileName": "application-checklist.pdf",
      "fileType": "application/pdf",
      "fileSize": 250880,
      "downloadUrl": "https://..."
    }
  ]
}

# Admin endpoints (authenticated)
GET /api/admin/forms
POST /api/admin/forms (upload)
DELETE /api/admin/forms/{id}
```

---

### US-061: Board Member Signature Document Upload (4 points) - HIGH

**As a** board member or coordinator
**I want to** upload scanned approval documents with board member signatures
**So that** we have a digital record of signed approvals for compliance and audit purposes

**Description:**
Enable uploading of scanned documents that contain physical signatures from board members for applicant approvals. These documents serve as official records of board decisions.

**Acceptance Criteria:**
- [ ] New document type: "Board Approval Document" in document types
- [ ] Upload accepts scanned images (PDF, JPG, PNG) of signed documents
- [ ] Document is linked to the applicant and the board decision
- [ ] Only BoardMember or Admin roles can upload approval documents
- [ ] Document metadata includes: signing board member, signature date
- [ ] Documents appear in applicant's document list with special "Signed Approval" badge
- [ ] Board review section shows if signed document is uploaded
- [ ] Validation: Cannot upload approval doc unless board decision is recorded

**Technical Notes:**
- Extend existing document upload infrastructure
- Add `SignedByUserId` and `SignatureDate` fields to document metadata
- Consider OCR for date extraction (future enhancement)

**UI/UX:**
```
Board Review Section (after approval):
┌─────────────────────────────────────────┐
│  Board Decision: ✓ Approved             │
│  Reviewed by: Rabbi Cohen               │
│  Date: January 21, 2026                 │
│                                         │
│  Signed Approval Document:              │
│  ┌─────────────────────────────────┐   │
│  │ 📄 No document uploaded         │   │
│  │    [Upload Signed Approval]     │   │
│  └─────────────────────────────────┘   │
│                                         │
│  OR (when uploaded):                    │
│  ┌─────────────────────────────────┐   │
│  │ ✓ board-approval-cohen.pdf      │   │
│  │   Signed by: Rabbi Cohen        │   │
│  │   Date: Jan 21, 2026            │   │
│  │   [View] [Replace]              │   │
│  └─────────────────────────────────┘   │
└─────────────────────────────────────────┘
```

**API Design:**
```
POST /api/documents/board-approval
Content-Type: multipart/form-data
- file: (binary)
- applicantId: guid
- signedByUserId: guid (optional, defaults to current user)
- signatureDate: date

Response:
{
  "documentId": "guid",
  "applicantId": "guid",
  "documentType": "BoardApproval",
  "signedBy": {
    "userId": "guid",
    "userName": "Rabbi Cohen"
  },
  "signatureDate": "2026-01-21",
  "uploadedAt": "2026-01-21T15:30:00Z"
}

GET /api/applicants/{id}/board-approval-document
Response: Document details or 404 if not uploaded
```

**Database Changes:**
- Add `SignedByUserId` (nullable Guid) to ApplicantDocument
- Add `SignatureDate` (nullable DateTime) to ApplicantDocument
- Create well-known DocumentType for "BoardApproval"

---

## PRIORITY MATRIX

| Story | Business Value | Effort | Priority |
|-------|---------------|--------|----------|
| US-061 | High (Compliance) | Medium | HIGH |
| US-060 | Medium (UX) | Low | MEDIUM |

---

## NOTES

- US-060 and US-061 were requested on January 21, 2026
- US-061 is higher priority due to compliance/audit requirements
- Consider implementing both in same sprint as they're related to document management
- US-060 requires decision on public vs private S3 bucket strategy

---

## FUTURE CONSIDERATIONS

1. **Digital Signatures**: Consider integrating DocuSign or similar for true digital signatures
2. **Form Builder**: Admin tool to create custom forms
3. **Version Control**: Track versions of public forms
4. **Expiration**: Forms could have expiration dates requiring re-download
