using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.RecordAgreement;

/// <summary>
/// Command to record that an applicant has signed an agreement.
/// </summary>
public record RecordAgreementCommand(
    Guid ApplicantId,
    RecordAgreementRequest Request
) : IRequest<RecordAgreementResponse>;

/// <summary>
/// Request body for recording agreement signing.
/// </summary>
public class RecordAgreementRequest
{
    /// <summary>
    /// Type of agreement: BrokerAgreement or CommunityTakanos.
    /// </summary>
    public required string AgreementType { get; init; }

    /// <summary>
    /// URL to the uploaded signed document (S3 or other storage).
    /// </summary>
    public required string DocumentUrl { get; init; }
}

/// <summary>
/// Response after recording agreement signing.
/// </summary>
public class RecordAgreementResponse
{
    /// <summary>
    /// Whether the broker agreement has been signed.
    /// </summary>
    public required bool BrokerAgreementSigned { get; init; }

    /// <summary>
    /// Whether the community takanos has been signed.
    /// </summary>
    public required bool CommunityTakanosSigned { get; init; }

    /// <summary>
    /// Whether all required agreements are signed (ready to start house hunting).
    /// </summary>
    public required bool AllAgreementsSigned { get; init; }
}

/// <summary>
/// Agreement types that must be signed before house hunting.
/// </summary>
public static class AgreementTypes
{
    /// <summary>
    /// Broker agreement - agreement to work with the community's broker.
    /// </summary>
    public const string BrokerAgreement = "BrokerAgreement";

    /// <summary>
    /// Community takanos - community guidelines and rules agreement.
    /// </summary>
    public const string CommunityTakanos = "CommunityTakanos";
}
