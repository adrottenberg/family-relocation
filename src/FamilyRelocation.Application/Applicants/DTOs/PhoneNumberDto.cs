namespace FamilyRelocation.Application.Applicants.DTOs;

public record PhoneNumberDto
{
    public required string Number { get; init; }
    public string Type { get; init; } = "Mobile";
    public bool IsPrimary { get; init; }
}
