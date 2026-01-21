using MediatR;

namespace FamilyRelocation.Application.Properties.Commands.AddPropertyPhoto;

public record AddPropertyPhotoCommand(
    Guid PropertyId,
    Stream FileStream,
    string FileName,
    string ContentType,
    string? Description = null
) : IRequest<Guid>;
