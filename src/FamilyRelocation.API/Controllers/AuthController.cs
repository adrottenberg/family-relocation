using FamilyRelocation.Application.Auth;
using FamilyRelocation.Application.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthModels = FamilyRelocation.API.Models.Auth;

namespace FamilyRelocation.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthModels.LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Password is required" });

        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (result.Challenge != null)
        {
            return Ok(new AuthModels.ChallengeResponse
            {
                ChallengeName = result.Challenge.ChallengeName,
                Session = result.Challenge.Session,
                Message = result.Challenge.Message,
                RequiredFields = result.Challenge.RequiredFields
            });
        }

        if (!result.Success)
        {
            return result.ErrorType switch
            {
                AuthErrorType.InvalidCredentials => Unauthorized(new { message = result.ErrorMessage }),
                AuthErrorType.UserNotConfirmed => BadRequest(new { message = result.ErrorMessage }),
                AuthErrorType.PasswordResetRequired => BadRequest(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(new AuthModels.LoginResponse
        {
            AccessToken = result.Tokens.AccessToken,
            IdToken = result.Tokens.IdToken,
            RefreshToken = result.Tokens.RefreshToken,
            ExpiresIn = result.Tokens.ExpiresIn
        });
    }

    [HttpPost("respond-to-challenge")]
    public async Task<IActionResult> RespondToChallenge([FromBody] AuthModels.ChallengeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        if (string.IsNullOrWhiteSpace(request.Session))
            return BadRequest(new { message = "Session is required" });

        if (string.IsNullOrWhiteSpace(request.ChallengeName))
            return BadRequest(new { message = "Challenge name is required" });

        if (request.Responses == null || request.Responses.Count == 0)
            return BadRequest(new { message = "Challenge responses are required" });

        var result = await _authService.RespondToChallengeAsync(new ChallengeResponseRequest
        {
            Email = request.Email,
            ChallengeName = request.ChallengeName,
            Session = request.Session,
            Responses = request.Responses
        });

        if (result.Challenge != null)
        {
            return Ok(new AuthModels.ChallengeResponse
            {
                ChallengeName = result.Challenge.ChallengeName,
                Session = result.Challenge.Session,
                Message = result.Challenge.Message,
                RequiredFields = result.Challenge.RequiredFields
            });
        }

        if (!result.Success)
        {
            return result.ErrorType switch
            {
                AuthErrorType.InvalidPassword => BadRequest(new { message = result.ErrorMessage }),
                AuthErrorType.InvalidSession => Unauthorized(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(new AuthModels.LoginResponse
        {
            AccessToken = result.Tokens.AccessToken,
            IdToken = result.Tokens.IdToken,
            RefreshToken = result.Tokens.RefreshToken,
            ExpiresIn = result.Tokens.ExpiresIn
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] AuthModels.RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest(new { message = "Username is required" });

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { message = "Refresh token is required" });

        var result = await _authService.RefreshTokensAsync(request.Username, request.RefreshToken);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        return Ok(new AuthModels.RefreshTokenResponse
        {
            AccessToken = result.Tokens.AccessToken,
            IdToken = result.Tokens.IdToken,
            ExpiresIn = result.Tokens.ExpiresIn
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] AuthModels.ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        var result = await _authService.RequestPasswordResetAsync(request.Email);

        if (!result.Success)
        {
            return result.ErrorType switch
            {
                AuthErrorType.TooManyRequests => BadRequest(new { message = result.ErrorMessage }),
                _ => Ok(new { message = result.Message ?? result.ErrorMessage })
            };
        }

        return Ok(new { message = result.Message });
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

        var result = await _authService.ConfirmPasswordResetAsync(request.Email, request.Code, request.NewPassword);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = result.Message });
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] AuthModels.ResendConfirmationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        var result = await _authService.ResendConfirmationCodeAsync(request.Email);

        if (!result.Success)
        {
            return result.ErrorType switch
            {
                AuthErrorType.TooManyRequests => BadRequest(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(new { message = result.Message });
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] AuthModels.ConfirmEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { message = "Verification code is required" });

        var result = await _authService.ConfirmEmailAsync(request.Email, request.Code);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Admin-only: Register a new user with a temporary password.
    /// The user will be required to change their password on first login.
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterUser([FromBody] AuthModels.RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        var result = await _authService.RegisterUserAsync(request.Email, request.TemporaryPassword);

        if (!result.Success)
        {
            return result.ErrorType switch
            {
                AuthErrorType.UserAlreadyExists => Conflict(new { message = result.ErrorMessage }),
                AuthErrorType.InvalidPassword => BadRequest(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(new AuthModels.RegisterUserResponse
        {
            UserId = result.UserId,
            TemporaryPassword = result.TemporaryPassword,
            Message = result.Message
        });
    }
}
