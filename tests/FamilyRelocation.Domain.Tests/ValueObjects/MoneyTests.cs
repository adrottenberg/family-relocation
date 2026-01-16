using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidAmount_ShouldCreateMoney()
    {
        // Arrange & Act
        var money = new Money(500000m);

        // Assert
        money.Amount.Should().Be(500000m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_ShouldRoundToTwoDecimalPlaces()
    {
        // Arrange & Act
        var money = new Money(500000.999m);

        // Assert
        money.Amount.Should().Be(500001.00m);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new Money(-100m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void Zero_ShouldReturnMoneyWithZeroAmount()
    {
        // Arrange & Act
        var money = Money.Zero;

        // Assert
        money.Amount.Should().Be(0);
    }

    [Fact]
    public void FromDollars_ShouldCreateMoney()
    {
        // Arrange & Act
        var money = Money.FromDollars(750000m);

        // Assert
        money.Amount.Should().Be(750000m);
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = new Money(100m);
        var money2 = new Money(50m);

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "EUR");

        // Act
        var act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnDifference()
    {
        // Arrange
        var money1 = new Money(100m);
        var money2 = new Money(30m);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Multiply_ShouldReturnProduct()
    {
        // Arrange
        var money = new Money(100m);

        // Act
        var result = money.Multiply(1.5m);

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Equals_WithSameAmount_ShouldReturnTrue()
    {
        // Arrange
        var money1 = new Money(500000m);
        var money2 = new Money(500000m);

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentAmount_ShouldReturnFalse()
    {
        // Arrange
        var money1 = new Money(500000m);
        var money2 = new Money(600000m);

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnAmount()
    {
        // Arrange
        var money = new Money(500000m);

        // Act
        decimal amount = money;

        // Assert
        amount.Should().Be(500000m);
    }

    [Fact]
    public void ToFormattedString_ShouldFormatWithCommas()
    {
        // Arrange
        var money = new Money(500000m);

        // Act & Assert
        money.ToFormattedString().Should().Be("500,000");
    }
}
