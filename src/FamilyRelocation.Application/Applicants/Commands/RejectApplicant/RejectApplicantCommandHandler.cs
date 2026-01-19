using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Commands.RejectApplicant;

/// <summary>
/// Handles the RejectApplicantCommand to transition an applicant to Rejected stage.
/// </summary>
public class RejectApplicantCommandHandler : IRequestHandler<RejectApplicantCommand, RejectApplicantResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RejectApplicantCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<RejectApplicantResponse> Handle(
        RejectApplicantCommand command,
        CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .FirstOrDefaultAsync(a => a.Id == command.ApplicantId, cancellationToken)
            ?? throw new NotFoundException("Applicant", command.ApplicantId);

        var housingSearch = applicant.HousingSearch
            ?? throw new InvalidOperationException("Applicant has no HousingSearch");

        // Validate: Board decision must be Rejected
        if (applicant.BoardReview?.Decision != BoardDecision.Rejected)
            throw new ValidationException(
                $"Cannot reject applicant. Board decision must be 'Rejected' but is " +
                $"'{applicant.BoardReview?.Decision.ToString() ?? "not set"}'.");

        // Validate: Not already rejected
        if (housingSearch.Stage == HousingSearchStage.Rejected)
            throw new ValidationException("Applicant is already in Rejected stage.");

        // Validate: Must be in Submitted stage (can't reject after board approved)
        if (housingSearch.Stage != HousingSearchStage.Submitted)
            throw new ValidationException(
                $"Can only reject from Submitted stage. Current stage: {housingSearch.Stage}");

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to reject applicant.");

        var previousStage = housingSearch.Stage;
        var reason = command.Request?.Reason;

        // Transition to Rejected
        housingSearch.Reject(reason, userId);

        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Future - send notification email if SendNotification is true

        return new RejectApplicantResponse
        {
            ApplicantId = applicant.Id,
            HousingSearchId = housingSearch.Id,
            PreviousStage = previousStage.ToString(),
            NewStage = housingSearch.Stage.ToString(),
            RejectedAt = DateTime.UtcNow,
            RejectedBy = userId,
            Reason = reason
        };
    }
}
