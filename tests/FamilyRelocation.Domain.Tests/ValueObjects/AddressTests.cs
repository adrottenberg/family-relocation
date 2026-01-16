using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Constructor_WithValidAddress_ShouldCreateAddress()
    {
        // Arrange & Act
        var address = new Address("123 Main St", "Union", "NJ", "07083");

        // Assert
        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Union");
        address.State.Should().Be("NJ");
        address.ZipCode.Should().Be("07083");
        address.Street2.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithStreet2_ShouldIncludeStreet2()
    {
        // Arrange & Act
        var address = new Address("123 Main St", "Union", "NJ", "07083", "Apt 4B");

        // Assert
        address.Street2.Should().Be("Apt 4B");
    }

    [Fact]
    public void Constructor_ShouldNormalizeStateToUppercase()
    {
        // Arrange & Act
        var address = new Address("123 Main St", "Union", "nj", "07083");

        // Assert
        address.State.Should().Be("NJ");
    }

    [Fact]
    public void Constructor_ShouldTrimWhitespace()
    {
        // Arrange & Act
        var address = new Address("  123 Main St  ", "  Union  ", "  NJ  ", "  07083  ");

        // Assert
        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Union");
        address.State.Should().Be("NJ");
        address.ZipCode.Should().Be("07083");
    }

    [Theory]
    [InlineData(null, "Union", "NJ", "07083")]
    [InlineData("", "Union", "NJ", "07083")]
    [InlineData("123 Main St", null, "NJ", "07083")]
    [InlineData("123 Main St", "", "NJ", "07083")]
    [InlineData("123 Main St", "Union", null, "07083")]
    [InlineData("123 Main St", "Union", "", "07083")]
    [InlineData("123 Main St", "Union", "NJ", null)]
    [InlineData("123 Main St", "Union", "NJ", "")]
    public void Constructor_WithMissingRequiredField_ShouldThrowArgumentException(
        string? street, string? city, string? state, string? zipCode)
    {
        // Arrange & Act
        var act = () => new Address(street!, city!, state!, zipCode!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FullAddress_WithoutStreet2_ShouldFormatCorrectly()
    {
        // Arrange
        var address = new Address("123 Main St", "Union", "NJ", "07083");

        // Act & Assert
        address.FullAddress.Should().Be("123 Main St, Union, NJ 07083");
    }

    [Fact]
    public void FullAddress_WithStreet2_ShouldFormatCorrectly()
    {
        // Arrange
        var address = new Address("123 Main St", "Union", "NJ", "07083", "Apt 4B");

        // Act & Assert
        address.FullAddress.Should().Be("123 Main St, Apt 4B, Union, NJ 07083");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange - Records compare exact values
        var address1 = new Address("123 Main St", "Union", "NJ", "07083");
        var address2 = new Address("123 Main St", "Union", "NJ", "07083");

        // Act & Assert
        address1.Should().Be(address2);
    }

    [Fact]
    public void Equals_WithDifferentCasing_ShouldReturnFalse()
    {
        // Arrange - Records compare exact values, different case = not equal
        var address1 = new Address("123 Main St", "Union", "NJ", "07083");
        var address2 = new Address("123 main st", "UNION", "NJ", "07083");

        // Act & Assert - Only State is normalized to uppercase
        address1.Should().NotBe(address2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Union", "NJ", "07083");
        var address2 = new Address("456 Oak Ave", "Union", "NJ", "07083");

        // Act & Assert
        address1.Should().NotBe(address2);
    }

    [Fact]
    public void ToString_ShouldReturnFullAddress()
    {
        // Arrange
        var address = new Address("123 Main St", "Union", "NJ", "07083");

        // Act & Assert
        address.ToString().Should().Be("123 Main St, Union, NJ 07083");
    }
}
