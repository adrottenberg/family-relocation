namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// DTO for Applicant response
/// </summary>
public record ApplicantDto
{
    public required Guid Id { get; init; }
    public required HusbandInfoDto Husband { get; init; }
    public SpouseInfoDto? Wife { get; init; }
    public AddressDto? Address { get; init; }
    public List<ChildDto>? Children { get; init; }
    public string? CurrentKehila { get; init; }
    public string? ShabbosShul { get; init; }
    public required string FamilyName { get; init; }
    public required int NumberOfChildren { get; init; }
    public required bool IsPendingBoardReview { get; init; }
    public required bool IsSelfSubmitted { get; init; }
    public required DateTime CreatedDate { get; init; }
}
