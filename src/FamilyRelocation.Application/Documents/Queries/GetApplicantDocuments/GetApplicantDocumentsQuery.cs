using FamilyRelocation.Application.Documents.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Documents.Queries.GetApplicantDocuments;

/// <summary>
/// Query to get all documents for an applicant.
/// </summary>
/// <param name="ApplicantId">The applicant ID to get documents for.</param>
public record GetApplicantDocumentsQuery(Guid ApplicantId) : IRequest<List<ApplicantDocumentDto>>;
