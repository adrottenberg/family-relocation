using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Properties.Commands.DeletePropertyPhoto;

public class DeletePropertyPhotoCommandHandler : IRequestHandler<DeletePropertyPhotoCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePropertyPhotoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePropertyPhotoCommand request, CancellationToken cancellationToken)
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

        property.RemovePhoto(request.PhotoId);
        await _context.SaveChangesAsync(cancellationToken);

        // Note: We don't delete from S3 immediately - a background job could clean up orphaned files
    }
}
