namespace FamilyRelocation.Application.Applicants.DTOs;

public record SpouseInfoDto
{
    public required string FirstName { get; init; }
    public string? MaidenName { get; init; }
    public string? FatherName { get; init; }
    public string? Email { get; init; }
    public List<PhoneNumberDto>? PhoneNumbers { get; init; }
    public string? Occupation { get; init; }
    public string? EmployerName { get; init; }
    public string? HighSchool { get; init; }
}
