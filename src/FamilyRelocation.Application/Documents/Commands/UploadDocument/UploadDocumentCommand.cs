using FamilyRelocation.Application.Documents.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Documents.Commands.UploadDocument;

/// <summary>
/// Command to upload a document for an applicant.
/// </summary>
public record UploadDocumentCommand(
    Guid ApplicantId,
    Guid DocumentTypeId,
    string FileName,
    string StorageKey,
    string ContentType,
    long FileSizeBytes) : IRequest<ApplicantDocumentDto>;
