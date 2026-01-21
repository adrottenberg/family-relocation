using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Properties.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Properties.Commands.UpdatePropertyStatus;

public class UpdatePropertyStatusCommandHandler : IRequestHandler<UpdatePropertyStatusCommand, PropertyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActivityLogger _activityLogger;

    public UpdatePropertyStatusCommandHandler(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IActivityLogger activityLogger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<PropertyDto> Handle(UpdatePropertyStatusCommand request, CancellationToken ct)
    {
        var property = await _context.Set<Property>()
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, ct);

        if (property == null)
            throw new NotFoundException(nameof(Property), request.Id);

        if (!Enum.TryParse<ListingStatus>(request.Status, true, out var newStatus))
            throw new ValidationException($"Invalid status: {request.Status}");

        var oldStatus = property.Status;
        property.UpdateStatus(newStatus, Guid.Empty); // TODO: Get from current user context

        await _unitOfWork.SaveChangesAsync(ct);

        await _activityLogger.LogAsync(
            "Property",
            property.Id,
            "StatusChanged",
            $"Property status changed from {oldStatus} to {newStatus}",
            ct);

        return property.ToDto();
    }
}
