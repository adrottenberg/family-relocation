using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Commands.RecordAgreement;

/// <summary>
/// Handles the RecordAgreementCommand to record that an applicant has uploaded a signed agreement.
/// This creates or updates an ApplicantDocument record.
/// </summary>
public class RecordAgreementCommandHandler : IRequestHandler<RecordAgreementCommand, RecordAgreementResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RecordAgreementCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<RecordAgreementResponse> Handle(RecordAgreementCommand command, CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == command.ApplicantId, cancellationToken)
            ?? throw new NotFoundException("Applicant", command.ApplicantId);

        if (applicant.HousingSearch == null)
            throw new NotFoundException("HousingSearch for Applicant", command.ApplicantId);

        var request = command.Request;
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to record agreement.");

        if (string.IsNullOrWhiteSpace(request.DocumentUrl))
            throw new ValidationException("Document URL is required.");

        // Find the document type by name
        Guid documentTypeId = request.AgreementType switch
        {
            AgreementTypes.BrokerAgreement => WellKnownIds.BrokerAgreementDocumentTypeId,
            AgreementTypes.CommunityTakanos => WellKnownIds.CommunityTakanosDocumentTypeId,
            _ => throw new ValidationException(
                $"Invalid agreement type: {request.AgreementType}. " +
                $"Valid types: {AgreementTypes.BrokerAgreement}, {AgreementTypes.CommunityTakanos}")
        };

        // Check if document already exists for this applicant and type
        var existingDocument = applicant.Documents
            .FirstOrDefault(d => d.DocumentTypeId == documentTypeId);

        if (existingDocument != null)
        {
            // Update existing document
            existingDocument.UpdateStorage(
                newStorageKey: ExtractStorageKey(request.DocumentUrl),
                newFileName: ExtractFileName(request.DocumentUrl),
                contentType: "application/pdf", // Default, actual content type set during upload
                fileSizeBytes: 0, // Will be updated from upload metadata
                uploadedBy: userId);
        }
        else
        {
            // Create new document record
            var newDocument = ApplicantDocument.Create(
                applicantId: command.ApplicantId,
                documentTypeId: documentTypeId,
                fileName: ExtractFileName(request.DocumentUrl),
                storageKey: ExtractStorageKey(request.DocumentUrl),
                contentType: "application/pdf",
                fileSizeBytes: 0,
                uploadedBy: userId);

            _context.Add(newDocument);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Get updated document status
        var brokerAgreementExists = applicant.Documents
            .Any(d => d.DocumentTypeId == WellKnownIds.BrokerAgreementDocumentTypeId)
            || documentTypeId == WellKnownIds.BrokerAgreementDocumentTypeId;

        var communityTakanosExists = applicant.Documents
            .Any(d => d.DocumentTypeId == WellKnownIds.CommunityTakanosDocumentTypeId)
            || documentTypeId == WellKnownIds.CommunityTakanosDocumentTypeId;

        return new RecordAgreementResponse
        {
            BrokerAgreementSigned = brokerAgreementExists,
            CommunityTakanosSigned = communityTakanosExists,
            AllAgreementsSigned = brokerAgreementExists && communityTakanosExists
        };
    }

    private static string ExtractStorageKey(string documentUrl)
    {
        // Extract the S3 key from the URL
        try
        {
            var uri = new Uri(documentUrl);
            return uri.AbsolutePath.TrimStart('/');
        }
        catch
        {
            return documentUrl;
        }
    }

    private static string ExtractFileName(string documentUrl)
    {
        try
        {
            var uri = new Uri(documentUrl);
            var path = uri.AbsolutePath;
            return Path.GetFileName(path);
        }
        catch
        {
            return Path.GetFileName(documentUrl) ?? "document.pdf";
        }
    }
}
