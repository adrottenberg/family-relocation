using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.ValueObjects;

public class ChildTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateChild()
    {
        // Arrange & Act
        var child = new Child("Moshe", 10, Gender.Male);

        // Assert
        child.Name.Should().Be("Moshe");
        child.Age.Should().Be(10);
        child.Gender.Should().Be(Gender.Male);
    }

    [Fact]
    public void Constructor_WithOptionalFields_ShouldSetThem()
    {
        // Arrange & Act
        var child = new Child("Sarah", 12, Gender.Female, "Bais Yaakov", "7th Grade", "Good student");

        // Assert
        child.School.Should().Be("Bais Yaakov");
        child.Grade.Should().Be("7th Grade");
        child.Notes.Should().Be("Good student");
    }

    [Fact]
    public void Constructor_ShouldTrimWhitespace()
    {
        // Arrange & Act
        var child = new Child("  Moshe  ", 10, Gender.Male, "  School  ", "  Grade  ");

        // Assert
        child.Name.Should().Be("Moshe");
        child.School.Should().Be("School");
        child.Grade.Should().Be("Grade");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyName_ShouldThrowArgumentException(string? name)
    {
        // Arrange & Act
        var act = () => new Child(name!, 10, Gender.Male);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*required*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(51)]
    public void Constructor_WithInvalidAge_ShouldThrowArgumentException(int age)
    {
        // Arrange & Act
        var act = () => new Child("Moshe", age, Gender.Male);

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
        var child = new Child("Test", age, Gender.Male);

        // Act & Assert
        child.IsSchoolAge.Should().Be(expectedIsSchoolAge);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange - Records compare exact values
        var child1 = new Child("Moshe", 10, Gender.Male);
        var child2 = new Child("Moshe", 10, Gender.Male);

        // Act & Assert
        child1.Should().Be(child2);
    }

    [Fact]
    public void Equals_WithDifferentCasing_ShouldReturnFalse()
    {
        // Arrange - Records compare exact values
        var child1 = new Child("Moshe", 10, Gender.Male);
        var child2 = new Child("MOSHE", 10, Gender.Male);

        // Act & Assert
        child1.Should().NotBe(child2);
    }

    [Fact]
    public void Equals_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        var child1 = new Child("Moshe", 10, Gender.Male);
        var child2 = new Child("David", 10, Gender.Male);

        // Act & Assert
        child1.Should().NotBe(child2);
    }

    [Fact]
    public void Equals_WithDifferentAge_ShouldReturnFalse()
    {
        // Arrange
        var child1 = new Child("Moshe", 10, Gender.Male);
        var child2 = new Child("Moshe", 12, Gender.Male);

        // Act & Assert
        child1.Should().NotBe(child2);
    }

    [Fact]
    public void Equals_WithDifferentGender_ShouldReturnFalse()
    {
        // Arrange
        var child1 = new Child("Name", 10, Gender.Male);
        var child2 = new Child("Name", 10, Gender.Female);

        // Act & Assert
        child1.Should().NotBe(child2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var child = new Child("Moshe", 10, Gender.Male);

        // Act & Assert
        child.ToString().Should().Be("Moshe (10, Male)");
    }
}
