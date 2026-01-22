using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.PropertyMatches.Commands.RequestShowing;

public class RequestShowingCommandHandler : IRequestHandler<RequestShowingCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RequestShowingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(RequestShowingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        if (request.MatchIds == null || request.MatchIds.Count == 0)
        {
            return 0;
        }

        // Load matches that are in MatchIdentified status
        var matches = await _context.Set<PropertyMatch>()
            .Where(m => request.MatchIds.Contains(m.Id) && m.Status == PropertyMatchStatus.MatchIdentified)
            .ToListAsync(cancellationToken);

        foreach (var match in matches)
        {
            match.RequestShowing(userId);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return matches.Count;
    }
}
