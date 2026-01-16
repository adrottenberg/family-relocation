using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Queries.GetApplicantById;

public class GetApplicantByIdQueryHandler : IRequestHandler<GetApplicantByIdQuery, ApplicantDto?>
{
    private readonly IApplicationDbContext _context;

    public GetApplicantByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicantDto?> Handle(GetApplicantByIdQuery request, CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        return applicant != null ? MapToDto(applicant) : null;
    }

    private static ApplicantDto MapToDto(Applicant applicant)
    {
        return new ApplicantDto
        {
            Id = applicant.Id,
            Husband = MapToHusbandDto(applicant.Husband),
            Wife = applicant.Wife != null ? MapToSpouseDto(applicant.Wife) : null,
            Address = applicant.Address != null ? MapToAddressDto(applicant.Address) : null,
            Children = applicant.Children.Select(MapToChildDto).ToList(),
            CurrentKehila = applicant.CurrentKehila,
            ShabbosShul = applicant.ShabbosShul,
            FamilyName = applicant.FamilyName,
            NumberOfChildren = applicant.NumberOfChildren,
            IsPendingBoardReview = applicant.IsPendingBoardReview,
            IsSelfSubmitted = applicant.IsSelfSubmitted,
            CreatedDate = applicant.CreatedDate
        };
    }

    private static HusbandInfoDto MapToHusbandDto(HusbandInfo husband)
    {
        return new HusbandInfoDto
        {
            FirstName = husband.FirstName,
            LastName = husband.LastName,
            FatherName = husband.FatherName,
            Email = husband.Email?.Value,
            PhoneNumbers = husband.PhoneNumbers.Select(MapToPhoneDto).ToList(),
            Occupation = husband.Occupation,
            EmployerName = husband.EmployerName
        };
    }

    private static SpouseInfoDto MapToSpouseDto(SpouseInfo wife)
    {
        return new SpouseInfoDto
        {
            FirstName = wife.FirstName,
            MaidenName = wife.MaidenName,
            FatherName = wife.FatherName,
            Email = wife.Email?.Value,
            PhoneNumbers = wife.PhoneNumbers.Select(MapToPhoneDto).ToList(),
            Occupation = wife.Occupation,
            EmployerName = wife.EmployerName,
            HighSchool = wife.HighSchool
        };
    }

    private static AddressDto MapToAddressDto(Address address)
    {
        return new AddressDto
        {
            Street = address.Street,
            Street2 = address.Street2,
            City = address.City,
            State = address.State,
            ZipCode = address.ZipCode
        };
    }

    private static ChildDto MapToChildDto(Child child)
    {
        return new ChildDto
        {
            Age = child.Age,
            Gender = child.Gender.ToString(),
            Name = child.Name,
            School = child.School
        };
    }

    private static PhoneNumberDto MapToPhoneDto(PhoneNumber phone)
    {
        return new PhoneNumberDto
        {
            Number = phone.Formatted,
            Type = phone.Type.ToString(),
            IsPrimary = phone.IsPrimary
        };
    }
}
