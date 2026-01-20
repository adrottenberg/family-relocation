using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;

namespace FamilyRelocation.Application.Documents.Commands.CreateStageRequirement;

public class CreateStageRequirementCommandHandler : IRequestHandler<CreateStageRequirementCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateStageRequirementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStageRequirementCommand request, CancellationToken cancellationToken)
    {
        var requirement = StageTransitionRequirement.Create(
            request.FromStage,
            request.ToStage,
            request.DocumentTypeId,
            request.IsRequired
        );

        _context.Add(requirement);
        await _context.SaveChangesAsync(cancellationToken);

        return requirement.Id;
    }
}
