using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Queries.ExistsByEmail;

/// <summary>
/// Handles the ExistsByEmailQuery to check if an email is already in use.
/// </summary>
public class ExistsByEmailQueryHandler : IRequestHandler<ExistsByEmailQuery, bool>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the handler.
    /// </summary>
    public ExistsByEmailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
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
