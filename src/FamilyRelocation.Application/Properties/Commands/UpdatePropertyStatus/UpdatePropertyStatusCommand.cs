using FamilyRelocation.Application.Properties.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Properties.Commands.UpdatePropertyStatus;

public record UpdatePropertyStatusCommand(Guid Id, string Status) : IRequest<PropertyDto>;
