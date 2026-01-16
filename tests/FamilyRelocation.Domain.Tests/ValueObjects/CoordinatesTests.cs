using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.ValueObjects;

public class CoordinatesTests
{
    [Fact]
    public void Constructor_WithValidCoordinates_ShouldCreateCoordinates()
    {
        // Arrange & Act
        var coords = new Coordinates(40.6782, -74.1934);

        // Assert
        coords.Latitude.Should().Be(40.6782);
        coords.Longitude.Should().Be(-74.1934);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    public void Constructor_WithInvalidLatitude_ShouldThrowArgumentException(double lat, double lon)
    {
        // Arrange & Act
        var act = () => new Coordinates(lat, lon);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Latitude*");
    }

    [Theory]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    public void Constructor_WithInvalidLongitude_ShouldThrowArgumentException(double lat, double lon)
    {
        // Arrange & Act
        var act = () => new Coordinates(lat, lon);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Longitude*");
    }

    [Fact]
    public void DistanceToMiles_ShouldCalculateCorrectDistance()
    {
        // Arrange - Union, NJ to Times Square, NYC (approximately 17 miles)
        var union = new Coordinates(40.6956, -74.2632);
        var timesSquare = new Coordinates(40.7580, -73.9855);

        // Act
        var distance = union.DistanceToMiles(timesSquare);

        // Assert
        distance.Should().BeApproximately(17.0, 2.0); // Allow 2 mile tolerance
    }

    [Fact]
    public void DistanceToMiles_SameLocation_ShouldReturnZero()
    {
        // Arrange
        var coords1 = new Coordinates(40.6782, -74.1934);
        var coords2 = new Coordinates(40.6782, -74.1934);

        // Act
        var distance = coords1.DistanceToMiles(coords2);

        // Assert
        distance.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void DistanceToKilometers_ShouldBeApproximately1_6TimesMiles()
    {
        // Arrange
        var coords1 = new Coordinates(40.6956, -74.2632);
        var coords2 = new Coordinates(40.7580, -73.9855);

        // Act
        var miles = coords1.DistanceToMiles(coords2);
        var km = coords1.DistanceToKilometers(coords2);

        // Assert
        km.Should().BeApproximately(miles * 1.60934, 0.1);
    }

    [Fact]
    public void Equals_WithSameCoordinates_ShouldReturnTrue()
    {
        // Arrange
        var coords1 = new Coordinates(40.678200, -74.193400);
        var coords2 = new Coordinates(40.678200, -74.193400);

        // Act & Assert
        coords1.Should().Be(coords2);
    }

    [Fact]
    public void Equals_WithSlightlyDifferentCoordinates_ShouldNotBeEqual()
    {
        // Arrange - Records compare exact double values
        var coords1 = new Coordinates(40.6782001, -74.1934001);
        var coords2 = new Coordinates(40.6782002, -74.1934002);

        // Act & Assert - Even slight differences mean not equal
        coords1.Should().NotBe(coords2);
    }

    [Fact]
    public void Equals_WithDifferentCoordinates_ShouldReturnFalse()
    {
        // Arrange
        var coords1 = new Coordinates(40.6782, -74.1934);
        var coords2 = new Coordinates(40.7580, -73.9855);

        // Act & Assert
        coords1.Should().NotBe(coords2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedCoordinates()
    {
        // Arrange
        var coords = new Coordinates(40.678234, -74.193456);

        // Act & Assert
        coords.ToString().Should().Be("40.678234, -74.193456");
    }
}
