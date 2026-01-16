using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("2015551234")]
    [InlineData("201-555-1234")]
    [InlineData("(201) 555-1234")]
    [InlineData("201.555.1234")]
    [InlineData("1-201-555-1234")]
    [InlineData("+1 201 555 1234")]
    public void Constructor_WithValidUSPhoneNumber_ShouldCreatePhoneNumber(string input)
    {
        // Arrange & Act
        var phone = new PhoneNumber(input);

        // Assert
        phone.Number.Should().Be("12015551234");
    }

    [Fact]
    public void Constructor_WithPhoneType_ShouldSetType()
    {
        // Arrange & Act
        var phone = new PhoneNumber("2015551234", PhoneType.Home);

        // Assert
        phone.Type.Should().Be(PhoneType.Home);
    }

    [Fact]
    public void Constructor_WithIsPrimary_ShouldSetPrimary()
    {
        // Arrange & Act
        var phone = new PhoneNumber("2015551234", PhoneType.Mobile, isPrimary: true);

        // Assert
        phone.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void Constructor_DefaultType_ShouldBeMobile()
    {
        // Arrange & Act
        var phone = new PhoneNumber("2015551234");

        // Assert
        phone.Type.Should().Be(PhoneType.Mobile);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmpty_ShouldThrowArgumentException(string? input)
    {
        // Arrange & Act
        var act = () => new PhoneNumber(input!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*required*");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901234")]
    [InlineData("abc")]
    public void Constructor_WithInvalidFormat_ShouldThrowArgumentException(string input)
    {
        // Arrange & Act
        var act = () => new PhoneNumber(input);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid*");
    }

    [Fact]
    public void Formatted_ShouldReturnFormattedNumber()
    {
        // Arrange
        var phone = new PhoneNumber("2015551234");

        // Act & Assert
        phone.Formatted.Should().Be("(201) 555-1234");
    }

    [Fact]
    public void E164_ShouldReturnE164Format()
    {
        // Arrange
        var phone = new PhoneNumber("2015551234");

        // Act & Assert
        phone.E164.Should().Be("+12015551234");
    }

    [Fact]
    public void Equals_WithSameNumber_ShouldReturnTrue()
    {
        // Arrange
        var phone1 = new PhoneNumber("201-555-1234", PhoneType.Mobile);
        var phone2 = new PhoneNumber("(201) 555-1234", PhoneType.Mobile);

        // Act & Assert
        phone1.Should().Be(phone2);
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var phone1 = new PhoneNumber("201-555-1234", PhoneType.Mobile);
        var phone2 = new PhoneNumber("201-555-1234", PhoneType.Home);

        // Act & Assert
        phone1.Should().NotBe(phone2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedNumber()
    {
        // Arrange
        var phone = new PhoneNumber("2015551234");

        // Act & Assert
        phone.ToString().Should().Be("(201) 555-1234");
    }
}
