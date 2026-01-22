using MediatR;

namespace FamilyRelocation.Application.Properties.Commands.SetPrimaryPhoto;

public record SetPrimaryPhotoCommand(Guid PropertyId, Guid PhotoId) : IRequest;
