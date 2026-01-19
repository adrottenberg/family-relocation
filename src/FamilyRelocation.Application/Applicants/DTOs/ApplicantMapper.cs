using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.ValueObjects;

namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// Extension methods for mapping between Applicant domain entities/value objects and DTOs
/// </summary>
public static class ApplicantMapper
{
    #region Domain to DTO (for responses)

    public static ApplicantDto ToDto(this Applicant applicant)
    {
        return new ApplicantDto
        {
            Id = applicant.Id,
            Husband = applicant.Husband.ToDto(),
            Wife = applicant.Wife?.ToDto(),
            Address = applicant.Address?.ToDto(),
            Children = applicant.Children.Select(c => c.ToDto()).ToList(),
            CurrentKehila = applicant.CurrentKehila,
            ShabbosShul = applicant.ShabbosShul,
            FamilyName = applicant.FamilyName,
            NumberOfChildren = applicant.NumberOfChildren,
            IsPendingBoardReview = applicant.IsPendingBoardReview,
            IsSelfSubmitted = applicant.IsSelfSubmitted,
            CreatedDate = applicant.CreatedDate,
            BoardReview = applicant.BoardReview?.ToDto()
        };
    }

    public static HusbandInfoDto ToDto(this HusbandInfo husband)
    {
        return new HusbandInfoDto
        {
            FirstName = husband.FirstName,
            LastName = husband.LastName,
            FatherName = husband.FatherName,
            Email = husband.Email,
            PhoneNumbers = husband.PhoneNumbers.Select(p => p.ToDto()).ToList(),
            Occupation = husband.Occupation,
            EmployerName = husband.EmployerName
        };
    }

    public static SpouseInfoDto ToDto(this SpouseInfo wife)
    {
        return new SpouseInfoDto
        {
            FirstName = wife.FirstName,
            MaidenName = wife.MaidenName,
            FatherName = wife.FatherName,
            Email = wife.Email,
            PhoneNumbers = wife.PhoneNumbers.Select(p => p.ToDto()).ToList(),
            Occupation = wife.Occupation,
            EmployerName = wife.EmployerName,
            HighSchool = wife.HighSchool
        };
    }

    public static AddressDto ToDto(this Address address)
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

    public static ChildDto ToDto(this Child child)
    {
        return new ChildDto
        {
            Age = child.Age,
            Gender = child.Gender.ToString(),
            Name = child.Name,
            School = child.School
        };
    }

    public static PhoneNumberDto ToDto(this PhoneNumber phone)
    {
        return new PhoneNumberDto
        {
            Number = phone.Formatted,
            Type = phone.Type.ToString(),
            IsPrimary = phone.IsPrimary
        };
    }

    public static BoardReviewDto ToDto(this BoardReview boardReview)
    {
        return new BoardReviewDto
        {
            Decision = boardReview.Decision.ToString(),
            ReviewDate = boardReview.ReviewDate,
            Notes = boardReview.Notes
        };
    }

    /// <summary>
    /// Map to lightweight list DTO for list views.
    /// </summary>
    public static ApplicantListDto ToListDto(this Applicant applicant)
    {
        return new ApplicantListDto
        {
            Id = applicant.Id,
            HusbandFullName = applicant.Husband.FullName,
            WifeMaidenName = applicant.Wife?.MaidenName,
            HusbandEmail = applicant.Husband.Email,
            HusbandPhone = applicant.Husband.PhoneNumbers.FirstOrDefault(p => p.IsPrimary)?.Formatted
                           ?? applicant.Husband.PhoneNumbers.FirstOrDefault()?.Formatted,
            BoardDecision = applicant.BoardReview?.Decision.ToString(),
            CreatedDate = applicant.CreatedDate
        };
    }

    #endregion

    #region DTO to Domain (for create/update)

    public static HusbandInfo ToDomain(this HusbandInfoDto dto)
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

    public static SpouseInfo ToDomain(this SpouseInfoDto dto)
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

    public static Address ToDomain(this AddressDto dto)
    {
        return new Address(
            street: dto.Street,
            city: dto.City,
            state: dto.State,
            zipCode: dto.ZipCode,
            street2: dto.Street2);
    }

    public static Child ToDomain(this ChildDto dto)
    {
        var gender = Enum.Parse<Gender>(dto.Gender, ignoreCase: true);
        return new Child(dto.Age, gender, dto.Name, dto.School);
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

    private static PhoneType ParsePhoneType(string? type)
    {
        return Enum.TryParse<PhoneType>(type, ignoreCase: true, out var phoneType)
            ? phoneType
            : PhoneType.Mobile;
    }

    public static HousingPreferences ToDomain(this HousingPreferencesDto dto)
    {
        Money? budget = dto.BudgetAmount.HasValue
            ? new Money(dto.BudgetAmount.Value)
            : null;

        MoveTimeline? moveTimeline = null;
        if (!string.IsNullOrEmpty(dto.MoveTimeline) &&
            Enum.TryParse<MoveTimeline>(dto.MoveTimeline, ignoreCase: true, out var timeline))
        {
            moveTimeline = timeline;
        }

        return new HousingPreferences(
            budget: budget,
            minBedrooms: dto.MinBedrooms,
            minBathrooms: dto.MinBathrooms,
            requiredFeatures: dto.RequiredFeatures,
            shulProximity: dto.ShulProximity?.ToDomain(),
            moveTimeline: moveTimeline);
    }

    public static ShulProximityPreference ToDomain(this ShulProximityPreferenceDto dto)
    {
        return new ShulProximityPreference(
            preferredShulIds: dto.PreferredShulIds,
            maxWalkingDistanceMiles: dto.MaxWalkingDistanceMiles,
            maxWalkingTimeMinutes: dto.MaxWalkingTimeMinutes,
            anyShulAcceptable: dto.AnyShulAcceptable);
    }

    #endregion
}
