using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Infrastructure.Services;

public class UserRoleService : IUserRoleService
{
    private readonly IApplicationDbContext _context;

    public UserRoleService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(string cognitoUserId, CancellationToken ct = default)
    {
        return await _context.Set<UserRole>()
            .Where(r => r.CognitoUserId == cognitoUserId)
            .Select(r => r.Role)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await _context.Set<UserRole>()
            .Where(r => r.Email == normalizedEmail)
            .Select(r => r.Role)
            .ToListAsync(ct);
    }

    public async Task SetUserRolesAsync(
        string cognitoUserId,
        string email,
        IEnumerable<string> roles,
        string? createdBy = null,
        CancellationToken ct = default)
    {
        // Remove existing roles
        var existingRoles = await _context.Set<UserRole>()
            .Where(r => r.CognitoUserId == cognitoUserId)
            .ToListAsync(ct);

        foreach (var role in existingRoles)
        {
            _context.Remove(role);
        }

        // Add new roles
        foreach (var role in roles)
        {
            var userRole = UserRole.Create(cognitoUserId, email, role, createdBy);
            _context.Add(userRole);
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task AddRoleAsync(
        string cognitoUserId,
        string email,
        string role,
        string? createdBy = null,
        CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var exists = await _context.Set<UserRole>()
            .AnyAsync(r => r.CognitoUserId == cognitoUserId && r.Role == role, ct);

        if (!exists)
        {
            var userRole = UserRole.Create(cognitoUserId, normalizedEmail, role, createdBy);
            _context.Add(userRole);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveRoleAsync(string cognitoUserId, string role, CancellationToken ct = default)
    {
        var userRole = await _context.Set<UserRole>()
            .FirstOrDefaultAsync(r => r.CognitoUserId == cognitoUserId && r.Role == role, ct);

        if (userRole != null)
        {
            _context.Remove(userRole);
            await _context.SaveChangesAsync(ct);
        }
    }
}
