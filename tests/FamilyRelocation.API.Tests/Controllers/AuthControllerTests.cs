using FamilyRelocation.API.Controllers;
using FamilyRelocation.API.Models.Auth;
using FamilyRelocation.Application.Auth;
using FamilyRelocation.Application.Auth.Models;
using FamilyRelocation.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FamilyRelocation.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<IUserRoleService> _userRoleServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        _userRoleServiceMock = new Mock<IUserRoleService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _configurationMock = new Mock<IConfiguration>();
        _controller = new AuthController(
            _authServiceMock.Object,
            _userRoleServiceMock.Object,
            _currentUserServiceMock.Object,
            _configurationMock.Object);
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "Password123!" };
        var tokens = new AuthTokens
        {
            AccessToken = "access-token",
            IdToken = "id-token",
            RefreshToken = "refresh-token",
            ExpiresIn = 3600
        };
        _authServiceMock.Setup(x => x.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(AuthResult.SuccessResult(tokens));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LoginResponse>().Subject;
        response.AccessToken.Should().Be("access-token");
        response.IdToken.Should().Be("id-token");
        response.RefreshToken.Should().Be("refresh-token");
        response.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public async Task Login_WithChallengeRequired_ReturnsChallengeResponse()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "TempPassword!" };
        var challenge = new ChallengeInfo
        {
            ChallengeName = "NEW_PASSWORD_REQUIRED",
            Session = "session-token",
            Message = "Please set a new password",
            RequiredFields = new[] { "newPassword" }
        };
        _authServiceMock.Setup(x => x.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(AuthResult.ChallengeResult(challenge));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ChallengeResponse>().Subject;
        response.ChallengeName.Should().Be("NEW_PASSWORD_REQUIRED");
        response.Session.Should().Be("session-token");
        response.Message.Should().Be("Please set a new password");
        response.RequiredFields.Should().Contain("newPassword");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };
        _authServiceMock.Setup(x => x.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(AuthResult.ErrorResult("Invalid email or password", AuthErrorType.InvalidCredentials));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.Value.Should().BeEquivalentTo(new { message = "Invalid email or password" });
    }

    [Fact]
    public async Task Login_WithUnconfirmedUser_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "Password123!" };
        _authServiceMock.Setup(x => x.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(AuthResult.ErrorResult("Email not verified", AuthErrorType.UserNotConfirmed));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Email not verified" });
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest { Email = "", Password = "Password123!" };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Email is required" });
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "" };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Password is required" });
    }

    #endregion

    #region RespondToChallenge Tests

    [Fact]
    public async Task RespondToChallenge_WithValidResponse_ReturnsTokens()
    {
        // Arrange
        var request = new ChallengeRequest
        {
            Email = "test@example.com",
            ChallengeName = "NEW_PASSWORD_REQUIRED",
            Session = "session-token",
            Responses = new Dictionary<string, string> { ["newPassword"] = "NewPassword123!" }
        };
        var tokens = new AuthTokens
        {
            AccessToken = "access-token",
            IdToken = "id-token",
            RefreshToken = "refresh-token",
            ExpiresIn = 3600
        };
        _authServiceMock.Setup(x => x.RespondToChallengeAsync(It.IsAny<ChallengeResponseRequest>()))
            .ReturnsAsync(AuthResult.SuccessResult(tokens));

        // Act
        var result = await _controller.RespondToChallenge(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LoginResponse>().Subject;
        response.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task RespondToChallenge_WithAnotherChallenge_ReturnsChallengeResponse()
    {
        // Arrange
        var request = new ChallengeRequest
        {
            Email = "test@example.com",
            ChallengeName = "NEW_PASSWORD_REQUIRED",
            Session = "session-token",
            Responses = new Dictionary<string, string> { ["newPassword"] = "NewPassword123!" }
        };
        var nextChallenge = new ChallengeInfo
        {
            ChallengeName = "SMS_MFA",
            Session = "new-session",
            Message = "Enter the verification code",
            RequiredFields = new[] { "mfaCode" }
        };
        _authServiceMock.Setup(x => x.RespondToChallengeAsync(It.IsAny<ChallengeResponseRequest>()))
            .ReturnsAsync(AuthResult.ChallengeResult(nextChallenge));

        // Act
        var result = await _controller.RespondToChallenge(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ChallengeResponse>().Subject;
        response.ChallengeName.Should().Be("SMS_MFA");
    }

    [Fact]
    public async Task RespondToChallenge_WithInvalidSession_ReturnsUnauthorized()
    {
        // Arrange
        var request = new ChallengeRequest
        {
            Email = "test@example.com",
            ChallengeName = "NEW_PASSWORD_REQUIRED",
            Session = "expired-session",
            Responses = new Dictionary<string, string> { ["newPassword"] = "NewPassword123!" }
        };
        _authServiceMock.Setup(x => x.RespondToChallengeAsync(It.IsAny<ChallengeResponseRequest>()))
            .ReturnsAsync(AuthResult.ErrorResult("Session expired", AuthErrorType.InvalidSession));

        // Act
        var result = await _controller.RespondToChallenge(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task RespondToChallenge_WithEmptySession_ReturnsBadRequest()
    {
        // Arrange
        var request = new ChallengeRequest
        {
            Email = "test@example.com",
            ChallengeName = "NEW_PASSWORD_REQUIRED",
            Session = "",
            Responses = new Dictionary<string, string> { ["newPassword"] = "NewPassword123!" }
        };

        // Act
        var result = await _controller.RespondToChallenge(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Session is required" });
    }

    [Fact]
    public async Task RespondToChallenge_WithEmptyResponses_ReturnsBadRequest()
    {
        // Arrange
        var request = new ChallengeRequest
        {
            Email = "test@example.com",
            ChallengeName = "NEW_PASSWORD_REQUIRED",
            Session = "session-token",
            Responses = new Dictionary<string, string>()
        };

        // Act
        var result = await _controller.RespondToChallenge(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Challenge responses are required" });
    }

    #endregion

    #region Refresh Tests

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var request = new RefreshTokenRequest { Username = "user-id", RefreshToken = "refresh-token" };
        var result = new TokenRefreshResult
        {
            Success = true,
            Tokens = new AuthTokens
            {
                AccessToken = "new-access-token",
                IdToken = "new-id-token",
                RefreshToken = null,
                ExpiresIn = 3600
            }
        };
        _authServiceMock.Setup(x => x.RefreshTokensAsync(request.Username, request.RefreshToken))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.Refresh(request);

        // Assert
        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<RefreshTokenResponse>().Subject;
        response.AccessToken.Should().Be("new-access-token");
        response.IdToken.Should().Be("new-id-token");
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest { Username = "user-id", RefreshToken = "invalid-token" };
        _authServiceMock.Setup(x => x.RefreshTokensAsync(request.Username, request.RefreshToken))
            .ReturnsAsync(new TokenRefreshResult { Success = false, ErrorMessage = "Invalid refresh token" });

        // Act
        var result = await _controller.Refresh(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Refresh_WithEmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new RefreshTokenRequest { Username = "", RefreshToken = "refresh-token" };

        // Act
        var result = await _controller.Refresh(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Username is required" });
    }

    #endregion

    #region ForgotPassword Tests

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ReturnsSuccess()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "test@example.com" };
        _authServiceMock.Setup(x => x.RequestPasswordResetAsync(request.Email))
            .ReturnsAsync(new OperationResult { Success = true, Message = "Password reset code sent" });

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { message = "Password reset code sent" });
    }

    [Fact]
    public async Task ForgotPassword_WithTooManyRequests_ReturnsBadRequest()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "test@example.com" };
        _authServiceMock.Setup(x => x.RequestPasswordResetAsync(request.Email))
            .ReturnsAsync(new OperationResult
            {
                Success = false,
                ErrorMessage = "Too many requests",
                ErrorType = AuthErrorType.TooManyRequests
            });

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ForgotPassword_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "" };

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Email is required" });
    }

    #endregion

    #region ConfirmForgotPassword Tests

    [Fact]
    public async Task ConfirmForgotPassword_WithValidCode_ReturnsSuccess()
    {
        // Arrange
        var request = new ConfirmForgotPasswordRequest
        {
            Email = "test@example.com",
            Code = "123456",
            NewPassword = "NewPassword123!"
        };
        _authServiceMock.Setup(x => x.ConfirmPasswordResetAsync(request.Email, request.Code, request.NewPassword))
            .ReturnsAsync(new OperationResult { Success = true, Message = "Password reset successfully" });

        // Act
        var result = await _controller.ConfirmForgotPassword(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { message = "Password reset successfully" });
    }

    [Fact]
    public async Task ConfirmForgotPassword_WithInvalidCode_ReturnsBadRequest()
    {
        // Arrange
        var request = new ConfirmForgotPasswordRequest
        {
            Email = "test@example.com",
            Code = "wrong-code",
            NewPassword = "NewPassword123!"
        };
        _authServiceMock.Setup(x => x.ConfirmPasswordResetAsync(request.Email, request.Code, request.NewPassword))
            .ReturnsAsync(new OperationResult { Success = false, ErrorMessage = "Invalid verification code" });

        // Act
        var result = await _controller.ConfirmForgotPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ConfirmForgotPassword_WithEmptyCode_ReturnsBadRequest()
    {
        // Arrange
        var request = new ConfirmForgotPasswordRequest
        {
            Email = "test@example.com",
            Code = "",
            NewPassword = "NewPassword123!"
        };

        // Act
        var result = await _controller.ConfirmForgotPassword(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Verification code is required" });
    }

    #endregion

    #region ResendConfirmation Tests

    [Fact]
    public async Task ResendConfirmation_WithValidEmail_ReturnsSuccess()
    {
        // Arrange
        var request = new ResendConfirmationRequest { Email = "test@example.com" };
        _authServiceMock.Setup(x => x.ResendConfirmationCodeAsync(request.Email))
            .ReturnsAsync(new OperationResult { Success = true, Message = "Confirmation code sent" });

        // Act
        var result = await _controller.ResendConfirmation(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { message = "Confirmation code sent" });
    }

    [Fact]
    public async Task ResendConfirmation_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResendConfirmationRequest { Email = "" };

        // Act
        var result = await _controller.ResendConfirmation(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Email is required" });
    }

    #endregion

    #region ConfirmEmail Tests

    [Fact]
    public async Task ConfirmEmail_WithValidCode_ReturnsSuccess()
    {
        // Arrange
        var request = new ConfirmEmailRequest { Email = "test@example.com", Code = "123456" };
        _authServiceMock.Setup(x => x.ConfirmEmailAsync(request.Email, request.Code))
            .ReturnsAsync(new OperationResult { Success = true, Message = "Email verified successfully" });

        // Act
        var result = await _controller.ConfirmEmail(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { message = "Email verified successfully" });
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidCode_ReturnsBadRequest()
    {
        // Arrange
        var request = new ConfirmEmailRequest { Email = "test@example.com", Code = "wrong-code" };
        _authServiceMock.Setup(x => x.ConfirmEmailAsync(request.Email, request.Code))
            .ReturnsAsync(new OperationResult { Success = false, ErrorMessage = "Invalid verification code" });

        // Act
        var result = await _controller.ConfirmEmail(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ConfirmEmail_WithEmptyCode_ReturnsBadRequest()
    {
        // Arrange
        var request = new ConfirmEmailRequest { Email = "test@example.com", Code = "" };

        // Act
        var result = await _controller.ConfirmEmail(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Verification code is required" });
    }

    #endregion
}
