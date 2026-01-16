using MediatR;

namespace FamilyRelocation.Application.Applicants.Queries.ExistsByEmail;

/// <summary>
/// Query to check if an email already exists for any applicant (husband or wife)
/// </summary>
public record ExistsByEmailQuery(string Email) : IRequest<bool>;
