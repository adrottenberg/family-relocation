using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.PropertyMatches.Commands.DeletePropertyMatch;

public class DeletePropertyMatchCommandHandler : IRequestHandler<DeletePropertyMatchCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePropertyMatchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePropertyMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _context.Set<PropertyMatch>()
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

        if (match == null)
        {
            throw new NotFoundException(nameof(PropertyMatch), request.MatchId);
        }

        _context.Remove(match);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
