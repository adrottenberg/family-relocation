using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Commands.RecordAgreement;

/// <summary>
/// Handles the RecordAgreementCommand to record that an applicant has signed an agreement.
/// </summary>
public class RecordAgreementCommandHandler : IRequestHandler<RecordAgreementCommand, RecordAgreementResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the handler.
    /// </summary>
    public RecordAgreementCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <inheritdoc />
    public async Task<RecordAgreementResponse> Handle(RecordAgreementCommand command, CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .FirstOrDefaultAsync(a => a.Id == command.ApplicantId, cancellationToken)
            ?? throw new NotFoundException("Applicant", command.ApplicantId);

        if (applicant.HousingSearch == null)
            throw new NotFoundException("HousingSearch for Applicant", command.ApplicantId);

        var request = command.Request;
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to record agreement.");

        if (string.IsNullOrWhiteSpace(request.DocumentUrl))
            throw new ValidationException("Document URL is required.");

        var housingSearch = applicant.HousingSearch;

        switch (request.AgreementType)
        {
            case AgreementTypes.BrokerAgreement:
                housingSearch.RecordBrokerAgreementSigned(request.DocumentUrl, userId);
                break;

            case AgreementTypes.CommunityTakanos:
                housingSearch.RecordCommunityTakanosSigned(request.DocumentUrl, userId);
                break;

            default:
                throw new ValidationException(
                    $"Invalid agreement type: {request.AgreementType}. " +
                    $"Valid types: {AgreementTypes.BrokerAgreement}, {AgreementTypes.CommunityTakanos}");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new RecordAgreementResponse
        {
            BrokerAgreementSigned = housingSearch.BrokerAgreementSigned,
            CommunityTakanosSigned = housingSearch.CommunityTakanosSigned,
            AllAgreementsSigned = housingSearch.AreAgreementsSigned
        };
    }
}
