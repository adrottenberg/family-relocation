using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.ValueObjects;

public class ChildTests
{
    [Fact]
    public void Constructor_WithRequiredFields_ShouldCreateChild()
    {
        // Arrange & Act
        var child = new Child(10, Gender.Male);

        // Assert
        child.Age.Should().Be(10);
        child.Gender.Should().Be(Gender.Male);
        child.Name.Should().BeNull();
        child.School.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithOptionalName_ShouldSetIt()
    {
        // Arrange & Act
        var child = new Child(10, Gender.Male, "Moshe");

        // Assert
        child.Age.Should().Be(10);
        child.Gender.Should().Be(Gender.Male);
        child.Name.Should().Be("Moshe");
    }

    [Fact]
    public void Constructor_WithAllFields_ShouldSetThem()
    {
        // Arrange & Act
        var child = new Child(12, Gender.Female, "Sarah", "Bais Yaakov");

        // Assert
        child.Age.Should().Be(12);
        child.Gender.Should().Be(Gender.Female);
        child.Name.Should().Be("Sarah");
        child.School.Should().Be("Bais Yaakov");
    }

    [Fact]
    public void Constructor_ShouldTrimWhitespace()
    {
        // Arrange & Act
        var child = new Child(10, Gender.Male, "  Moshe  ", "  School  ");

        // Assert
        child.Name.Should().Be("Moshe");
        child.School.Should().Be("School");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(51)]
    public void Constructor_WithInvalidAge_ShouldThrowArgumentException(int age)
    {
        // Arrange & Act
        var act = () => new Child(age, Gender.Male);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Age*");
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    [InlineData(18, true)]
    [InlineData(19, false)]
    public void IsSchoolAge_ShouldReturnCorrectValue(int age, bool expectedIsSchoolAge)
    {
        // Arrange
        var child = new Child(age, Gender.Male);

        // Act & Assert
        child.IsSchoolAge.Should().Be(expectedIsSchoolAge);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange - Records compare exact values
        var child1 = new Child(10, Gender.Male, "Moshe");
        var child2 = new Child(10, Gender.Male, "Moshe");

        // Act & Assert
        child1.Should().Be(child2);
    }

    [Fact]
    public void Equals_WithDifferentCasing_ShouldReturnFalse()
    {
        // Arrange - Records compare exact values
        var child1 = new Child(10, Gender.Male, "Moshe");
        var child2 = new Child(10, Gender.Male, "MOSHE");

        // Act & Assert
        child1.Should().NotBe(child2);
    }

    [Fact]
    public void Equals_WithDifferentAge_ShouldReturnFalse()
    {
        // Arrange
        var child1 = new Child(10, Gender.Male);
        var child2 = new Child(12, Gender.Male);

        // Act & Assert
        child1.Should().NotBe(child2);
    }

    [Fact]
    public void Equals_WithDifferentGender_ShouldReturnFalse()
    {
        // Arrange
        var child1 = new Child(10, Gender.Male);
        var child2 = new Child(10, Gender.Female);

        // Act & Assert
        child1.Should().NotBe(child2);
    }

    [Fact]
    public void ToString_WithName_ShouldReturnFormattedString()
    {
        // Arrange
        var child = new Child(10, Gender.Male, "Moshe");

        // Act & Assert
        child.ToString().Should().Be("Moshe (10, Male)");
    }

    [Fact]
    public void ToString_WithoutName_ShouldReturnGenderAndAge()
    {
        // Arrange
        var child = new Child(10, Gender.Male);

        // Act & Assert
        child.ToString().Should().Be("Male, Age 10");
    }
}
