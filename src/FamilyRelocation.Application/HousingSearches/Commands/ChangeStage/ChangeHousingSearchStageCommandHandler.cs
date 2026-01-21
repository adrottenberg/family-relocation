using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.HousingSearches.Commands.ChangeStage;

/// <summary>
/// Handles the ChangeHousingSearchStageCommand to transition a housing search to a new stage.
/// Handles search-level stages: AwaitingAgreements, Searching, UnderContract, Closed, MovedIn, Paused.
/// </summary>
public class ChangeHousingSearchStageCommandHandler : IRequestHandler<ChangeHousingSearchStageCommand, ChangeHousingSearchStageResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public ChangeHousingSearchStageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<ChangeHousingSearchStageResponse> Handle(
        ChangeHousingSearchStageCommand command,
        CancellationToken cancellationToken)
    {
        var housingSearch = await _context.Set<HousingSearch>()
            .Include(hs => hs.Applicant)
            .FirstOrDefaultAsync(hs => hs.Id == command.HousingSearchId && hs.IsActive, cancellationToken)
            ?? throw new NotFoundException("HousingSearch", command.HousingSearchId);

        var request = command.Request;
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to change stage.");

        if (!Enum.TryParse<HousingSearchStage>(request.NewStage, ignoreCase: true, out var targetStage))
            throw new ValidationException($"Invalid stage: {request.NewStage}");

        // Route to appropriate domain method based on target stage
        switch (targetStage)
        {
            case HousingSearchStage.Searching:
                TransitionToSearching(housingSearch, request, userId);
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

        // Send notification email for significant stage changes
        var applicant = housingSearch.Applicant;
        var email = applicant?.Husband?.Email;
        if (!string.IsNullOrEmpty(email) && ShouldSendStageEmail(targetStage))
        {
            await _emailService.SendTemplatedEmailAsync(
                email,
                "StageChanged",
                new Dictionary<string, string>
                {
                    ["HusbandFirstName"] = applicant!.Husband.FirstName,
                    ["HusbandLastName"] = applicant.Husband.LastName,
                    ["NewStage"] = FormatStageName(targetStage)
                },
                cancellationToken);
        }

        return new ChangeHousingSearchStageResponse
        {
            HousingSearchId = housingSearch.Id,
            Stage = housingSearch.Stage.ToString(),
            StageChangedDate = housingSearch.StageChangedDate
        };
    }

    private static bool ShouldSendStageEmail(HousingSearchStage stage)
    {
        return stage is HousingSearchStage.Searching
            or HousingSearchStage.UnderContract
            or HousingSearchStage.Closed
            or HousingSearchStage.MovedIn;
    }

    private static string FormatStageName(HousingSearchStage stage) => stage switch
    {
        HousingSearchStage.AwaitingAgreements => "Awaiting Agreements",
        HousingSearchStage.Searching => "Searching",
        HousingSearchStage.UnderContract => "Under Contract",
        HousingSearchStage.Closed => "Closed",
        HousingSearchStage.MovedIn => "Moved In",
        HousingSearchStage.Paused => "Paused",
        _ => stage.ToString()
    };

    private static void TransitionToSearching(
        HousingSearch housingSearch,
        ChangeHousingSearchStageRequest request,
        Guid userId)
    {
        switch (housingSearch.Stage)
        {
            case HousingSearchStage.AwaitingAgreements:
                housingSearch.StartSearching(userId);
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
                    $"Cannot transition from {housingSearch.Stage} to Searching.");
        }
    }

    private static void TransitionToUnderContract(
        HousingSearch housingSearch,
        ChangeHousingSearchStageRequest request,
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
        ChangeHousingSearchStageRequest request,
        Guid userId)
    {
        if (!request.ClosingDate.HasValue)
            throw new ValidationException("Closing date is required to record closing.");

        housingSearch.RecordClosing(request.ClosingDate.Value, userId);
    }

    private static void TransitionToMovedIn(
        HousingSearch housingSearch,
        ChangeHousingSearchStageRequest request,
        Guid userId)
    {
        if (!request.MovedInDate.HasValue)
            throw new ValidationException("Move-in date is required to record move-in.");

        housingSearch.RecordMovedIn(request.MovedInDate.Value, userId);
    }
}
