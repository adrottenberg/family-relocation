namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Represents a user role assignment stored in the database.
/// Used to manage user roles independently of Cognito groups.
/// </summary>
public class UserRole
{
    public Guid Id { get; private set; }
    public string CognitoUserId { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Role { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    private UserRole() { }

    public static UserRole Create(
        string cognitoUserId,
        string email,
        string role,
        string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(cognitoUserId))
            throw new ArgumentException("Cognito user ID is required", nameof(cognitoUserId));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role is required", nameof(role));

        return new UserRole
        {
            Id = Guid.NewGuid(),
            CognitoUserId = cognitoUserId,
            Email = email.ToLowerInvariant(),
            Role = role,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
}
