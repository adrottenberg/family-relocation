using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.CreateApplicant;

public class CreateApplicantCommandHandler : IRequestHandler<CreateApplicantCommand, ApplicantDto>
{
    private readonly IApplicantRepository _applicantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateApplicantCommandHandler(
        IApplicantRepository applicantRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _applicantRepository = applicantRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApplicantDto> Handle(CreateApplicantCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate emails (husband and wife)
        if (!string.IsNullOrEmpty(request.Husband.Email))
        {
            var exists = await _applicantRepository.ExistsByEmailAsync(request.Husband.Email, cancellationToken);
            if (exists)
            {
                throw new DuplicateEmailException(request.Husband.Email);
            }
        }

        if (!string.IsNullOrEmpty(request.Wife?.Email))
        {
            var exists = await _applicantRepository.ExistsByEmailAsync(request.Wife.Email, cancellationToken);
            if (exists)
            {
                throw new DuplicateEmailException(request.Wife.Email);
            }
        }

        // Map DTOs to domain objects
        var husband = MapToHusbandInfo(request.Husband);
        var wife = request.Wife != null ? MapToSpouseInfo(request.Wife) : null;
        var address = request.Address != null ? MapToAddress(request.Address) : null;
        var children = request.Children?.Select(MapToChild).ToList();

        var applicant = Applicant.Create(
            husband: husband,
            wife: wife,
            address: address,
            children: children,
            currentKehila: request.CurrentKehila,
            shabbosShul: request.ShabbosShul,
            createdBy: _currentUserService.UserId);

        await _applicantRepository.AddAsync(applicant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(applicant);
    }

    private static HusbandInfo MapToHusbandInfo(HusbandInfoDto dto)
    {
        var phoneNumbers = NormalizePhoneNumbers(dto.PhoneNumbers);

        return new HusbandInfo(
            firstName: dto.FirstName,
            lastName: dto.LastName,
            fatherName: dto.FatherName,
            email: Email.FromString(dto.Email),
            phoneNumbers: phoneNumbers,
            occupation: dto.Occupation,
            employerName: dto.EmployerName);
    }

    private static SpouseInfo MapToSpouseInfo(SpouseInfoDto dto)
    {
        var phoneNumbers = NormalizePhoneNumbers(dto.PhoneNumbers);

        return new SpouseInfo(
            firstName: dto.FirstName,
            maidenName: dto.MaidenName,
            fatherName: dto.FatherName,
            email: Email.FromString(dto.Email),
            phoneNumbers: phoneNumbers,
            occupation: dto.Occupation,
            employerName: dto.EmployerName,
            highSchool: dto.HighSchool);
    }

    /// <summary>
    /// Normalizes phone numbers to ensure only one is marked as primary.
    /// If multiple are marked primary, only the first one remains primary.
    /// </summary>
    private static List<PhoneNumber>? NormalizePhoneNumbers(List<PhoneNumberDto>? phoneDtos)
    {
        if (phoneDtos == null || phoneDtos.Count == 0)
            return null;

        var hasPrimary = false;
        var phoneNumbers = new List<PhoneNumber>();

        foreach (var dto in phoneDtos)
        {
            var isPrimary = dto.IsPrimary && !hasPrimary;
            if (isPrimary)
                hasPrimary = true;

            phoneNumbers.Add(new PhoneNumber(dto.Number, ParsePhoneType(dto.Type), isPrimary));
        }

        return phoneNumbers;
    }

    private static Address MapToAddress(AddressDto dto)
    {
        return new Address(
            street: dto.Street,
            city: dto.City,
            state: dto.State,
            zipCode: dto.ZipCode,
            street2: dto.Street2);
    }

    private static Child MapToChild(ChildDto dto)
    {
        var gender = Enum.Parse<Gender>(dto.Gender, ignoreCase: true);
        return new Child(dto.Age, gender, dto.Name, dto.School);
    }

    private static PhoneType ParsePhoneType(string type)
    {
        return Enum.TryParse<PhoneType>(type, ignoreCase: true, out var phoneType)
            ? phoneType
            : PhoneType.Mobile;
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
