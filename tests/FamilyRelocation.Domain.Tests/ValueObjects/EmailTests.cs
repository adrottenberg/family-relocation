using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Constructor_WithValidEmail_ShouldCreateEmail()
    {
        // Arrange & Act
        var email = new Email("test@example.com");

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Constructor_ShouldNormalizeEmailToLowercase()
    {
        // Arrange & Act
        var email = new Email("Test@EXAMPLE.COM");

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Constructor_ShouldTrimWhitespace()
    {
        // Arrange & Act
        var email = new Email("  test@example.com  ");

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Arrange & Act
        var act = () => new Email(invalidEmail!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*required*");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    public void Constructor_WithInvalidEmailFormat_ShouldThrowArgumentException(string invalidEmail)
    {
        // Arrange & Act
        var act = () => new Email(invalidEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email*");
    }

    [Fact]
    public void FromString_WithValidEmail_ShouldReturnEmail()
    {
        // Arrange & Act
        var email = Email.FromString("test@example.com");

        // Assert
        email.Should().NotBeNull();
        email!.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromString_WithNullOrEmpty_ShouldReturnNull(string? value)
    {
        // Arrange & Act
        var email = Email.FromString(value);

        // Assert
        email.Should().BeNull();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("TEST@EXAMPLE.COM");

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act & Assert
        email.ToString().Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnEmailValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        string value = email;

        // Assert
        value.Should().Be("test@example.com");
    }
}
