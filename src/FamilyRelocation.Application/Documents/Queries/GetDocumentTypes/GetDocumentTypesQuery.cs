using FamilyRelocation.Application.Documents.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Documents.Queries.GetDocumentTypes;

/// <summary>
/// Query to get all document types.
/// </summary>
/// <param name="ActiveOnly">If true, only return active document types.</param>
public record GetDocumentTypesQuery(bool ActiveOnly = true) : IRequest<List<DocumentTypeDto>>;
