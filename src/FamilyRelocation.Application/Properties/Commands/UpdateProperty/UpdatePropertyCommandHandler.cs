using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Properties.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Properties.Commands.UpdateProperty;

public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, PropertyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActivityLogger _activityLogger;

    public UpdatePropertyCommandHandler(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IActivityLogger activityLogger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<PropertyDto> Handle(UpdatePropertyCommand request, CancellationToken ct)
    {
        var property = await _context.Set<Property>()
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, ct);

        if (property == null)
            throw new NotFoundException(nameof(Property), request.Id);

        var address = new Address(
            request.Street,
            request.City,
            request.State,
            request.ZipCode,
            request.Street2);

        var price = new Money(request.Price);

        property.Update(
            address: address,
            price: price,
            bedrooms: request.Bedrooms,
            bathrooms: request.Bathrooms,
            modifiedBy: Guid.Empty, // TODO: Get from current user context
            squareFeet: request.SquareFeet,
            lotSize: request.LotSize,
            yearBuilt: request.YearBuilt,
            annualTaxes: request.AnnualTaxes,
            features: request.Features,
            mlsNumber: request.MlsNumber,
            notes: request.Notes);

        await _unitOfWork.SaveChangesAsync(ct);

        await _activityLogger.LogAsync(
            "Property",
            property.Id,
            "Updated",
            $"Property updated at {address.Street}, {address.City}",
            ct);

        return property.ToDto();
    }
}
