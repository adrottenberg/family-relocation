namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Value object representing a failed contract attempt during house hunting
/// Preserves history of properties that fell through
/// </summary>
public sealed record FailedContractAttempt
{
    public Contract Contract { get; init; }
    public DateTime FailedDate { get; init; }
    public string? Reason { get; init; }

    // Private parameterless constructor for EF Core
    private FailedContractAttempt()
    {
        Contract = null!;
        FailedDate = DateTime.MinValue;
    }

    public FailedContractAttempt(
        Contract contract,
        DateTime failedDate,
        string? reason = null)
    {
        Contract = contract ?? throw new ArgumentNullException(nameof(contract));
        FailedDate = failedDate;
        Reason = reason?.Trim();
    }

    // Convenience accessors
    public Guid PropertyId => Contract.PropertyId;
    public Money ContractPrice => Contract.Price;
    public DateTime ContractDate => Contract.ContractDate;

    public int DaysUnderContract => (FailedDate - Contract.ContractDate).Days;

    public override string ToString() =>
        $"Contract on {Contract.ContractDate:d} for {Contract.Price} - Failed: {Reason ?? "No reason given"}";
}
