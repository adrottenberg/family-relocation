using FamilyRelocation.API.Controllers;
using FamilyRelocation.Application.Applicants.Commands.CreateApplicant;
using FamilyRelocation.Application.Applicants.DTOs;
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
