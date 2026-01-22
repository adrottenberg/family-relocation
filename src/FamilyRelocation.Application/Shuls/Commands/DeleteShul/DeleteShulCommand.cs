using MediatR;

namespace FamilyRelocation.Application.Shuls.Commands.DeleteShul;

public record DeleteShulCommand(Guid Id) : IRequest<Unit>;
