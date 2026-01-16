using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.ValueObjects;

public class FailedContractAttemptTests
{
    private Contract CreateTestContract(
        Guid? propertyId = null,
        Money? price = null,
        DateTime? contractDate = null)
    {
        return new Contract(
            propertyId ?? Guid.NewGuid(),
            price ?? new Money(450000),
            contractDate ?? DateTime.UtcNow.AddDays(-30));
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreateAttempt()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var contractPrice = new Money(450000);
        var contractDate = DateTime.UtcNow.AddDays(-30);
        var failedDate = DateTime.UtcNow;
        var reason = "Inspection issues";
        var contract = new Contract(propertyId, contractPrice, contractDate);

        // Act
        var attempt = new FailedContractAttempt(contract, failedDate, reason);

        // Assert
        attempt.Contract.Should().Be(contract);
        attempt.PropertyId.Should().Be(propertyId);
        attempt.ContractPrice.Should().Be(contractPrice);
        attempt.ContractDate.Should().Be(contractDate);
        attempt.FailedDate.Should().Be(failedDate);
        attempt.Reason.Should().Be(reason);
    }

    [Fact]
    public void Constructor_WithNullContract_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new FailedContractAttempt(
            null!,
            DateTime.UtcNow,
            "Reason");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullReason_ShouldAllowNull()
    {
        // Arrange & Act
        var attempt = new FailedContractAttempt(
            CreateTestContract(),
            DateTime.UtcNow,
            null);

        // Assert
        attempt.Reason.Should().BeNull();
    }

    [Fact]
    public void DaysUnderContract_ShouldCalculateCorrectly()
    {
        // Arrange
        var contractDate = DateTime.UtcNow.AddDays(-45);
        var failedDate = DateTime.UtcNow;
        var contract = new Contract(Guid.NewGuid(), new Money(450000), contractDate);
        var attempt = new FailedContractAttempt(contract, failedDate, "Reason");

        // Act & Assert
        attempt.DaysUnderContract.Should().Be(45);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var contractPrice = new Money(450000);
        var contractDate = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var failedDate = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var reason = "Inspection issues";

        var contract = new Contract(propertyId, contractPrice, contractDate);
        var attempt1 = new FailedContractAttempt(contract, failedDate, reason);
        var attempt2 = new FailedContractAttempt(contract, failedDate, reason);

        // Act & Assert
        attempt1.Should().Be(attempt2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var contractPrice = new Money(450000);
        var contractDate = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var failedDate = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        var contract1 = new Contract(Guid.NewGuid(), contractPrice, contractDate);
        var contract2 = new Contract(Guid.NewGuid(), contractPrice, contractDate);
        var attempt1 = new FailedContractAttempt(contract1, failedDate, "Reason 1");
        var attempt2 = new FailedContractAttempt(contract2, failedDate, "Reason 2");

        // Act & Assert
        attempt1.Should().NotBe(attempt2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var contract = new Contract(
            Guid.NewGuid(),
            new Money(450000),
            new DateTime(2026, 1, 1));
        var attempt = new FailedContractAttempt(
            contract,
            new DateTime(2026, 1, 15),
            "Financing fell through");

        // Act
        var result = attempt.ToString();

        // Assert
        result.Should().Contain("Financing fell through");
        result.Should().Contain("$450,000");
    }

    [Fact]
    public void ToString_WithNoReason_ShouldShowNoReasonGiven()
    {
        // Arrange
        var contract = new Contract(
            Guid.NewGuid(),
            new Money(450000),
            new DateTime(2026, 1, 1));
        var attempt = new FailedContractAttempt(
            contract,
            new DateTime(2026, 1, 15),
            null);

        // Act
        var result = attempt.ToString();

        // Assert
        result.Should().Contain("No reason given");
    }
}
