using FamilyRelocation.Application.Applicants.Commands.CreateApplicant;
using FamilyRelocation.Application.Applicants.DTOs;
using FluentValidation.TestHelper;

namespace FamilyRelocation.API.Tests.Applicants;

public class CreateApplicantCommandValidatorTests
{
    private readonly CreateApplicantCommandValidator _validator;

    public CreateApplicantCommandValidatorTests()
    {
        _validator = new CreateApplicantCommandValidator();
    }

    #region Husband Validation

    [Fact]
    public void Validate_WithNullHusband_ShouldHaveError()
    {
        // Arrange
        var command = new CreateApplicantCommand
        {
            Husband = null!
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Husband)
            .WithErrorMessage("Husband information is required");
    }

    [Fact]
    public void Validate_WithEmptyHusbandFirstName_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Husband = command.Husband with { FirstName = "" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Husband.FirstName);
    }

    [Fact]
    public void Validate_WithEmptyHusbandLastName_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Husband = command.Husband with { LastName = "" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Husband.LastName);
    }

    [Fact]
    public void Validate_WithTooLongHusbandFirstName_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Husband = command.Husband with { FirstName = new string('a', 101) }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Husband.FirstName)
            .WithErrorMessage("Husband's first name cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_WithInvalidHusbandEmail_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Husband = command.Husband with { Email = "invalid-email" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Husband.Email);
    }

    [Fact]
    public void Validate_WithValidHusbandEmail_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Husband = command.Husband with { Email = "valid@example.com" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Husband.Email);
    }

    [Fact]
    public void Validate_WithMultiplePrimaryHusbandPhones_ShouldNotHaveError()
    {
        // Multiple primary phones are allowed - handler auto-demotes to single primary
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Husband = command.Husband with
            {
                PhoneNumbers = new List<PhoneNumberDto>
                {
                    new() { Number = "2015551234", IsPrimary = true },
                    new() { Number = "2015555678", IsPrimary = true }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Wife Validation

    [Fact]
    public void Validate_WithNullWife_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Wife = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyWifeFirstName_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Wife = new SpouseInfoDto { FirstName = "" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Wife!.FirstName);
    }

    [Fact]
    public void Validate_WithInvalidWifeEmail_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Wife = new SpouseInfoDto { FirstName = "Sarah", Email = "invalid-email" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Wife!.Email);
    }

    [Fact]
    public void Validate_WithMultiplePrimaryWifePhones_ShouldNotHaveError()
    {
        // Multiple primary phones are allowed - handler auto-demotes to single primary
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Wife = new SpouseInfoDto
            {
                FirstName = "Sarah",
                PhoneNumbers = new List<PhoneNumberDto>
                {
                    new() { Number = "2015551234", IsPrimary = true },
                    new() { Number = "2015555678", IsPrimary = true }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Address Validation

    [Fact]
    public void Validate_WithNullAddress_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Address = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyAddressStreet_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Address = new AddressDto
            {
                Street = "",
                City = "Union",
                State = "NJ",
                ZipCode = "07083"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address!.Street);
    }

    [Fact]
    public void Validate_WithInvalidStateLength_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Address = new AddressDto
            {
                Street = "123 Main St",
                City = "Union",
                State = "New Jersey",
                ZipCode = "07083"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address!.State)
            .WithErrorMessage("State must be a 2-letter code");
    }

    [Fact]
    public void Validate_WithInvalidZipCode_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Address = new AddressDto
            {
                Street = "123 Main St",
                City = "Union",
                State = "NJ",
                ZipCode = "invalid"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address!.ZipCode);
    }

    [Fact]
    public void Validate_WithValidAddress_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Address = new AddressDto
            {
                Street = "123 Main St",
                City = "Union",
                State = "NJ",
                ZipCode = "07083"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Children Validation

    [Fact]
    public void Validate_WithInvalidChildAge_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Children = new List<ChildDto>
            {
                new ChildDto { Age = -1, Gender = "Male" }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validate_WithInvalidChildGender_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Children = new List<ChildDto>
            {
                new ChildDto { Age = 5, Gender = "Invalid" }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validate_WithValidChildren_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Children = new List<ChildDto>
            {
                new ChildDto { Age = 5, Gender = "Male", Name = "Yosef" },
                new ChildDto { Age = 3, Gender = "Female" }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Community Validation

    [Fact]
    public void Validate_WithTooLongKehila_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { CurrentKehila = new string('a', 201) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentKehila);
    }

    [Fact]
    public void Validate_WithTooLongShabbosShul_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { ShabbosShul = new string('a', 201) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShabbosShul);
    }

    #endregion

    #region Valid Command

    [Fact]
    public void Validate_WithMinimalValidCommand_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithFullValidCommand_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateApplicantCommand
        {
            Husband = new HusbandInfoDto
            {
                FirstName = "Moshe",
                LastName = "Cohen",
                FatherName = "Yaakov",
                Email = "moshe@example.com"
            },
            Wife = new SpouseInfoDto
            {
                FirstName = "Sarah",
                MaidenName = "Goldstein",
                Email = "sarah@example.com",
                HighSchool = "Bais Yaakov"
            },
            Address = new AddressDto
            {
                Street = "123 Main St",
                City = "Union",
                State = "NJ",
                ZipCode = "07083"
            },
            Children = new List<ChildDto>
            {
                new ChildDto { Age = 8, Gender = "Male", Name = "Yosef", School = "Torah Academy" }
            },
            CurrentKehila = "Brooklyn",
            ShabbosShul = "Bobov"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    private static CreateApplicantCommand CreateValidCommand()
    {
        return new CreateApplicantCommand
        {
            Husband = new HusbandInfoDto
            {
                FirstName = "Moshe",
                LastName = "Cohen"
            }
        };
    }
}
