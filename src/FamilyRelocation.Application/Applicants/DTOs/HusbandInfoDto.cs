namespace FamilyRelocation.Application.Applicants.DTOs;

public record HusbandInfoDto
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? FatherName { get; init; }
    public string? Email { get; init; }
    public List<PhoneNumberDto>? PhoneNumbers { get; init; }
    public string? Occupation { get; init; }
    public string? EmployerName { get; init; }
}
