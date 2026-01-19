using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Commands.ChangeStage;

/// <summary>
/// Handles the ChangeStageCommand to transition a housing search to a new stage.
/// </summary>
public class ChangeStageCommandHandler : IRequestHandler<ChangeStageCommand, ChangeStageResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the handler.
    /// </summary>
    public ChangeStageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <inheritdoc />
    public async Task<ChangeStageResponse> Handle(ChangeStageCommand command, CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .FirstOrDefaultAsync(a => a.Id == command.ApplicantId, cancellationToken)
            ?? throw new NotFoundException("Applicant", command.ApplicantId);

        if (applicant.HousingSearch == null)
            throw new NotFoundException("HousingSearch for Applicant", command.ApplicantId);

        var request = command.Request;
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to change stage.");

        if (!Enum.TryParse<HousingSearchStage>(request.NewStage, ignoreCase: true, out var targetStage))
            throw new ValidationException($"Invalid stage: {request.NewStage}");

        var housingSearch = applicant.HousingSearch;

        // Route to appropriate domain method based on target stage
        switch (targetStage)
        {
            case HousingSearchStage.HouseHunting:
                TransitionToHouseHunting(applicant, housingSearch, request, userId);
                break;

            case HousingSearchStage.Rejected:
                housingSearch.Reject(request.Reason, userId);
                break;

            case HousingSearchStage.Paused:
                housingSearch.Pause(request.Reason, userId);
                break;

            case HousingSearchStage.UnderContract:
                TransitionToUnderContract(housingSearch, request, userId);
                break;

            case HousingSearchStage.Closed:
                TransitionToClosed(housingSearch, request, userId);
                break;

            case HousingSearchStage.MovedIn:
                TransitionToMovedIn(housingSearch, request, userId);
                break;

            default:
                throw new ValidationException($"Cannot transition to stage: {targetStage}");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ChangeStageResponse
        {
            Stage = housingSearch.Stage.ToString(),
            StageChangedDate = housingSearch.StageChangedDate
        };
    }

    private static void TransitionToHouseHunting(
        Applicant applicant,
        HousingSearch housingSearch,
        ChangeStageRequest request,
        Guid userId)
    {
        switch (housingSearch.Stage)
        {
            case HousingSearchStage.Submitted:
                // Board approval checked here; agreements checked in domain
                if (applicant.BoardReview?.Decision != BoardDecision.Approved)
                    throw new ValidationException(
                        "Cannot start house hunting until applicant is approved by the board.");
                housingSearch.StartHouseHunting(userId);
                break;

            case HousingSearchStage.Paused:
                housingSearch.Resume(userId);
                break;

            case HousingSearchStage.UnderContract:
            case HousingSearchStage.Closed:
                housingSearch.ContractFellThrough(request.Reason, userId);
                break;

            default:
                throw new ValidationException(
                    $"Cannot transition from {housingSearch.Stage} to HouseHunting.");
        }
    }

    private static void TransitionToUnderContract(
        HousingSearch housingSearch,
        ChangeStageRequest request,
        Guid userId)
    {
        if (request.Contract == null)
            throw new ValidationException("Contract details are required to put under contract.");

        if (request.Contract.Price <= 0)
            throw new ValidationException("Contract price must be greater than zero.");

        var propertyId = request.Contract.PropertyId ?? Guid.Empty;
        var price = new Money(request.Contract.Price);

        housingSearch.PutUnderContract(
            propertyId,
            price,
            request.Contract.ExpectedClosingDate,
            userId);
    }

    private static void TransitionToClosed(
        HousingSearch housingSearch,
        ChangeStageRequest request,
        Guid userId)
    {
        if (!request.ClosingDate.HasValue)
            throw new ValidationException("Closing date is required to record closing.");

        housingSearch.RecordClosing(request.ClosingDate.Value, userId);
    }

    private static void TransitionToMovedIn(
        HousingSearch housingSearch,
        ChangeStageRequest request,
        Guid userId)
    {
        if (!request.MovedInDate.HasValue)
            throw new ValidationException("Move-in date is required to record move-in.");

        housingSearch.RecordMovedIn(request.MovedInDate.Value, userId);
    }
}
