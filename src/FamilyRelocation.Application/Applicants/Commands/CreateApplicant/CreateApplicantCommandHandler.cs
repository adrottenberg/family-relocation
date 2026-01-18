using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Applicants.Queries.ExistsByEmail;
using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Entities;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.CreateApplicant;

public class CreateApplicantCommandHandler : IRequestHandler<CreateApplicantCommand, ApplicantDto>
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateApplicantCommandHandler(
        IMediator mediator,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApplicantDto> Handle(CreateApplicantCommand request, CancellationToken cancellationToken)
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

        // Map DTOs to domain objects
        var husband = request.Husband.ToDomain();
        var wife = request.Wife?.ToDomain();
        var address = request.Address?.ToDomain();
        var children = request.Children?.Select(c => c.ToDomain()).ToList();

        var applicant = Applicant.Create(
            husband: husband,
            wife: wife,
            address: address,
            children: children,
            currentKehila: request.CurrentKehila,
            shabbosShul: request.ShabbosShul,
            createdBy: _currentUserService.UserId ?? WellKnownIds.SelfSubmittedUserId);

        _context.Add(applicant);
        await _context.SaveChangesAsync(cancellationToken);

        return applicant.ToDto();
    }
}
