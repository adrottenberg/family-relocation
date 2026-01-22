using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Shuls.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Shuls.Queries.GetShulById;

public class GetShulByIdQueryHandler : IRequestHandler<GetShulByIdQuery, ShulDto?>
{
    private readonly IApplicationDbContext _context;

    public GetShulByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ShulDto?> Handle(GetShulByIdQuery request, CancellationToken cancellationToken)
    {
        var shul = await _context.Set<Shul>()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        return shul != null ? ShulMapper.ToDto(shul) : null;
    }
}
