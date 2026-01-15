using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FamilyRelocation.Application.Auth;
using FamilyRelocation.Application.Auth.Models;
using Moq;

namespace FamilyRelocation.IntegrationTests;

public class AuthEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthEndpointsTests()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        _factory = new CustomWebApplicationFactory<Program>
        {
            MockAuthService = _authServiceMock.Object
        };
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    #region Login Endpoint Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange
        var tokens = new AuthTokens
        {
            AccessToken = "test-access-token",
            IdToken = "test-id-token",
            RefreshToken = "test-refresh-token",
            ExpiresIn = 3600
        };
        _authServiceMock.Setup(x => x.LoginAsync("test@example.com", "Password123!"))
            .ReturnsAsync(AuthResult.SuccessResult(tokens));

        var client = _factory.CreateClient();
        var request = new { email = "test@example.com", password = "Password123!" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);
        result!.AccessToken.Should().Be("test-access-token");
        result.IdToken.Should().Be("test-id-token");
        result.RefreshToken.Should().Be("test-refresh-token");
    }

    [Fact]
    public async Task Login_WithChallengeRequired_ReturnsOkWithChallenge()
    {
        // Arrange
        var challenge = new ChallengeInfo
        {
            ChallengeName = "NEW_PASSWORD_REQUIRED",
            Session = "test-session",
            Message = "Please set a new password",
            RequiredFields = new[] { "newPassword" }
        };
        _authServiceMock.Setup(x => x.LoginAsync("test@example.com", "TempPassword!"))
            .ReturnsAsync(AuthResult.ChallengeResult(challenge));

        var client = _factory.CreateClient();
        var request = new { email = "test@example.com", password = "TempPassword!" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ChallengeResponseDto>(content, _jsonOptions);
        result!.ChallengeName.Should().Be("NEW_PASSWORD_REQUIRED");
        result.Session.Should().Be("test-session");
        result.RequiredFields.Should().Contain("newPassword");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        _authServiceMock.Setup(x => x.LoginAsync("test@example.com", "WrongPassword"))
            .ReturnsAsync(AuthResult.ErrorResult("Invalid email or password", AuthErrorType.InvalidCredentials));

        var client = _factory.CreateClient();
        var request = new { email = "test@example.com", password = "WrongPassword" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithMissingEmail_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { email = "", password = "Password123!" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email is required");
    }

    #endregion

    #region RespondToChallenge Endpoint Tests

    [Fact]
    public async Task RespondToChallenge_WithValidResponse_ReturnsOkWithTokens()
    {
        // Arrange
        var tokens = new AuthTokens
        {
            AccessToken = "new-access-token",
            IdToken = "new-id-token",
            RefreshToken = "new-refresh-token",
            ExpiresIn = 3600
        };
        _authServiceMock.Setup(x => x.RespondToChallengeAsync(It.IsAny<ChallengeResponseRequest>()))
            .ReturnsAsync(AuthResult.SuccessResult(tokens));

        var client = _factory.CreateClient();
        var request = new
        {
            email = "test@example.com",
            challengeName = "NEW_PASSWORD_REQUIRED",
            session = "test-session",
            responses = new Dictionary<string, string> { ["newPassword"] = "NewPassword123!" }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/respond-to-challenge", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);
        result!.AccessToken.Should().Be("new-access-token");
    }

    [Fact]
    public async Task RespondToChallenge_WithMissingSession_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new
        {
            email = "test@example.com",
            challengeName = "NEW_PASSWORD_REQUIRED",
            session = "",
            responses = new Dictionary<string, string> { ["newPassword"] = "NewPassword123!" }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/respond-to-challenge", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Session is required");
    }

    #endregion

    #region Refresh Endpoint Tests

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsOkWithNewTokens()
    {
        // Arrange
        var result = new TokenRefreshResult
        {
            Success = true,
            Tokens = new AuthTokens
            {
                AccessToken = "refreshed-access-token",
                IdToken = "refreshed-id-token",
                RefreshToken = null,
                ExpiresIn = 3600
            }
        };
        _authServiceMock.Setup(x => x.RefreshTokensAsync("user-id", "refresh-token"))
            .ReturnsAsync(result);

        var client = _factory.CreateClient();
        var request = new { username = "user-id", refreshToken = "refresh-token" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var responseDto = JsonSerializer.Deserialize<RefreshResponseDto>(content, _jsonOptions);
        responseDto!.AccessToken.Should().Be("refreshed-access-token");
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        _authServiceMock.Setup(x => x.RefreshTokensAsync("user-id", "invalid-token"))
            .ReturnsAsync(new TokenRefreshResult { Success = false, ErrorMessage = "Invalid refresh token" });

        var client = _factory.CreateClient();
        var request = new { username = "user-id", refreshToken = "invalid-token" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region ForgotPassword Endpoint Tests

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ReturnsOk()
    {
        // Arrange
        _authServiceMock.Setup(x => x.RequestPasswordResetAsync("test@example.com"))
            .ReturnsAsync(new OperationResult { Success = true, Message = "Password reset code sent" });

        var client = _factory.CreateClient();
        var request = new { email = "test@example.com" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region ConfirmForgotPassword Endpoint Tests

    [Fact]
    public async Task ConfirmForgotPassword_WithValidCode_ReturnsOk()
    {
        // Arrange
        _authServiceMock.Setup(x => x.ConfirmPasswordResetAsync("test@example.com", "123456", "NewPassword123!"))
            .ReturnsAsync(new OperationResult { Success = true, Message = "Password reset successfully" });

        var client = _factory.CreateClient();
        var request = new { email = "test@example.com", code = "123456", newPassword = "NewPassword123!" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/confirm-forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region ResendConfirmation Endpoint Tests

    [Fact]
    public async Task ResendConfirmation_WithValidEmail_ReturnsOk()
    {
        // Arrange
        _authServiceMock.Setup(x => x.ResendConfirmationCodeAsync("test@example.com"))
            .ReturnsAsync(new OperationResult { Success = true, Message = "Confirmation code sent" });

        var client = _factory.CreateClient();
        var request = new { email = "test@example.com" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/resend-confirmation", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region ConfirmEmail Endpoint Tests

    [Fact]
    public async Task ConfirmEmail_WithValidCode_ReturnsOk()
    {
        // Arrange
        _authServiceMock.Setup(x => x.ConfirmEmailAsync("test@example.com", "123456"))
            .ReturnsAsync(new OperationResult { Success = true, Message = "Email verified successfully" });

        var client = _factory.CreateClient();
        var request = new { email = "test@example.com", code = "123456" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/confirm-email", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region DTOs for deserialization

    private record LoginResponseDto(string AccessToken, string IdToken, string? RefreshToken, int ExpiresIn);
    private record ChallengeResponseDto(string ChallengeName, string Session, string Message, string[] RequiredFields);
    private record RefreshResponseDto(string AccessToken, string IdToken, int ExpiresIn);

    #endregion
}
