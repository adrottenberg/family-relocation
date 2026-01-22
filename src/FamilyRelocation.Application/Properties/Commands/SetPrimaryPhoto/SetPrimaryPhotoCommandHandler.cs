using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Properties.Commands.SetPrimaryPhoto;

public class SetPrimaryPhotoCommandHandler : IRequestHandler<SetPrimaryPhotoCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SetPrimaryPhotoCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(SetPrimaryPhotoCommand request, CancellationToken cancellationToken)
    {
        var property = await _context.Set<Property>()
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId && !p.IsDeleted, cancellationToken);

        if (property == null)
        {
            throw new NotFoundException(nameof(Property), request.PropertyId);
        }

        var photo = property.Photos.FirstOrDefault(p => p.Id == request.PhotoId);
        if (photo == null)
        {
            throw new NotFoundException("PropertyPhoto", request.PhotoId);
        }

        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
        property.SetPrimaryPhoto(request.PhotoId, userId);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
