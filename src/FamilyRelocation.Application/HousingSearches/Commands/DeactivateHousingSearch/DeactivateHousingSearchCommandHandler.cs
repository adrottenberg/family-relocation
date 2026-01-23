using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.HousingSearches.Commands.DeactivateHousingSearch;

/// <summary>
/// Handles the DeactivateHousingSearchCommand to deactivate a housing search.
/// </summary>
public class DeactivateHousingSearchCommandHandler : IRequestHandler<DeactivateHousingSearchCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateHousingSearchCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeactivateHousingSearchCommand command, CancellationToken cancellationToken)
    {
        var housingSearch = await _context.Set<HousingSearch>()
            .FirstOrDefaultAsync(h => h.Id == command.HousingSearchId, cancellationToken)
            ?? throw new NotFoundException("HousingSearch", command.HousingSearchId);

        var userId = _currentUserService.UserId ?? Guid.Empty;
        housingSearch.Deactivate(userId);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
