using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Commands.SetBoardDecision;

/// <summary>
/// Handles the SetBoardDecisionCommand to record the board's decision on an applicant.
/// If approved, the domain automatically creates the first HousingSearch in AwaitingAgreements stage.
/// </summary>
public class SetBoardDecisionCommandHandler : IRequestHandler<SetBoardDecisionCommand, SetBoardDecisionResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public SetBoardDecisionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<SetBoardDecisionResponse> Handle(
        SetBoardDecisionCommand command,
        CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearches)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == command.ApplicantId, cancellationToken)
            ?? throw new NotFoundException("Applicant", command.ApplicantId);

        // Validate: Can only set board decision when in Submitted status
        if (applicant.Status != ApplicationStatus.Submitted)
            throw new ValidationException(
                $"Can only set board decision for applicants in Submitted status. " +
                $"Current status: {applicant.Status}");

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to set board decision.");

        var request = command.Request;
        var previousStatus = applicant.Status;

        // Set the board decision - domain handles creating HousingSearch if approved
        applicant.SetBoardDecision(
            decision: request.Decision,
            notes: request.Notes,
            reviewedByUserId: userId,
            reviewDate: request.ReviewDate);

        await _context.SaveChangesAsync(cancellationToken);

        // Send notification email based on decision
        var email = applicant.Husband?.Email;
        if (!string.IsNullOrEmpty(email))
        {
            var templateName = request.Decision switch
            {
                BoardDecision.Approved => "BoardDecisionApproved",
                BoardDecision.Rejected => "BoardDecisionRejected",
                BoardDecision.Deferred => "BoardDecisionDeferred",
                _ => null
            };

            if (templateName != null)
            {
                await _emailService.SendTemplatedEmailAsync(
                    email,
                    templateName,
                    new Dictionary<string, string>
                    {
                        ["FamilyName"] = applicant.FamilyName
                    },
                    cancellationToken);
            }
        }

        // Get the housing search (created by domain if approved)
        var housingSearch = applicant.ActiveHousingSearch;

        return new SetBoardDecisionResponse
        {
            ApplicantId = applicant.Id,
            HousingSearchId = housingSearch?.Id,
            BoardReview = new BoardReviewDto
            {
                Decision = applicant.BoardReview!.Decision.ToString(),
                ReviewDate = applicant.BoardReview.ReviewDate,
                Notes = applicant.BoardReview.Notes,
                ReviewedBy = applicant.BoardReview.ReviewedByUserId
            },
            PreviousStage = previousStatus.ToString(),
            NewStage = GetDisplayStage(applicant),
            Message = GetNextStepMessage(request.Decision, applicant),
            NextSteps = BuildNextStepsInfo(applicant.Documents)
        };
    }

    private static string GetDisplayStage(Applicant applicant)
    {
        // Return combined stage for display
        if (applicant.Status == ApplicationStatus.Submitted)
            return "Submitted";
        if (applicant.Status == ApplicationStatus.Rejected)
            return "Rejected";

        // Approved - return housing search stage
        return applicant.ActiveHousingSearch?.Stage.ToString() ?? "Searching";
    }

    private static NextStepsInfo BuildNextStepsInfo(IReadOnlyCollection<ApplicantDocument> documents)
    {
        var brokerSigned = documents.Any(d => d.DocumentTypeId == WellKnownIds.BrokerAgreementDocumentTypeId);
        var takanosSigned = documents.Any(d => d.DocumentTypeId == WellKnownIds.CommunityTakanosDocumentTypeId);

        return new NextStepsInfo
        {
            BrokerAgreementSigned = brokerSigned,
            CommunityTakanosSigned = takanosSigned,
            ReadyForHouseHunting = brokerSigned && takanosSigned
        };
    }

    private static string GetNextStepMessage(BoardDecision decision, Applicant applicant) => decision switch
    {
        BoardDecision.Approved =>
            "Board approved. Applicant can now begin house hunting.",
        BoardDecision.Rejected =>
            "Application rejected.",
        BoardDecision.Deferred =>
            "Decision deferred. Applicant remains in Submitted status for future review.",
        BoardDecision.Pending =>
            "Decision set to pending.",
        _ => "Decision recorded."
    };
}
