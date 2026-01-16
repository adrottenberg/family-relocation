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

        // EF Core's ToJson() configuration enables LINQ queries into JSON columns
        var exists = await _context.Set<Applicant>()
            .AnyAsync(a =>
                (a.Husband.Email != null && a.Husband.Email.Value.ToLower() == normalizedEmail) ||
                (a.Wife != null && a.Wife.Email != null && a.Wife.Email.Value.ToLower() == normalizedEmail),
                cancellationToken);

        return exists;
    }
}
