using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Documents.DTOs;
using FamilyRelocation.Domain.Entities;
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
}
