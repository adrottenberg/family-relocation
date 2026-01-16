namespace FamilyRelocation.Application.Common.Interfaces;

public interface ICurrentUserService
{
    /// <summary>
    /// The current user's ID, or null if not authenticated.
    /// Consumers should decide how to handle null (e.g., use a well-known ID for anonymous operations).
    /// </summary>
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
