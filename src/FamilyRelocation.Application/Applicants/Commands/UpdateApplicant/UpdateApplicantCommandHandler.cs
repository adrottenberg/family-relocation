using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Commands.UpdateApplicant;

public class UpdateApplicantCommandHandler : IRequestHandler<UpdateApplicantCommand, ApplicantDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateApplicantCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApplicantDto> Handle(UpdateApplicantCommand request, CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (applicant == null)
        {
            throw new NotFoundException("Applicant", request.Id);
        }

        // Check for duplicate emails (excluding current applicant)
        await ValidateEmailUniqueness(request, applicant.Id, cancellationToken);

        var userId = _currentUserService.UserId ?? Guid.Empty;

        // Update husband info
        applicant.UpdateHusband(request.Husband.ToDomain(), userId);

        // Update wife info
        applicant.UpdateWife(request.Wife?.ToDomain(), userId);

        // Update address
        applicant.UpdateAddress(request.Address?.ToDomain(), userId);

        // Update children
        var children = request.Children?.Select(c => c.ToDomain()).ToList() ?? new List<Child>();
        applicant.UpdateChildren(children, userId);

        // Update community info
        applicant.UpdateCommunityInfo(request.CurrentKehila, request.ShabbosShul, userId);

        await _context.SaveChangesAsync(cancellationToken);

        return applicant.ToDto();
    }

    private async Task ValidateEmailUniqueness(UpdateApplicantCommand request, Guid currentApplicantId, CancellationToken cancellationToken)
    {
        // Check husband email
        if (!string.IsNullOrEmpty(request.Husband.Email))
        {
            var normalizedEmail = request.Husband.Email.ToLowerInvariant();
            var emailExists = await _context.Set<Applicant>()
                .AnyAsync(a => a.Id != currentApplicantId &&
                    (a.Husband.Email == normalizedEmail ||
                     (a.Wife != null && a.Wife.Email == normalizedEmail)),
                    cancellationToken);

            if (emailExists)
            {
                throw new DuplicateEmailException(request.Husband.Email);
            }
        }

        // Check wife email
        if (!string.IsNullOrEmpty(request.Wife?.Email))
        {
            var normalizedEmail = request.Wife.Email.ToLowerInvariant();
            var emailExists = await _context.Set<Applicant>()
                .AnyAsync(a => a.Id != currentApplicantId &&
                    (a.Husband.Email == normalizedEmail ||
                     (a.Wife != null && a.Wife.Email == normalizedEmail)),
                    cancellationToken);

            if (emailExists)
            {
                throw new DuplicateEmailException(request.Wife.Email);
            }
        }
    }
}
