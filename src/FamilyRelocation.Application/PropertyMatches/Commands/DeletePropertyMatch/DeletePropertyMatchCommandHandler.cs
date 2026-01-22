using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.PropertyMatches.Commands.DeletePropertyMatch;

public class DeletePropertyMatchCommandHandler : IRequestHandler<DeletePropertyMatchCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IActivityLogger _activityLogger;

    public DeletePropertyMatchCommandHandler(IApplicationDbContext context, IActivityLogger activityLogger)
    {
        _context = context;
        _activityLogger = activityLogger;
    }

    public async Task Handle(DeletePropertyMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _context.Set<PropertyMatch>()
            .Include(m => m.Property)
            .Include(m => m.HousingSearch)
                .ThenInclude(h => h.Applicant)
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

        if (match == null)
        {
            throw new NotFoundException(nameof(PropertyMatch), request.MatchId);
        }

        var familyName = match.HousingSearch.Applicant?.Husband?.LastName ?? "Unknown";
        var propertyAddress = $"{match.Property.Address.Street}, {match.Property.Address.City}";
        var matchId = match.Id;

        _context.Remove(match);
        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(
            "PropertyMatch",
            matchId,
            "Deleted",
            $"Match deleted between {familyName} family and property at {propertyAddress}",
            cancellationToken);
    }
}
