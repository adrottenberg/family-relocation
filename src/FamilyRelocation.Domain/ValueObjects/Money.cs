namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Money value object representing USD currency
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    // Private parameterless constructor for EF Core
    private Money()
    {
        Amount = 0;
        Currency = "USD";
    }

    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        Amount = Math.Round(amount, 2);
        Currency = currency;
    }

    public static Money Zero => new(0);

    public static Money FromDollars(decimal dollars) => new(dollars);

    public Money Add(Money other)
    {
        if (other.Currency != Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (other.Currency != Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");

        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException($"Cannot subtract {other.Amount:C} from {Amount:C} - would result in negative amount");

        return new Money(result, Currency);
    }

    /// <summary>
    /// Check if this amount is greater than or equal to another
    /// </summary>
    public bool IsGreaterThanOrEqual(Money other)
    {
        if (other.Currency != Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");

        return Amount >= other.Amount;
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    public override string ToString() => $"{Amount:C}";

    public string ToFormattedString() => $"{Amount:N0}";

    public static implicit operator decimal(Money money) => money.Amount;
}
