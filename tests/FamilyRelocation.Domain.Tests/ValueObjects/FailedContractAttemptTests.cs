using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.ValueObjects;

public class FailedContractAttemptTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateAttempt()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var contractPrice = new Money(450000);
        var contractDate = DateTime.UtcNow.AddDays(-30);
        var failedDate = DateTime.UtcNow;
        var reason = "Inspection issues";

        // Act
        var attempt = new FailedContractAttempt(propertyId, contractPrice, contractDate, failedDate, reason);

        // Assert
        attempt.PropertyId.Should().Be(propertyId);
        attempt.ContractPrice.Should().Be(contractPrice);
        attempt.ContractDate.Should().Be(contractDate);
        attempt.FailedDate.Should().Be(failedDate);
        attempt.Reason.Should().Be(reason);
    }

    [Fact]
    public void Constructor_WithEmptyPropertyId_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new FailedContractAttempt(
            Guid.Empty,
            new Money(450000),
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            "Reason");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Property ID*");
    }

    [Fact]
    public void Constructor_WithNullContractPrice_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new FailedContractAttempt(
            Guid.NewGuid(),
            null!,
            DateTime.UtcNow.AddDays(-30),
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
            Guid.NewGuid(),
            new Money(450000),
            DateTime.UtcNow.AddDays(-30),
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
        var attempt = new FailedContractAttempt(
            Guid.NewGuid(),
            new Money(450000),
            contractDate,
            failedDate,
            "Reason");

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

        var attempt1 = new FailedContractAttempt(propertyId, contractPrice, contractDate, failedDate, reason);
        var attempt2 = new FailedContractAttempt(propertyId, contractPrice, contractDate, failedDate, reason);

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

        var attempt1 = new FailedContractAttempt(Guid.NewGuid(), contractPrice, contractDate, failedDate, "Reason 1");
        var attempt2 = new FailedContractAttempt(Guid.NewGuid(), contractPrice, contractDate, failedDate, "Reason 2");

        // Act & Assert
        attempt1.Should().NotBe(attempt2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var attempt = new FailedContractAttempt(
            Guid.NewGuid(),
            new Money(450000),
            new DateTime(2026, 1, 1),
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
        var attempt = new FailedContractAttempt(
            Guid.NewGuid(),
            new Money(450000),
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 15),
            null);

        // Act
        var result = attempt.ToString();

        // Assert
        result.Should().Contain("No reason given");
    }
}
