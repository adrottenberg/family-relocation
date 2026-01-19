using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Applicants.Queries.ExistsByEmail;
using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.CreateApplicant;

/// <summary>
/// Handles the CreateApplicantCommand to create a new applicant and their housing search.
/// </summary>
public class CreateApplicantCommandHandler : IRequestHandler<CreateApplicantCommand, CreateApplicantResponse>
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the handler.
    /// </summary>
    public CreateApplicantCommandHandler(
        IMediator mediator,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <inheritdoc />
    public async Task<CreateApplicantResponse> Handle(CreateApplicantCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate emails (husband and wife)
        if (!string.IsNullOrEmpty(request.Husband.Email))
        {
            var exists = await _mediator.Send(new ExistsByEmailQuery(request.Husband.Email), cancellationToken);
            if (exists)
            {
                throw new DuplicateEmailException(request.Husband.Email);
            }
        }

        if (!string.IsNullOrEmpty(request.Wife?.Email))
        {
            var exists = await _mediator.Send(new ExistsByEmailQuery(request.Wife.Email), cancellationToken);
            if (exists)
            {
                throw new DuplicateEmailException(request.Wife.Email);
            }
        }

        // Determine CreatedBy (self-submitted if anonymous)
        var createdBy = _currentUserService.UserId ?? WellKnownIds.SelfSubmittedUserId;

        // Map DTOs to domain objects
        var husband = request.Husband.ToDomain();
        var wife = request.Wife?.ToDomain();
        var address = request.Address?.ToDomain();
        var children = request.Children?.Select(c => c.ToDomain()).ToList();

        // Create Applicant
        var applicant = Applicant.Create(
            husband: husband,
            wife: wife,
            address: address,
            children: children,
            currentKehila: request.CurrentKehila,
            shabbosShul: request.ShabbosShul,
            createdBy: createdBy);

        // Create HousingSearch (automatically created with Applicant)
        var housingSearch = HousingSearch.Create(
            applicantId: applicant.Id,
            createdBy: createdBy);

        // Apply housing preferences if provided
        if (request.HousingPreferences != null)
        {
            var preferences = request.HousingPreferences.ToDomain();
            housingSearch.UpdatePreferences(preferences, createdBy);
        }

        // Save both in same transaction
        _context.Add(applicant);
        _context.Add(housingSearch);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateApplicantResponse
        {
            ApplicantId = applicant.Id,
            HousingSearchId = housingSearch.Id,
            Applicant = applicant.ToDto()
        };
    }
}
