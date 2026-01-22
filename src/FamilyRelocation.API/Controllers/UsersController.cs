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
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogger _activityLogger;

    public UsersController(
        IAuthenticationService authService,
        ICurrentUserService currentUserService,
        IActivityLogger activityLogger)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _activityLogger = activityLogger;
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

        return Ok(new UserListResponse
        {
            Users = result.Users,
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

        return Ok(result.User);
    }

    /// <summary>
    /// Updates a user's roles.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
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
        // Prevent removing own Admin role
        var currentUserId = _currentUserService.UserId?.ToString();
        if (userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase) ||
            userId.Equals(_currentUserService.Email, StringComparison.OrdinalIgnoreCase))
        {
            var currentRoles = await _authService.GetUserGroupsAsync(userId, ct);
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

        var result = await _authService.UpdateUserRolesAsync(userId, request.Roles, ct);

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
            Guid.Empty, // We don't have a GUID for users
            "RolesUpdated",
            $"User {userId} roles updated to: {string.Join(", ", request.Roles)}",
            ct);

        return Ok(new UpdateRolesResponse
        {
            UserId = userId,
            Roles = request.Roles.ToList(),
            Message = result.Message ?? "Roles updated successfully"
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
