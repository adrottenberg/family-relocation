using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Commands.UpdateApplicant;

public class UpdateApplicantCommandHandler : IRequestHandler<UpdateApplicantCommand, ApplicantDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateApplicantCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApplicantDto> Handle(UpdateApplicantCommand request, CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (applicant == null)
        {
            throw new NotFoundException("Applicant", request.Id);
        }

        // Check for duplicate emails (excluding current applicant)
        await ValidateEmailUniqueness(request, applicant.Id, cancellationToken);

        var userId = _currentUserService.UserId ?? Guid.Empty;

        // Update husband info
        var husband = MapToHusbandInfo(request.Husband);
        applicant.UpdateHusband(husband, userId);

        // Update wife info
        var wife = request.Wife != null ? MapToSpouseInfo(request.Wife) : null;
        applicant.UpdateWife(wife, userId);

        // Update address
        var address = request.Address != null ? MapToAddress(request.Address) : null;
        applicant.UpdateAddress(address, userId);

        // Update children
        var children = request.Children?.Select(MapToChild).ToList() ?? new List<Child>();
        applicant.UpdateChildren(children, userId);

        // Update community info
        applicant.UpdateCommunityInfo(request.CurrentKehila, request.ShabbosShul, userId);

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(applicant);
    }

    private async Task ValidateEmailUniqueness(UpdateApplicantCommand request, Guid currentApplicantId, CancellationToken cancellationToken)
    {
        // Check husband email
        if (!string.IsNullOrEmpty(request.Husband.Email))
        {
            var normalizedEmail = request.Husband.Email.ToLowerInvariant();
            var emailExists = await _context.Set<Applicant>()
                .AnyAsync(a => a.Id != currentApplicantId &&
                    (a.Husband.Email == normalizedEmail ||
                     (a.Wife != null && a.Wife.Email == normalizedEmail)),
                    cancellationToken);

            if (emailExists)
            {
                throw new DuplicateEmailException(request.Husband.Email);
            }
        }

        // Check wife email
        if (!string.IsNullOrEmpty(request.Wife?.Email))
        {
            var normalizedEmail = request.Wife.Email.ToLowerInvariant();
            var emailExists = await _context.Set<Applicant>()
                .AnyAsync(a => a.Id != currentApplicantId &&
                    (a.Husband.Email == normalizedEmail ||
                     (a.Wife != null && a.Wife.Email == normalizedEmail)),
                    cancellationToken);

            if (emailExists)
            {
                throw new DuplicateEmailException(request.Wife.Email);
            }
        }
    }

    private static HusbandInfo MapToHusbandInfo(HusbandInfoDto dto)
    {
        var phoneNumbers = NormalizePhoneNumbers(dto.PhoneNumbers);

        return new HusbandInfo(
            firstName: dto.FirstName,
            lastName: dto.LastName,
            fatherName: dto.FatherName,
            email: dto.Email,
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
            email: dto.Email,
            phoneNumbers: phoneNumbers,
            occupation: dto.Occupation,
            employerName: dto.EmployerName,
            highSchool: dto.HighSchool);
    }

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

    private static PhoneType ParsePhoneType(string? type)
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
            CreatedDate = applicant.CreatedDate,
            BoardReview = applicant.BoardReview != null ? MapToBoardReviewDto(applicant.BoardReview) : null
        };
    }

    private static HusbandInfoDto MapToHusbandDto(HusbandInfo husband)
    {
        return new HusbandInfoDto
        {
            FirstName = husband.FirstName,
            LastName = husband.LastName,
            FatherName = husband.FatherName,
            Email = husband.Email,
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
            Email = wife.Email,
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

    private static BoardReviewDto MapToBoardReviewDto(BoardReview boardReview)
    {
        return new BoardReviewDto
        {
            Decision = boardReview.Decision.ToString(),
            ReviewDate = boardReview.ReviewDate,
            Notes = boardReview.Notes
        };
    }
}
