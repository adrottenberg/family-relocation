using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Commands.DeleteApplicant;

/// <summary>
/// Handles the DeleteApplicantCommand to soft delete an applicant.
/// </summary>
public class DeleteApplicantCommandHandler : IRequestHandler<DeleteApplicantCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the handler.
    /// </summary>
    public DeleteApplicantCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <inheritdoc />
    public async Task Handle(DeleteApplicantCommand command, CancellationToken cancellationToken)
    {
        // Need to bypass the global query filter to find already-deleted applicants
        // or applicants that are about to be deleted
        var applicant = await _context.Set<Applicant>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == command.ApplicantId, cancellationToken)
            ?? throw new NotFoundException("Applicant", command.ApplicantId);

        if (applicant.IsDeleted)
        {
            // Already deleted, nothing to do
            return;
        }

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to delete an applicant.");

        applicant.Delete(userId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
