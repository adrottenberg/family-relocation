using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Queries.ExistsByEmail;

public class ExistsByEmailQueryHandler : IRequestHandler<ExistsByEmailQuery, bool>
{
    private readonly IApplicationDbContext _context;

    public ExistsByEmailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ExistsByEmailQuery request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLowerInvariant();

        return await _context.Set<Applicant>()
            .AnyAsync(a =>
                a.Husband.Email == normalizedEmail ||
                (a.Wife != null && a.Wife.Email == normalizedEmail),
                cancellationToken);
    }
}
