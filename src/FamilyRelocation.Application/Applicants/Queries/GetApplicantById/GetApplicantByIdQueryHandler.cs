using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Queries.GetApplicantById;

public class GetApplicantByIdQueryHandler : IRequestHandler<GetApplicantByIdQuery, ApplicantDto?>
{
    private readonly IApplicationDbContext _context;

    public GetApplicantByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicantDto?> Handle(GetApplicantByIdQuery request, CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        return applicant?.ToDto();
    }
}
