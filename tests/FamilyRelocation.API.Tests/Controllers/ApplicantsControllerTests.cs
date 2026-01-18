using FamilyRelocation.API.Controllers;
using FamilyRelocation.Application.Applicants.Commands.CreateApplicant;
using FamilyRelocation.Application.Applicants.Commands.UpdateApplicant;
using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Applicants.Queries.GetApplicantById;
using FamilyRelocation.Application.Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Tests.Controllers;

public class ApplicantsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ApplicantsController _controller;

    public ApplicantsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ApplicantsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_WithValidCommand_ReturnsCreatedResult()
    {
        // Arrange
        var command = CreateValidCommand();
        var expectedDto = new ApplicantDto
        {
            Id = Guid.NewGuid(),
            Husband = new HusbandInfoDto
            {
                FirstName = "Moshe",
                LastName = "Cohen"
            },
            FamilyName = "Moshe Cohen",
            NumberOfChildren = 0,
            IsPendingBoardReview = true,
            IsSelfSubmitted = true,
            CreatedDate = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateApplicantCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be("GetById");
        createdResult.RouteValues!["id"].Should().Be(expectedDto.Id);
        createdResult.Value.Should().Be(expectedDto);
    }

    [Fact]
    public async Task Create_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with
        {
            Husband = command.Husband with { Email = "duplicate@example.com" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateApplicantCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEmailException("duplicate@example.com"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().BeEquivalentTo(new
        {
            message = "An applicant with email 'duplicate@example.com' already exists.",
            email = "duplicate@example.com"
        });
    }

    [Fact]
    public async Task Create_SendsCommandToMediator()
    {
        // Arrange
        var command = CreateValidCommand();
        var expectedDto = new ApplicantDto
        {
            Id = Guid.NewGuid(),
            Husband = new HusbandInfoDto
            {
                FirstName = "Moshe",
                LastName = "Cohen"
            },
            FamilyName = "Moshe Cohen",
            NumberOfChildren = 0,
            IsPendingBoardReview = true,
            IsSelfSubmitted = true,
            CreatedDate = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateApplicantCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        await _controller.Create(command);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(It.Is<CreateApplicantCommand>(c =>
                c.Husband.FirstName == "Moshe" &&
                c.Husband.LastName == "Cohen"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenApplicantExists_ReturnsOkWithApplicant()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var expectedDto = new ApplicantDto
        {
            Id = applicantId,
            Husband = new HusbandInfoDto
            {
                FirstName = "Moshe",
                LastName = "Cohen"
            },
            FamilyName = "Moshe Cohen",
            NumberOfChildren = 0,
            IsPendingBoardReview = true,
            IsSelfSubmitted = false,
            CreatedDate = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(It.Is<GetApplicantByIdQuery>(q => q.Id == applicantId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.GetById(applicantId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedDto);
    }

    [Fact]
    public async Task GetById_WhenApplicantNotFound_ReturnsNotFound()
    {
        // Arrange
        var applicantId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.Is<GetApplicantByIdQuery>(q => q.Id == applicantId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicantDto?)null);

        // Act
        var result = await _controller.GetById(applicantId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_SendsQueryToMediator()
    {
        // Arrange
        var applicantId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicantByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicantDto?)null);

        // Act
        await _controller.GetById(applicantId);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(It.Is<GetApplicantByIdQuery>(q => q.Id == applicantId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidCommand_ReturnsOkResult()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var command = CreateValidUpdateCommand(applicantId);
        var expectedDto = new ApplicantDto
        {
            Id = applicantId,
            Husband = new HusbandInfoDto
            {
                FirstName = "Moshe",
                LastName = "Cohen"
            },
            FamilyName = "Moshe Cohen",
            NumberOfChildren = 0,
            IsPendingBoardReview = true,
            IsSelfSubmitted = false,
            CreatedDate = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateApplicantCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Update(applicantId, command);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedDto);
    }

    [Fact]
    public async Task Update_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var urlId = Guid.NewGuid();
        var commandId = Guid.NewGuid();
        var command = CreateValidUpdateCommand(commandId);

        // Act
        var result = await _controller.Update(urlId, command);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new
        {
            message = "ID in URL does not match ID in request body"
        });
    }

    [Fact]
    public async Task Update_WhenApplicantNotFound_ReturnsNotFound()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var command = CreateValidUpdateCommand(applicantId);

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateApplicantCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Applicant", applicantId));

        // Act
        var result = await _controller.Update(applicantId, command);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().BeEquivalentTo(new
        {
            message = $"Applicant with ID '{applicantId}' was not found."
        });
    }

    [Fact]
    public async Task Update_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var command = CreateValidUpdateCommand(applicantId);
        command = command with
        {
            Husband = command.Husband with { Email = "duplicate@example.com" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateApplicantCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEmailException("duplicate@example.com"));

        // Act
        var result = await _controller.Update(applicantId, command);

        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().BeEquivalentTo(new
        {
            message = "An applicant with email 'duplicate@example.com' already exists.",
            email = "duplicate@example.com"
        });
    }

    [Fact]
    public async Task Update_SendsCommandToMediator()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var command = CreateValidUpdateCommand(applicantId);
        var expectedDto = new ApplicantDto
        {
            Id = applicantId,
            Husband = new HusbandInfoDto
            {
                FirstName = "Moshe",
                LastName = "Cohen"
            },
            FamilyName = "Moshe Cohen",
            NumberOfChildren = 0,
            IsPendingBoardReview = true,
            IsSelfSubmitted = false,
            CreatedDate = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateApplicantCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        await _controller.Update(applicantId, command);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(It.Is<UpdateApplicantCommand>(c =>
                c.Id == applicantId &&
                c.Husband.FirstName == "Moshe" &&
                c.Husband.LastName == "Cohen"),
                It.IsAny<CancellationToken>()),
            Times.Once);
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

    private static UpdateApplicantCommand CreateValidUpdateCommand(Guid id)
    {
        return new UpdateApplicantCommand
        {
            Id = id,
            Husband = new HusbandInfoDto
            {
                FirstName = "Moshe",
                LastName = "Cohen"
            }
        };
    }
}
