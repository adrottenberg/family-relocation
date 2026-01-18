using FamilyRelocation.Application.Applicants.Commands.UpdateApplicant;
using FamilyRelocation.Application.Applicants.DTOs;
using FluentValidation.TestHelper;

namespace FamilyRelocation.API.Tests.Applicants;

public class UpdateApplicantCommandValidatorTests
{
    private readonly UpdateApplicantCommandValidator _validator;

    public UpdateApplicantCommandValidatorTests()
    {
        _validator = new UpdateApplicantCommandValidator();
    }

    #region ID Validation

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { Id = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Applicant ID is required");
    }

    [Fact]
    public void Validate_WithValidId_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    #endregion

    #region Husband Validation

    [Fact]
    public void Validate_WithNullHusband_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateApplicantCommand
        {
            Id = Guid.NewGuid(),
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
        result.ShouldHaveValidationErrorFor(x => x.Husband.Email)
            .WithErrorMessage("Husband's email must be a valid email address");
    }

    [Fact]
    public void Validate_WithValidHusbandEmail_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Husband = command.Husband with { Email = "test@example.com" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Husband.Email);
    }

    #endregion

    #region Wife Validation

    [Fact]
    public void Validate_WithWifeMissingFirstName_ShouldHaveError()
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
    public void Validate_WithValidWife_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Wife = new SpouseInfoDto
            {
                FirstName = "Sarah",
                MaidenName = "Goldstein",
                Email = "sarah@example.com"
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
    public void Validate_WithAddressMissingStreet_ShouldHaveError()
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
        result.ShouldHaveValidationErrorFor("Children[0].Age");
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
                new ChildDto { Age = 5, Gender = "Unknown" }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Children[0].Gender")
            .WithErrorMessage("Child gender must be 'Male' or 'Female'");
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
                new ChildDto { Age = 3, Gender = "Female", Name = "Miriam" }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Community Info Validation

    [Fact]
    public void Validate_WithTooLongCurrentKehila_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            CurrentKehila = new string('a', 201)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentKehila)
            .WithErrorMessage("Current kehila cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_WithTooLongShabbosShul_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            ShabbosShul = new string('a', 201)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShabbosShul)
            .WithErrorMessage("Shabbos shul cannot exceed 200 characters");
    }

    #endregion

    #region Valid Command

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
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
            ShabbosShul = "Young Israel of Union"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    private static UpdateApplicantCommand CreateValidCommand()
    {
        return new UpdateApplicantCommand
        {
            Id = Guid.NewGuid(),
            Husband = new HusbandInfoDto
            {
                FirstName = "Moshe",
                LastName = "Cohen",
                Email = "moshe@example.com"
            }
        };
    }
}
