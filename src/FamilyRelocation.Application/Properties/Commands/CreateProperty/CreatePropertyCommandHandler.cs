using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Properties.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;

namespace FamilyRelocation.Application.Properties.Commands.CreateProperty;

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, PropertyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActivityLogger _activityLogger;

    public CreatePropertyCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IActivityLogger activityLogger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<PropertyDto> Handle(CreatePropertyCommand request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var address = new Address(
            request.Street,
            request.City,
            request.State,
            request.ZipCode,
            request.Street2);

        var price = new Money(request.Price);

        var property = Property.Create(
            address: address,
            price: price,
            bedrooms: request.Bedrooms,
            bathrooms: request.Bathrooms,
            createdBy: userId,
            squareFeet: request.SquareFeet,
            lotSize: request.LotSize,
            yearBuilt: request.YearBuilt,
            annualTaxes: request.AnnualTaxes,
            features: request.Features,
            mlsNumber: request.MlsNumber,
            notes: request.Notes);

        _context.Add(property);
        await _unitOfWork.SaveChangesAsync(ct);

        await _activityLogger.LogAsync(
            "Property",
            property.Id,
            "Created",
            $"Property created at {address.Street}, {address.City}",
            ct);

        return property.ToDto();
    }
}
