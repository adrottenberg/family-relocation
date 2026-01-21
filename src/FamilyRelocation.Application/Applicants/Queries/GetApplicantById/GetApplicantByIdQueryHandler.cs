using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Queries.GetApplicantById;

/// <summary>
/// Handles the GetApplicantByIdQuery to retrieve an applicant by their ID.
/// </summary>
public class GetApplicantByIdQueryHandler : IRequestHandler<GetApplicantByIdQuery, ApplicantDto?>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the handler.
    /// </summary>
    public GetApplicantByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ApplicantDto?> Handle(GetApplicantByIdQuery request, CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearches)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        return applicant?.ToDto();
    }
}
