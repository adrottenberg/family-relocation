using FamilyRelocation.Domain.Entities;

namespace FamilyRelocation.Application.Common.Interfaces;

public interface IApplicantRepository
{
    Task<Applicant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(Applicant applicant, CancellationToken cancellationToken = default);
    void Update(Applicant applicant);
    void Delete(Applicant applicant);
}
