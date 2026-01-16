namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Reason a contract failed
/// </summary>
public enum ContractFailureReason
{
    InspectionIssues,
    FinancingFellThrough,
    AppraisalLow,
    TitleIssues,
    BuyerBackedOut,
    SellerBackedOut,
    MutualAgreement,
    Other
}
