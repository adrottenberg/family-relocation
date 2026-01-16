using FamilyRelocation.Application.Applicants.Queries.ExistsByEmail;
using FamilyRelocation.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Infrastructure.QueryHandlers;

public class ExistsByEmailQueryHandler : IRequestHandler<ExistsByEmailQuery, bool>
{
    private readonly ApplicationDbContext _context;

    public ExistsByEmailQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ExistsByEmailQuery request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLowerInvariant();

        // Check both husband and wife emails using raw SQL query since they are stored as JSONB
        var exists = await _context.Applicants
            .FromSqlRaw(
                @"SELECT * FROM ""Applicants""
                  WHERE ""IsDeleted"" = false
                  AND (
                      (""Husband""->'Email'->>'Value' IS NOT NULL AND LOWER(""Husband""->'Email'->>'Value') = {0})
                      OR (""Wife""->'Email'->>'Value' IS NOT NULL AND LOWER(""Wife""->'Email'->>'Value') = {0})
                  )",
                normalizedEmail)
            .AnyAsync(cancellationToken);

        return exists;
    }
}
