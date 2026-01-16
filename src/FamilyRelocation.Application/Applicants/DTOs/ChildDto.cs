namespace FamilyRelocation.Application.Applicants.DTOs;

public record ChildDto
{
    public required int Age { get; init; }
    public required string Gender { get; init; }
    public string? Name { get; init; }
    public string? School { get; init; }
}
