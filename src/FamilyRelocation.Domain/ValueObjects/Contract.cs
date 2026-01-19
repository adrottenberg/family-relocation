namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Value object representing a property contract
/// </summary>
public sealed record Contract
{
    public Guid PropertyId { get; init; }
    public Money Price { get; init; }
    public DateTime ContractDate { get; init; }
    public DateTime? ExpectedClosingDate { get; init; }
    public DateTime? ActualClosingDate { get; init; }

    // Private parameterless constructor for EF Core
    private Contract()
    {
        PropertyId = Guid.Empty;
        Price = Money.Zero;
        ContractDate = DateTime.MinValue;
    }

    public Contract(
        Guid propertyId,
        Money price,
        DateTime contractDate,
        DateTime? expectedClosingDate = null)
    {
        if (propertyId == Guid.Empty)
            throw new ArgumentException("Property ID is required", nameof(propertyId));

        PropertyId = propertyId;
        Price = price ?? throw new ArgumentNullException(nameof(price));
        ContractDate = contractDate;
        ExpectedClosingDate = expectedClosingDate;
    }

    /// <summary>
    /// Create a new contract with the actual closing date set
    /// </summary>
    public Contract WithActualClosingDate(DateTime actualClosingDate)
    {
        return new Contract(PropertyId, Price, ContractDate, ExpectedClosingDate)
        {
            ActualClosingDate = actualClosingDate
        };
    }

    /// <summary>
    /// Check if the contract has closed
    /// </summary>
    public bool IsClosed => ActualClosingDate.HasValue;

    /// <summary>
    /// Days under contract (from contract date to closing or today)
    /// </summary>
    public int DaysUnderContract => ((ActualClosingDate ?? DateTime.UtcNow) - ContractDate).Days;

    public override string ToString() =>
        $"Contract on {ContractDate:d} for {Price}" +
        (IsClosed ? $" - Closed {ActualClosingDate:d}" : "");
}
