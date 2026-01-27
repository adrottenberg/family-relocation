using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Documents.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Documents.Commands.UploadDocument;

/// <summary>
/// Handler for UploadDocumentCommand.
/// </summary>
public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, ApplicantDocumentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogger _activityLogger;

    public UploadDocumentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IActivityLogger activityLogger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _activityLogger = activityLogger;
    }

    public async Task<ApplicantDocumentDto> Handle(UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        // Verify applicant exists
        var applicantExists = await _context.Set<Applicant>()
            .AnyAsync(a => a.Id == command.ApplicantId, cancellationToken);

        if (!applicantExists)
            throw new NotFoundException("Applicant", command.ApplicantId);

        // Verify document type exists and is active
        var documentType = await _context.Set<DocumentType>()
            .FirstOrDefaultAsync(dt => dt.Id == command.DocumentTypeId, cancellationToken)
            ?? throw new NotFoundException("DocumentType", command.DocumentTypeId);

        if (!documentType.IsActive)
            throw new ValidationException("Document type is not active.");

        // Check if document already exists for this applicant and type
        var existingDocument = await _context.Set<ApplicantDocument>()
            .Include(d => d.DocumentType)
            .FirstOrDefaultAsync(d =>
                d.ApplicantId == command.ApplicantId &&
                d.DocumentTypeId == command.DocumentTypeId,
                cancellationToken);

        var userId = _currentUserService.UserId;

        if (existingDocument != null)
        {
            // Update existing document
            existingDocument.UpdateStorage(
                command.StorageKey,
                command.FileName,
                command.ContentType,
                command.FileSizeBytes,
                userId);

            await _context.SaveChangesAsync(cancellationToken);

            // Log activity
            await _activityLogger.LogAsync(
                "Applicant",
                command.ApplicantId,
                "DocumentUpdated",
                $"Updated document: {documentType.DisplayName} ({command.FileName})",
                cancellationToken);

            // Check for auto-transition (in case this was a re-upload that completes requirements)
            await TryAutoTransitionToSearchingAsync(command.ApplicantId, cancellationToken);

            return new ApplicantDocumentDto
            {
                Id = existingDocument.Id,
                DocumentTypeId = existingDocument.DocumentTypeId,
                DocumentTypeName = existingDocument.DocumentType.DisplayName,
                FileName = existingDocument.FileName,
                StorageKey = existingDocument.StorageKey,
                ContentType = existingDocument.ContentType,
                FileSizeBytes = existingDocument.FileSizeBytes,
                UploadedAt = existingDocument.UploadedAt,
                UploadedBy = existingDocument.UploadedBy
            };
        }

        // Create new document
        var newDocument = ApplicantDocument.Create(
            command.ApplicantId,
            command.DocumentTypeId,
            command.FileName,
            command.StorageKey,
            command.ContentType,
            command.FileSizeBytes,
            userId);

        _context.Add(newDocument);
        await _context.SaveChangesAsync(cancellationToken);

        // Log activity
        await _activityLogger.LogAsync(
            "Applicant",
            command.ApplicantId,
            "DocumentUploaded",
            $"Uploaded document: {documentType.DisplayName} ({command.FileName})",
            cancellationToken);

        // Check for auto-transition from AwaitingAgreements to Searching
        await TryAutoTransitionToSearchingAsync(command.ApplicantId, cancellationToken);

        return new ApplicantDocumentDto
        {
            Id = newDocument.Id,
            DocumentTypeId = newDocument.DocumentTypeId,
            DocumentTypeName = documentType.DisplayName,
            FileName = newDocument.FileName,
            StorageKey = newDocument.StorageKey,
            ContentType = newDocument.ContentType,
            FileSizeBytes = newDocument.FileSizeBytes,
            UploadedAt = newDocument.UploadedAt,
            UploadedBy = newDocument.UploadedBy
        };
    }

    /// <summary>
    /// Checks if all required documents for AwaitingAgreements → Searching transition are uploaded,
    /// and if so, automatically transitions the housing search to the Searching stage.
    /// </summary>
    private async Task TryAutoTransitionToSearchingAsync(Guid applicantId, CancellationToken cancellationToken)
    {
        // Get the applicant's active housing search in AwaitingAgreements stage
        var housingSearch = await _context.Set<HousingSearch>()
            .FirstOrDefaultAsync(h =>
                h.ApplicantId == applicantId &&
                h.Stage == HousingSearchStage.AwaitingAgreements,
                cancellationToken);

        if (housingSearch == null)
            return; // No housing search in AwaitingAgreements stage

        // Get required document types for AwaitingAgreements → Searching transition
        var requiredDocTypeIds = await _context.Set<StageTransitionRequirement>()
            .Where(r =>
                r.FromStage == HousingSearchStage.AwaitingAgreements &&
                r.ToStage == HousingSearchStage.Searching &&
                r.IsRequired)
            .Select(r => r.DocumentTypeId)
            .ToListAsync(cancellationToken);

        if (requiredDocTypeIds.Count == 0)
            return; // No required documents configured

        // Get uploaded document type IDs for this applicant
        var uploadedDocTypeIds = await _context.Set<ApplicantDocument>()
            .Where(d => d.ApplicantId == applicantId)
            .Select(d => d.DocumentTypeId)
            .ToListAsync(cancellationToken);

        // Check if all required documents are uploaded
        var allRequiredUploaded = requiredDocTypeIds.All(reqId => uploadedDocTypeIds.Contains(reqId));

        if (!allRequiredUploaded)
            return; // Not all required documents uploaded yet

        var userId = _currentUserService.UserId;
        if (userId == null)
            return; // No authenticated user to attribute the change to

        // Auto-transition to Searching stage
        housingSearch.StartSearching(userId.Value);
        await _context.SaveChangesAsync(cancellationToken);

        // Log the auto-transition
        await _activityLogger.LogAsync(
            "HousingSearch",
            housingSearch.Id,
            "StageChanged",
            "Automatically transitioned to Searching stage (all required documents uploaded)",
            cancellationToken);
    }
}
