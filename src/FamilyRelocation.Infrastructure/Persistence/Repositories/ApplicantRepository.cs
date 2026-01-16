using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Infrastructure.Persistence.Repositories;

public class ApplicantRepository : IApplicantRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Applicant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicantsDbSet
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();

        // Check husband email using raw SQL query since Husband is stored as JSONB
        var exists = await _context.ApplicantsDbSet
            .FromSqlRaw(
                "SELECT * FROM \"Applicants\" WHERE \"IsDeleted\" = false AND \"Husband\"->>'Email' IS NOT NULL AND LOWER(\"Husband\"->'Email'->>'Value') = {0}",
                normalizedEmail)
            .AnyAsync(cancellationToken);

        return exists;
    }

    public async Task AddAsync(Applicant applicant, CancellationToken cancellationToken = default)
    {
        await _context.ApplicantsDbSet.AddAsync(applicant, cancellationToken);
    }

    public void Update(Applicant applicant)
    {
        _context.ApplicantsDbSet.Update(applicant);
    }

    public void Delete(Applicant applicant)
    {
        // Soft delete - the actual delete happens through the entity's Delete method
        _context.ApplicantsDbSet.Update(applicant);
    }
}
