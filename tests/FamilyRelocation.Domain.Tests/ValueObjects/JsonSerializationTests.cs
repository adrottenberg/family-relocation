using System.Text.Json;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyRelocation.Domain.Tests.ValueObjects;

/// <summary>
/// Tests to ensure value objects serialize correctly for PostgreSQL jsonb columns.
/// These tests verify that System.Text.Json can roundtrip our record value objects.
/// </summary>
public class JsonSerializationTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    #region PhoneNumber Tests

    [Fact]
    public void PhoneNumber_ShouldSerializeAndDeserialize()
    {
        // Arrange - PhoneNumber normalizes to 11 digits (1 + 10 digit number)
        var phoneNumber = new PhoneNumber("201-555-1234", PhoneType.Mobile, true);

        // Act
        var json = JsonSerializer.Serialize(phoneNumber, _options);
        var deserialized = JsonSerializer.Deserialize<PhoneNumber>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Number.Should().Be("12015551234"); // Normalized format
        deserialized.Type.Should().Be(PhoneType.Mobile);
        deserialized.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void PhoneNumberList_ShouldSerializeAndDeserialize()
    {
        // Arrange - PhoneNumber normalizes to digits
        var phoneNumbers = new List<PhoneNumber>
        {
            new("201-555-1234", PhoneType.Mobile, true),
            new("908-555-5678", PhoneType.Home, false),
            new("908-555-9999", PhoneType.Work, false)
        };

        // Act
        var json = JsonSerializer.Serialize(phoneNumbers, _options);
        var deserialized = JsonSerializer.Deserialize<List<PhoneNumber>>(json, _options);

        // Assert
        deserialized.Should().HaveCount(3);
        deserialized![0].Number.Should().Be("12015551234"); // Normalized
        deserialized[1].Type.Should().Be(PhoneType.Home);
        deserialized[2].IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void EmptyPhoneNumberList_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var phoneNumbers = new List<PhoneNumber>();

        // Act
        var json = JsonSerializer.Serialize(phoneNumbers, _options);
        var deserialized = JsonSerializer.Deserialize<List<PhoneNumber>>(json, _options);

        // Assert
        deserialized.Should().BeEmpty();
    }

    #endregion

    #region Child Tests

    [Fact]
    public void Child_ShouldSerializeAndDeserialize()
    {
        // Arrange - Child(name, age, gender, school, grade, notes)
        var child = new Child("Yosef", 8, Gender.Male, "Torah Academy");

        // Act
        var json = JsonSerializer.Serialize(child, _options);
        var deserialized = JsonSerializer.Deserialize<Child>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Name.Should().Be("Yosef");
        deserialized.Gender.Should().Be(Gender.Male);
        deserialized.Age.Should().Be(8);
        deserialized.School.Should().Be("Torah Academy");
    }

    [Fact]
    public void Child_WithoutSchool_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var child = new Child("Sarah", 2, Gender.Female, null);

        // Act
        var json = JsonSerializer.Serialize(child, _options);
        var deserialized = JsonSerializer.Deserialize<Child>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Name.Should().Be("Sarah");
        deserialized.School.Should().BeNull();
    }

    [Fact]
    public void ChildList_ShouldSerializeAndDeserialize()
    {
        // Arrange - Child(name, age, gender, school)
        var children = new List<Child>
        {
            new("Moshe", 10, Gender.Male, "Yeshiva Ketana"),
            new("Rivka", 8, Gender.Female, "Bais Yaakov"),
            new("Dovid", 3, Gender.Male, null)
        };

        // Act
        var json = JsonSerializer.Serialize(children, _options);
        var deserialized = JsonSerializer.Deserialize<List<Child>>(json, _options);

        // Assert
        deserialized.Should().HaveCount(3);
        deserialized![0].Name.Should().Be("Moshe");
        deserialized![1].School.Should().Be("Bais Yaakov");
        deserialized![2].Age.Should().Be(3);
    }

    #endregion

    #region ShulProximityPreference Tests

    [Fact]
    public void ShulProximityPreference_WithMaxDistance_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var preference = ShulProximityPreference.WithMaxDistance(0.75);

        // Act
        var json = JsonSerializer.Serialize(preference, _options);
        var deserialized = JsonSerializer.Deserialize<ShulProximityPreference>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.MaxWalkingDistanceMiles.Should().Be(0.75);
        deserialized.AnyShulAcceptable.Should().BeTrue();
    }

    [Fact]
    public void ShulProximityPreference_NoPreference_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var preference = ShulProximityPreference.NoPreference();

        // Act
        var json = JsonSerializer.Serialize(preference, _options);
        var deserialized = JsonSerializer.Deserialize<ShulProximityPreference>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.AnyShulAcceptable.Should().BeTrue();
        deserialized.MaxWalkingDistanceMiles.Should().BeNull();
        deserialized.PreferredShulIds.Should().BeEmpty();
    }

    [Fact]
    public void ShulProximityPreference_ForSpecificShuls_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var shulIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var preference = ShulProximityPreference.ForSpecificShuls(shulIds, 0.5);

        // Act
        var json = JsonSerializer.Serialize(preference, _options);
        var deserialized = JsonSerializer.Deserialize<ShulProximityPreference>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.PreferredShulIds.Should().HaveCount(2);
        deserialized.MaxWalkingDistanceMiles.Should().Be(0.5);
        deserialized.AnyShulAcceptable.Should().BeFalse();
    }

    [Fact]
    public void NullShulProximityPreference_ShouldSerializeAsNull()
    {
        // Arrange
        ShulProximityPreference? preference = null;

        // Act
        var json = JsonSerializer.Serialize(preference, _options);
        var deserialized = JsonSerializer.Deserialize<ShulProximityPreference?>(json, _options);

        // Assert
        json.Should().Be("null");
        deserialized.Should().BeNull();
    }

    #endregion

    #region FailedContractAttempt Tests

    [Fact]
    public void FailedContractAttempt_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var contractPrice = new Money(450000);
        var contractDate = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var failedDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var attempt = new FailedContractAttempt(propertyId, contractPrice, contractDate, failedDate, "Inspection issues");

        // Act
        var json = JsonSerializer.Serialize(attempt, _options);
        var deserialized = JsonSerializer.Deserialize<FailedContractAttempt>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.PropertyId.Should().Be(propertyId);
        deserialized.ContractPrice.Should().Be(contractPrice);
        deserialized.ContractDate.Should().Be(contractDate);
        deserialized.FailedDate.Should().Be(failedDate);
        deserialized.Reason.Should().Be("Inspection issues");
    }

    [Fact]
    public void FailedContractAttemptList_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var attempts = new List<FailedContractAttempt>
        {
            new(Guid.NewGuid(), new Money(450000), DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-20), "Financing fell through"),
            new(Guid.NewGuid(), new Money(475000), DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(-5), "Seller backed out")
        };

        // Act
        var json = JsonSerializer.Serialize(attempts, _options);
        var deserialized = JsonSerializer.Deserialize<List<FailedContractAttempt>>(json, _options);

        // Assert
        deserialized.Should().HaveCount(2);
        deserialized![0].Reason.Should().Be("Financing fell through");
        deserialized![1].ContractPrice!.Amount.Should().Be(475000);
    }

    #endregion

    #region RequiredFeatures (List<string>) Tests

    [Fact]
    public void RequiredFeaturesList_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var features = new List<string> { "Basement", "Garage", "Sukkah Porch", "Walk to Shul" };

        // Act
        var json = JsonSerializer.Serialize(features, _options);
        var deserialized = JsonSerializer.Deserialize<List<string>>(json, _options);

        // Assert
        deserialized.Should().HaveCount(4);
        deserialized.Should().Contain("Basement");
        deserialized.Should().Contain("Sukkah Porch");
    }

    [Fact]
    public void EmptyRequiredFeaturesList_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var features = new List<string>();

        // Act
        var json = JsonSerializer.Serialize(features, _options);
        var deserialized = JsonSerializer.Deserialize<List<string>>(json, _options);

        // Assert
        deserialized.Should().BeEmpty();
    }

    #endregion

    #region Money Tests (for JSON columns)

    [Fact]
    public void Money_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var money = new Money(500000m, "USD");

        // Act
        var json = JsonSerializer.Serialize(money, _options);
        var deserialized = JsonSerializer.Deserialize<Money>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Amount.Should().Be(500000m);
        deserialized.Currency.Should().Be("USD");
    }

    [Fact]
    public void Money_WithDecimalPlaces_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var money = new Money(499999.99m, "USD");

        // Act
        var json = JsonSerializer.Serialize(money, _options);
        var deserialized = JsonSerializer.Deserialize<Money>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Amount.Should().Be(499999.99m);
    }

    #endregion

    #region Coordinates Tests

    [Fact]
    public void Coordinates_ShouldSerializeAndDeserialize()
    {
        // Arrange - Union, NJ coordinates
        var coordinates = new Coordinates(40.6976, -74.2632);

        // Act
        var json = JsonSerializer.Serialize(coordinates, _options);
        var deserialized = JsonSerializer.Deserialize<Coordinates>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Latitude.Should().Be(40.6976);
        deserialized.Longitude.Should().Be(-74.2632);
    }

    #endregion
}
