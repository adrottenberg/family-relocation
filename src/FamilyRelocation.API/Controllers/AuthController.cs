using System.Security.Cryptography;
using System.Text;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using AuthModels = FamilyRelocation.API.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly string _clientId;
    private readonly string? _clientSecret;

    public AuthController(IAmazonCognitoIdentityProvider cognitoClient, IConfiguration configuration)
    {
        _cognitoClient = cognitoClient;
        _clientId = configuration["AWS:Cognito:ClientId"]
            ?? throw new InvalidOperationException("AWS:Cognito:ClientId configuration is required");
        _clientSecret = configuration["AWS:Cognito:ClientSecret"];
    }

    private string? ComputeSecretHash(string username)
    {
        if (string.IsNullOrEmpty(_clientSecret))
            return null;

        var message = username + _clientId;
        var keyBytes = Encoding.UTF8.GetBytes(_clientSecret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthModels.LoginResponse>> Login([FromBody] AuthModels.LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Password is required" });

        try
        {
            var authParameters = new Dictionary<string, string>
            {
                { "USERNAME", request.Email },
                { "PASSWORD", request.Password }
            };

            var secretHash = ComputeSecretHash(request.Email);
            if (secretHash != null)
                authParameters["SECRET_HASH"] = secretHash;

            var authRequest = new InitiateAuthRequest
            {
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                ClientId = _clientId,
                AuthParameters = authParameters
            };

            var response = await _cognitoClient.InitiateAuthAsync(authRequest);

            // Check if a challenge is required (e.g., NEW_PASSWORD_REQUIRED)
            if (!string.IsNullOrEmpty(response.ChallengeName))
            {
                return Ok(new AuthModels.ChallengeResponse
                {
                    ChallengeName = response.ChallengeName,
                    Session = response.Session,
                    Message = response.ChallengeName == "NEW_PASSWORD_REQUIRED"
                        ? "Password change required. Use POST /api/auth/respond-to-challenge to set a new password."
                        : $"Challenge required: {response.ChallengeName}"
                });
            }

            return Ok(new AuthModels.LoginResponse
            {
                AccessToken = response.AuthenticationResult.AccessToken,
                IdToken = response.AuthenticationResult.IdToken,
                RefreshToken = response.AuthenticationResult.RefreshToken,
                ExpiresIn = response.AuthenticationResult.ExpiresIn ?? 3600
            });
        }
        catch (NotAuthorizedException)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }
        catch (UserNotFoundException)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }
        catch (UserNotConfirmedException)
        {
            return BadRequest(new { message = "Email not verified. Use POST /api/auth/resend-confirmation to receive a new code." });
        }
        catch (PasswordResetRequiredException)
        {
            return BadRequest(new { message = "Password reset is required. Use POST /api/auth/forgot-password to initiate reset." });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] AuthModels.ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        try
        {
            var forgotRequest = new ForgotPasswordRequest
            {
                ClientId = _clientId,
                Username = request.Email
            };

            var secretHash = ComputeSecretHash(request.Email);
            if (secretHash != null)
                forgotRequest.SecretHash = secretHash;

            await _cognitoClient.ForgotPasswordAsync(forgotRequest);

            return Ok(new { message = "If an account exists with this email, a password reset code has been sent." });
        }
        catch (UserNotFoundException)
        {
            // Return same message to prevent email enumeration
            return Ok(new { message = "If an account exists with this email, a password reset code has been sent." });
        }
        catch (LimitExceededException)
        {
            return BadRequest(new { message = "Too many requests. Please try again later." });
        }
        catch (InvalidParameterException)
        {
            // User has no verified email/phone - return same generic message to prevent enumeration
            return Ok(new { message = "If an account exists with this email, a password reset code has been sent." });
        }
    }

    [HttpPost("confirm-forgot-password")]
    public async Task<IActionResult> ConfirmForgotPassword([FromBody] AuthModels.ConfirmForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { message = "Verification code is required" });

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "New password is required" });

        try
        {
            var confirmRequest = new ConfirmForgotPasswordRequest
            {
                ClientId = _clientId,
                Username = request.Email,
                ConfirmationCode = request.Code,
                Password = request.NewPassword
            };

            var secretHash = ComputeSecretHash(request.Email);
            if (secretHash != null)
                confirmRequest.SecretHash = secretHash;

            await _cognitoClient.ConfirmForgotPasswordAsync(confirmRequest);

            return Ok(new { message = "Password has been reset successfully. You can now log in." });
        }
        catch (CodeMismatchException)
        {
            return BadRequest(new { message = "Invalid verification code" });
        }
        catch (ExpiredCodeException)
        {
            return BadRequest(new { message = "Verification code has expired. Please request a new one." });
        }
        catch (InvalidPasswordException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] AuthModels.ResendConfirmationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        try
        {
            var resendRequest = new ResendConfirmationCodeRequest
            {
                ClientId = _clientId,
                Username = request.Email
            };

            var secretHash = ComputeSecretHash(request.Email);
            if (secretHash != null)
                resendRequest.SecretHash = secretHash;

            await _cognitoClient.ResendConfirmationCodeAsync(resendRequest);

            return Ok(new { message = "If an account exists with this email, a confirmation code has been sent." });
        }
        catch (UserNotFoundException)
        {
            // Return same message to prevent email enumeration
            return Ok(new { message = "If an account exists with this email, a confirmation code has been sent." });
        }
        catch (InvalidParameterException)
        {
            // User is already confirmed
            return BadRequest(new { message = "This email is already verified." });
        }
        catch (LimitExceededException)
        {
            return BadRequest(new { message = "Too many requests. Please try again later." });
        }
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] AuthModels.ConfirmEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { message = "Verification code is required" });

        try
        {
            var confirmSignUpRequest = new ConfirmSignUpRequest
            {
                ClientId = _clientId,
                Username = request.Email,
                ConfirmationCode = request.Code
            };

            var secretHash = ComputeSecretHash(request.Email);
            if (secretHash != null)
                confirmSignUpRequest.SecretHash = secretHash;

            await _cognitoClient.ConfirmSignUpAsync(confirmSignUpRequest);

            return Ok(new { message = "Email verified successfully. You can now log in." });
        }
        catch (CodeMismatchException)
        {
            return BadRequest(new { message = "Invalid verification code" });
        }
        catch (ExpiredCodeException)
        {
            return BadRequest(new { message = "Verification code has expired. Use /api/auth/resend-confirmation to get a new one." });
        }
        catch (NotAuthorizedException)
        {
            return BadRequest(new { message = "This email is already verified." });
        }
    }

    [HttpPost("respond-to-challenge")]
    public async Task<ActionResult<AuthModels.LoginResponse>> RespondToChallenge([FromBody] AuthModels.ChallengeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Session))
            return BadRequest(new { message = "Session is required" });

        if (string.IsNullOrWhiteSpace(request.ChallengeName))
            return BadRequest(new { message = "Challenge name is required" });

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "New password is required" });

        try
        {
            var challengeResponses = new Dictionary<string, string>
            {
                { "NEW_PASSWORD", request.NewPassword },
                { "USERNAME", request.Email }
            };

            var secretHash = ComputeSecretHash(request.Email);
            if (secretHash != null)
                challengeResponses["SECRET_HASH"] = secretHash;

            var response = await _cognitoClient.RespondToAuthChallengeAsync(new RespondToAuthChallengeRequest
            {
                ClientId = _clientId,
                ChallengeName = request.ChallengeName,
                Session = request.Session,
                ChallengeResponses = challengeResponses
            });

            // Check if another challenge is required
            if (!string.IsNullOrEmpty(response.ChallengeName))
            {
                return Ok(new AuthModels.ChallengeResponse
                {
                    ChallengeName = response.ChallengeName,
                    Session = response.Session,
                    Message = $"Additional challenge required: {response.ChallengeName}"
                });
            }

            return Ok(new AuthModels.LoginResponse
            {
                AccessToken = response.AuthenticationResult.AccessToken,
                IdToken = response.AuthenticationResult.IdToken,
                RefreshToken = response.AuthenticationResult.RefreshToken,
                ExpiresIn = response.AuthenticationResult.ExpiresIn ?? 3600
            });
        }
        catch (InvalidPasswordException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotAuthorizedException)
        {
            return Unauthorized(new { message = "Session expired. Please login again." });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthModels.RefreshTokenResponse>> Refresh([FromBody] AuthModels.RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest(new { message = "Username is required" });

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { message = "Refresh token is required" });

        try
        {
            var authParameters = new Dictionary<string, string>
            {
                { "REFRESH_TOKEN", request.RefreshToken }
            };

            // SECRET_HASH must be computed using the Cognito username (sub/UUID), not email alias
            var secretHash = ComputeSecretHash(request.Username);
            if (secretHash != null)
                authParameters["SECRET_HASH"] = secretHash;

            var authRequest = new InitiateAuthRequest
            {
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                ClientId = _clientId,
                AuthParameters = authParameters
            };

            var response = await _cognitoClient.InitiateAuthAsync(authRequest);

            return Ok(new AuthModels.RefreshTokenResponse
            {
                AccessToken = response.AuthenticationResult.AccessToken,
                IdToken = response.AuthenticationResult.IdToken,
                ExpiresIn = response.AuthenticationResult.ExpiresIn ?? 3600
            });
        }
        catch (NotAuthorizedException)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }
    }
}
