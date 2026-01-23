using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Properties.Commands.AddPropertyPhoto;

public class AddPropertyPhotoCommandHandler : IRequestHandler<AddPropertyPhotoCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentStorageService _storage;

    public AddPropertyPhotoCommandHandler(
        IApplicationDbContext context,
        IDocumentStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<Guid> Handle(AddPropertyPhotoCommand request, CancellationToken cancellationToken)
    {
        var property = await _context.Set<Property>()
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId && !p.IsDeleted, cancellationToken);

        if (property == null)
        {
            throw new NotFoundException(nameof(Property), request.PropertyId);
        }

        if (property.Photos.Count >= 50)
        {
            throw new ValidationException("Maximum 50 photos allowed per property");
        }

        // Upload to S3
        var fileExtension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var storageKey = $"properties/{request.PropertyId}/photos/{Guid.NewGuid()}{fileExtension}";

        var uploadResult = await _storage.UploadAsync(
            request.FileStream,
            storageKey,
            request.ContentType,
            cancellationToken);

        // Create photo record
        var displayOrder = property.Photos.Count;
        var photo = PropertyPhoto.Create(
            request.PropertyId,
            uploadResult.StorageUrl,
            request.Description,
            displayOrder);

        // Explicitly add to context to ensure EF Core treats it as a new entity
        _context.Add(photo);
        property.AddPhoto(photo);
        await _context.SaveChangesAsync(cancellationToken);

        return photo.Id;
    }
}
