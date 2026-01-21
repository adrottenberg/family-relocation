using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Properties.Commands.DeleteProperty;

public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogger _activityLogger;

    public DeletePropertyCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IActivityLogger activityLogger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _activityLogger = activityLogger;
    }

    public async Task<Unit> Handle(DeletePropertyCommand command, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var property = await _context.Set<Property>()
            .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (property == null)
            throw new NotFoundException(nameof(Property), command.Id);

        property.Delete(userId);
        await _context.SaveChangesAsync(ct);

        await _activityLogger.LogAsync(
            "Property",
            property.Id,
            "Deleted",
            $"Property deleted: {property.Address.FullAddress}",
            ct);

        return Unit.Value;
    }
}
