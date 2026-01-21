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
/// Handles the CreateApplicantCommand to create a new applicant.
/// HousingSearch is created by the domain when the applicant is approved by the board.
/// </summary>
public class CreateApplicantCommandHandler : IRequestHandler<CreateApplicantCommand, CreateApplicantResponse>
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of the handler.
    /// </summary>
    public CreateApplicantCommandHandler(
        IMediator mediator,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _mediator = mediator;
        _context = context;
        _currentUserService = currentUserService;
        _emailService = emailService;
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
        var preferences = request.HousingPreferences?.ToDomain();

        // Create Applicant with preferences (HousingSearch is created by domain when approved)
        var applicant = Applicant.Create(
            husband: husband,
            wife: wife,
            address: address,
            children: children,
            currentKehila: request.CurrentKehila,
            shabbosShul: request.ShabbosShul,
            preferences: preferences,
            createdBy: createdBy);

        // Save applicant - HousingSearch will be created when board approves
        _context.Add(applicant);
        await _context.SaveChangesAsync(cancellationToken);

        // Send confirmation email (fire and forget - don't fail if email fails)
        if (!string.IsNullOrEmpty(request.Husband.Email))
        {
            await _emailService.SendTemplatedEmailAsync(
                request.Husband.Email,
                "ApplicationReceived",
                new Dictionary<string, string>
                {
                    ["FamilyName"] = applicant.FamilyName
                },
                cancellationToken);
        }

        return new CreateApplicantResponse
        {
            ApplicantId = applicant.Id,
            HousingSearchId = null, // HousingSearch created when approved by board
            Applicant = applicant.ToDto()
        };
    }
}
