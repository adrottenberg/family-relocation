using MediatR;

namespace FamilyRelocation.Application.Properties.Commands.DeletePropertyPhoto;

public record DeletePropertyPhotoCommand(Guid PropertyId, Guid PhotoId) : IRequest;
