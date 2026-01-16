namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Property listing status
/// NOTE: UnderContractThroughUs removed per Correction #10
/// Use Application.ContractPropertyId to track "our" contracts
/// </summary>
public enum ListingStatus
{
    Active,
    UnderContract,
    Sold,
    OffMarket
}
