using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Documents.Commands.DeleteStageRequirement;

public class DeleteStageRequirementCommandHandler : IRequestHandler<DeleteStageRequirementCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteStageRequirementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteStageRequirementCommand request, CancellationToken cancellationToken)
    {
        var requirement = await _context.Set<StageTransitionRequirement>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (requirement is null)
            return false;

        _context.Remove(requirement);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
