using FamilyRelocation.Application.Shuls.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Shuls.Queries.GetShulById;

public record GetShulByIdQuery(Guid Id) : IRequest<ShulDto?>;
