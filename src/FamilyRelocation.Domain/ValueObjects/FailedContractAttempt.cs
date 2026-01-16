namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Value object representing a failed contract attempt during house hunting
/// Preserves history of properties that fell through
/// </summary>
public sealed record FailedContractAttempt
{
    public Guid PropertyId { get; }
    public Money ContractPrice { get; }
    public DateTime ContractDate { get; }
    public DateTime FailedDate { get; }
    public string? Reason { get; }

    // Private parameterless constructor for EF Core
    private FailedContractAttempt()
    {
        PropertyId = Guid.Empty;
        ContractPrice = Money.Zero;
        ContractDate = DateTime.MinValue;
        FailedDate = DateTime.MinValue;
    }

    public FailedContractAttempt(
        Guid propertyId,
        Money contractPrice,
        DateTime contractDate,
        DateTime failedDate,
        string? reason = null)
    {
        if (propertyId == Guid.Empty)
            throw new ArgumentException("Property ID is required", nameof(propertyId));

        PropertyId = propertyId;
        ContractPrice = contractPrice ?? throw new ArgumentNullException(nameof(contractPrice));
        ContractDate = contractDate;
        FailedDate = failedDate;
        Reason = reason?.Trim();
    }

    public int DaysUnderContract => (FailedDate - ContractDate).Days;

    public override string ToString() =>
        $"Contract on {ContractDate:d} for {ContractPrice} - Failed: {Reason ?? "No reason given"}";
}
