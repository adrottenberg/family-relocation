using System.Security.Claims;
using FamilyRelocation.Application.Common.Interfaces;

namespace FamilyRelocation.API.Services;

/// <summary>
/// Implementation of ICurrentUserService that extracts user information from HTTP context claims.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes the service with the HTTP context accessor.
    /// </summary>
    /// <param name="httpContextAccessor">HTTP context accessor for accessing request claims.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

            return Guid.TryParse(sub, out var userId) ? userId : null;
        }
    }

    /// <inheritdoc />
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("email");

    /// <inheritdoc />
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
