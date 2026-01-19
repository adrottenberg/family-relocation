using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Commands.SetBoardDecision;

/// <summary>
/// Handles the SetBoardDecisionCommand to record the board's decision on an applicant.
/// </summary>
public class SetBoardDecisionCommandHandler : IRequestHandler<SetBoardDecisionCommand, SetBoardDecisionResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SetBoardDecisionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SetBoardDecisionResponse> Handle(
        SetBoardDecisionCommand command,
        CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .FirstOrDefaultAsync(a => a.Id == command.ApplicantId, cancellationToken)
            ?? throw new NotFoundException("Applicant", command.ApplicantId);

        var housingSearch = applicant.HousingSearch
            ?? throw new InvalidOperationException("Applicant has no HousingSearch");

        // Validate: Can only set board decision when in Submitted stage
        if (housingSearch.Stage != HousingSearchStage.Submitted)
            throw new ValidationException(
                $"Can only set board decision for applicants in Submitted stage. " +
                $"Current stage: {housingSearch.Stage}");

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to set board decision.");

        var request = command.Request;
        var previousStage = housingSearch.Stage;

        // Set the board decision
        applicant.SetBoardDecision(
            decision: request.Decision,
            notes: request.Notes,
            reviewedByUserId: userId,
            reviewDate: request.ReviewDate);

        // Transition stage based on decision
        NextStepsInfo? nextSteps = null;
        switch (request.Decision)
        {
            case BoardDecision.Approved:
                housingSearch.ApproveBoardReview(userId);
                nextSteps = BuildNextStepsInfo(housingSearch);
                break;

            case BoardDecision.Rejected:
                housingSearch.Reject(request.Notes, userId);
                break;

            // Deferred and Pending don't change stage
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SetBoardDecisionResponse
        {
            ApplicantId = applicant.Id,
            HousingSearchId = housingSearch.Id,
            BoardReview = new BoardReviewDto
            {
                Decision = applicant.BoardReview!.Decision.ToString(),
                ReviewDate = applicant.BoardReview.ReviewDate,
                Notes = applicant.BoardReview.Notes,
                ReviewedBy = applicant.BoardReview.ReviewedByUserId
            },
            PreviousStage = previousStage.ToString(),
            NewStage = housingSearch.Stage.ToString(),
            Message = GetNextStepMessage(request.Decision, nextSteps?.ReadyForHouseHunting ?? false),
            NextSteps = nextSteps
        };
    }

    private static NextStepsInfo BuildNextStepsInfo(HousingSearch housingSearch)
    {
        var brokerSigned = housingSearch.BrokerAgreementSignedDate.HasValue;
        var takanosSigned = housingSearch.CommunityTakanosSignedDate.HasValue;

        return new NextStepsInfo
        {
            BrokerAgreementSigned = brokerSigned,
            CommunityTakanosSigned = takanosSigned,
            ReadyForHouseHunting = brokerSigned && takanosSigned
        };
    }

    private static string GetNextStepMessage(BoardDecision decision, bool readyForHouseHunting) => decision switch
    {
        BoardDecision.Approved when readyForHouseHunting =>
            "Board approved. All agreements signed. Ready to start house hunting.",
        BoardDecision.Approved =>
            "Board approved. Awaiting signed broker agreement and community takanos before house hunting can begin.",
        BoardDecision.Rejected =>
            "Application rejected.",
        BoardDecision.Deferred =>
            "Decision deferred. Applicant remains in Submitted stage for future review.",
        BoardDecision.Pending =>
            "Decision set to pending.",
        _ => "Decision recorded."
    };
}
