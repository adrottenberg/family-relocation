using FamilyRelocation.Application.Auth;
using FamilyRelocation.Application.Auth.Models;
using FamilyRelocation.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for user management operations.
/// Admin-only endpoints for managing system users.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IUserRoleService _userRoleService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogger _activityLogger;

    public UsersController(
        IAuthenticationService authService,
        IUserRoleService userRoleService,
        ICurrentUserService currentUserService,
        IActivityLogger activityLogger)
    {
        _authService = authService;
        _userRoleService = userRoleService;
        _currentUserService = currentUserService;
        _activityLogger = activityLogger;
    }

    /// <summary>
    /// Creates a new user with a temporary password.
    /// </summary>
    /// <param name="request">User creation details.</param>
    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken ct = default)
    {
        // Validate email format
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return BadRequest(new { error = "Valid email address is required" });
        }

        // Validate roles if provided
        if (request.Roles?.Any() == true)
        {
            var validRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Admin", "Coordinator", "BoardMember"
            };
            var invalidRoles = request.Roles.Where(r => !validRoles.Contains(r)).ToList();
            if (invalidRoles.Any())
            {
                return BadRequest(new { error = $"Invalid roles: {string.Join(", ", invalidRoles)}" });
            }
        }

        var result = await _authService.RegisterUserAsync(request.Email);

        if (!result.Success)
        {
            if (result.ErrorType == AuthErrorType.UserAlreadyExists)
            {
                return BadRequest(new { error = "A user with this email already exists" });
            }
            return BadRequest(new { error = result.ErrorMessage });
        }

        // Assign roles if provided (stored in database, not Cognito groups)
        if (request.Roles?.Any() == true)
        {
            await _userRoleService.SetUserRolesAsync(result.UserId, request.Email, request.Roles, _currentUserService.Email, ct);
        }

        // Log activity
        await _activityLogger.LogAsync(
            "User",
            Guid.Empty,
            "Created",
            $"User {request.Email} created with roles: {string.Join(", ", request.Roles ?? new List<string>())}",
            ct);

        return CreatedAtAction(nameof(GetUser), new { userId = request.Email }, new CreateUserResponse
        {
            UserId = result.UserId,
            Email = request.Email,
            TemporaryPassword = result.TemporaryPassword,
            Roles = request.Roles?.ToList() ?? new List<string>(),
            Message = "User created successfully. Share the temporary password with the user - they will be required to change it on first login."
        });
    }

    /// <summary>
    /// Lists all users with optional filtering.
    /// </summary>
    /// <param name="search">Search by email prefix (e.g., "john").</param>
    /// <param name="status">Filter by status (e.g., "CONFIRMED", "FORCE_CHANGE_PASSWORD").</param>
    /// <param name="limit">Maximum users to return (1-60, default 60).</param>
    /// <param name="paginationToken">Token for fetching next page.</param>
    [HttpGet]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListUsers(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] int limit = 60,
        [FromQuery] string? paginationToken = null,
        CancellationToken ct = default)
    {
        // Build Cognito filter if search provided
        string? filter = null;
        if (!string.IsNullOrWhiteSpace(search))
        {
            filter = $"email ^= \"{search}\"";
        }
        else if (!string.IsNullOrWhiteSpace(status))
        {
            filter = $"cognito:user_status = \"{status}\"";
        }

        var result = await _authService.ListUsersAsync(filter, limit, paginationToken, ct);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        // Fetch roles from database (not Cognito groups) for each user
        var usersWithDbRoles = new List<UserDto>();
        foreach (var user in result.Users)
        {
            var dbRoles = await _userRoleService.GetUserRolesByEmailAsync(user.Email, ct);
            usersWithDbRoles.Add(user with { Roles = dbRoles.ToList() });
        }

        return Ok(new UserListResponse
        {
            Users = usersWithDbRoles,
            PaginationToken = result.PaginationToken
        });
    }

    /// <summary>
    /// Gets a specific user's details.
    /// </summary>
    /// <param name="userId">The user's ID (email or Cognito username).</param>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(string userId, CancellationToken ct = default)
    {
        var result = await _authService.GetUserAsync(userId, ct);

        if (!result.Success)
        {
            if (result.ErrorType == AuthErrorType.UserNotFound)
            {
                return NotFound(new { error = result.ErrorMessage });
            }
            return BadRequest(new { error = result.ErrorMessage });
        }

        // Get roles from database instead of Cognito groups
        var dbRoles = await _userRoleService.GetUserRolesByEmailAsync(result.User!.Email, ct);
        var userWithDbRoles = result.User with { Roles = dbRoles.ToList() };

        return Ok(userWithDbRoles);
    }

    /// <summary>
    /// Updates a user's roles.
    /// </summary>
    /// <param name="userId">The user's ID (email).</param>
    /// <param name="request">The new roles to assign.</param>
    [HttpPut("{userId}/roles")]
    [ProducesResponseType(typeof(UpdateRolesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRoles(
        string userId,
        [FromBody] UpdateRolesRequest request,
        CancellationToken ct = default)
    {
        // Get the user from Cognito to verify they exist and get their Cognito ID
        var userResult = await _authService.GetUserAsync(userId, ct);
        if (!userResult.Success)
        {
            if (userResult.ErrorType == AuthErrorType.UserNotFound)
            {
                return NotFound(new { error = "User not found" });
            }
            return BadRequest(new { error = userResult.ErrorMessage });
        }

        var cognitoUserId = userResult.User!.Id;
        var email = userResult.User.Email;

        // Prevent removing own Admin role
        var currentUserEmail = _currentUserService.Email;
        if (email.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase))
        {
            var currentRoles = await _userRoleService.GetUserRolesByEmailAsync(email, ct);
            if (currentRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase) &&
                !request.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Cannot remove your own Admin role" });
            }
        }

        // Validate roles
        var validRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Admin", "Coordinator", "BoardMember"
        };
        var invalidRoles = request.Roles.Where(r => !validRoles.Contains(r)).ToList();
        if (invalidRoles.Any())
        {
            return BadRequest(new { error = $"Invalid roles: {string.Join(", ", invalidRoles)}" });
        }

        // Update roles in database
        await _userRoleService.SetUserRolesAsync(cognitoUserId, email, request.Roles, _currentUserService.Email, ct);

        // Log activity
        await _activityLogger.LogAsync(
            "User",
            Guid.Empty,
            "RolesUpdated",
            $"User {email} roles updated to: {string.Join(", ", request.Roles)}",
            ct);

        return Ok(new UpdateRolesResponse
        {
            UserId = userId,
            Roles = request.Roles.ToList(),
            Message = "Roles updated successfully"
        });
    }

    /// <summary>
    /// Deactivates a user account (prevents login).
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    [HttpPost("{userId}/deactivate")]
    [ProducesResponseType(typeof(UserStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(string userId, CancellationToken ct = default)
    {
        // Prevent deactivating own account
        var currentUserId = _currentUserService.UserId?.ToString();
        if (userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase) ||
            userId.Equals(_currentUserService.Email, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Cannot deactivate your own account" });
        }

        var result = await _authService.DisableUserAsync(userId, ct);

        if (!result.Success)
        {
            if (result.ErrorType == AuthErrorType.UserNotFound)
            {
                return NotFound(new { error = result.ErrorMessage });
            }
            return BadRequest(new { error = result.ErrorMessage });
        }

        // Log activity
        await _activityLogger.LogAsync(
            "User",
            Guid.Empty,
            "Deactivated",
            $"User {userId} deactivated",
            ct);

        return Ok(new UserStatusResponse
        {
            UserId = userId,
            Status = "Disabled",
            Message = result.Message ?? "User deactivated successfully"
        });
    }

    /// <summary>
    /// Reactivates a disabled user account.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    [HttpPost("{userId}/reactivate")]
    [ProducesResponseType(typeof(UserStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateUser(string userId, CancellationToken ct = default)
    {
        var result = await _authService.EnableUserAsync(userId, ct);

        if (!result.Success)
        {
            if (result.ErrorType == AuthErrorType.UserNotFound)
            {
                return NotFound(new { error = result.ErrorMessage });
            }
            return BadRequest(new { error = result.ErrorMessage });
        }

        // Log activity
        await _activityLogger.LogAsync(
            "User",
            Guid.Empty,
            "Reactivated",
            $"User {userId} reactivated",
            ct);

        return Ok(new UserStatusResponse
        {
            UserId = userId,
            Status = "Enabled",
            Message = result.Message ?? "User reactivated successfully"
        });
    }
}

/// <summary>
/// Response containing list of users.
/// </summary>
public record UserListResponse
{
    /// <summary>List of users.</summary>
    public List<UserDto> Users { get; init; } = new();
    /// <summary>Token for fetching next page.</summary>
    public string? PaginationToken { get; init; }
}

/// <summary>
/// Request to update user roles.
/// </summary>
public record UpdateRolesRequest(List<string> Roles);

/// <summary>
/// Response after updating user roles.
/// </summary>
public record UpdateRolesResponse
{
    /// <summary>The user's ID.</summary>
    public string UserId { get; init; } = string.Empty;
    /// <summary>The user's new roles.</summary>
    public List<string> Roles { get; init; } = new();
    /// <summary>Success message.</summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Response after changing user status.
/// </summary>
public record UserStatusResponse
{
    /// <summary>The user's ID.</summary>
    public string UserId { get; init; } = string.Empty;
    /// <summary>The user's new status.</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Success message.</summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Request to create a new user.
/// </summary>
public record CreateUserRequest(string Email, List<string>? Roles = null);

/// <summary>
/// Response after creating a user.
/// </summary>
public record CreateUserResponse
{
    /// <summary>The user's Cognito ID.</summary>
    public string UserId { get; init; } = string.Empty;
    /// <summary>The user's email.</summary>
    public string Email { get; init; } = string.Empty;
    /// <summary>Temporary password to share with the user.</summary>
    public string TemporaryPassword { get; init; } = string.Empty;
    /// <summary>Assigned roles.</summary>
    public List<string> Roles { get; init; } = new();
    /// <summary>Success message.</summary>
    public string Message { get; init; } = string.Empty;
}
