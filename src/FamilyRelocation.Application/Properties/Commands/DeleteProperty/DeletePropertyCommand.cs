using MediatR;

namespace FamilyRelocation.Application.Properties.Commands.DeleteProperty;

public record DeletePropertyCommand(Guid Id) : IRequest<Unit>;
