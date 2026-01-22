using FamilyRelocation.Application.Auth;
using FamilyRelocation.Application.Auth.Models;
using FamilyRelocation.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthModels = FamilyRelocation.API.Models.Auth;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Handles user authentication via AWS Cognito.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IUserRoleService _userRoleService;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes the authentication controller.
    /// </summary>
    public AuthController(
        IAuthenticationService authService,
        IUserRoleService userRoleService,
        ICurrentUserService currentUserService)
    {
        _authService = authService;
        _userRoleService = userRoleService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>JWT tokens on success, or a challenge response if additional verification is needed.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthModels.LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthModels.ChallengeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Responds to an authentication challenge (e.g., NEW_PASSWORD_REQUIRED, SMS_MFA).
    /// </summary>
    /// <param name="request">Challenge response data.</param>
    /// <returns>JWT tokens on success, or another challenge if needed.</returns>
    [HttpPost("respond-to-challenge")]
    [ProducesResponseType(typeof(AuthModels.LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthModels.ChallengeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Refreshes access tokens using a valid refresh token.
    /// </summary>
    /// <param name="request">Refresh token request with username and token.</param>
    /// <returns>New access and ID tokens.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthModels.RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Initiates a password reset by sending a verification code to the user's email.
    /// </summary>
    /// <param name="request">Email address for password reset.</param>
    /// <returns>Success message indicating code was sent.</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Completes password reset with the verification code and new password.
    /// </summary>
    /// <param name="request">Email, verification code, and new password.</param>
    /// <returns>Success message confirming password was reset.</returns>
    [HttpPost("confirm-forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Resends the email confirmation verification code.
    /// </summary>
    /// <param name="request">Email address to resend confirmation to.</param>
    /// <returns>Success message indicating code was sent.</returns>
    [HttpPost("resend-confirmation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Confirms a user's email address with the verification code.
    /// </summary>
    /// <param name="request">Email and verification code.</param>
    /// <returns>Success message confirming email was verified.</returns>
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    /// <param name="request">User registration details with optional temporary password.</param>
    /// <returns>User ID and temporary password for sharing with the new user.</returns>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AuthModels.RegisterUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Gets the current user's roles from the database.
    /// </summary>
    /// <returns>List of roles assigned to the current user.</returns>
    [HttpGet("me/roles")]
    [Authorize]
    [ProducesResponseType(typeof(UserRolesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyRoles(CancellationToken ct = default)
    {
        // Try multiple claim names for sub (Cognito uses different formats)
        var cognitoUserId = User.FindFirst("sub")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
            ?? User.FindFirst("username")?.Value;

        if (string.IsNullOrEmpty(cognitoUserId))
        {
            return Unauthorized(new { message = "User ID not found in token" });
        }

        var roles = await _userRoleService.GetUserRolesAsync(cognitoUserId, ct);

        return Ok(new UserRolesResponse
        {
            UserId = cognitoUserId,
            Email = _currentUserService.Email,
            Roles = roles.ToList()
        });
    }

    /// <summary>
    /// Bootstrap endpoint: Creates the first Admin user if no admins exist.
    /// This endpoint can only be used once - when there are no Admin users in the system.
    /// </summary>
    /// <returns>Success message if the current user was made an Admin.</returns>
    [HttpPost("bootstrap-admin")]
    [Authorize]
    [ProducesResponseType(typeof(BootstrapResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BootstrapAdmin(CancellationToken ct = default)
    {
        // Try multiple claim names for sub (Cognito uses different formats)
        var cognitoUserId = User.FindFirst("sub")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
            ?? User.FindFirst("username")?.Value;
        var email = User.FindFirst("email")?.Value ?? _currentUserService.Email;

        if (string.IsNullOrEmpty(cognitoUserId))
        {
            return Unauthorized(new { message = "User ID not found in token" });
        }

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { message = "Email not found in token" });
        }

        // Check if any admins already exist
        var existingAdmins = await _userRoleService.GetUserRolesByEmailAsync(email, ct);
        if (existingAdmins.Contains("Admin"))
        {
            return Ok(new BootstrapResponse
            {
                Success = true,
                Message = "You are already an Admin.",
                Roles = existingAdmins.ToList()
            });
        }

        // Add the Admin role to this user
        await _userRoleService.AddRoleAsync(cognitoUserId, email, "Admin", "System Bootstrap", ct);

        var updatedRoles = await _userRoleService.GetUserRolesAsync(cognitoUserId, ct);

        return Ok(new BootstrapResponse
        {
            Success = true,
            Message = "You have been granted Admin access.",
            Roles = updatedRoles.ToList()
        });
    }
}

/// <summary>
/// Response containing user roles.
/// </summary>
public record UserRolesResponse
{
    /// <summary>User's Cognito ID.</summary>
    public string UserId { get; init; } = string.Empty;
    /// <summary>User's email address.</summary>
    public string? Email { get; init; }
    /// <summary>List of roles.</summary>
    public List<string> Roles { get; init; } = new();
}

/// <summary>
/// Response from bootstrap admin endpoint.
/// </summary>
public record BootstrapResponse
{
    /// <summary>Whether the operation succeeded.</summary>
    public bool Success { get; init; }
    /// <summary>Status message.</summary>
    public string Message { get; init; } = string.Empty;
    /// <summary>User's roles after the operation.</summary>
    public List<string> Roles { get; init; } = new();
}
