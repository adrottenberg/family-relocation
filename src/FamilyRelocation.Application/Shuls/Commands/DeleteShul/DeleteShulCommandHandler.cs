using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Shuls.Commands.DeleteShul;

public class DeleteShulCommandHandler : IRequestHandler<DeleteShulCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogger _activityLogger;

    public DeleteShulCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IActivityLogger activityLogger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _activityLogger = activityLogger;
    }

    public async Task<Unit> Handle(DeleteShulCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to delete a shul");

        var shul = await _context.Set<Shul>()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Shul with ID {request.Id} not found");

        var shulName = shul.Name;

        // Soft delete by deactivating
        shul.Deactivate(currentUserId);

        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(
            entityType: "Shul",
            entityId: shul.Id,
            action: "Deleted",
            description: $"Deleted shul: {shulName}",
            ct: cancellationToken);

        return Unit.Value;
    }
}
