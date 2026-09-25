using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class ApiTokenService : IApiTokenService
{
    private readonly BugCoreDbContext _context;

    public ApiTokenService(BugCoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ApiToken>> GetTokensForUserAsync(int userId)
    {
        return await _context.ApiTokens
            .Include(t => t.Project)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<ApiToken?> GetTokenByIdAsync(int tokenId)
    {
        return await _context.ApiTokens
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == tokenId);
    }

    public async Task<CreateApiTokenResult> CreateTokenAsync(
        int userId, 
        string tokenName, 
        string scope = "full_access", 
        int expiryDays = 90, 
        int? projectId = null)
    {
        // 1. Generate secure raw bearer token (e.g. pat_bugcore_xxxxxxxxxxxxxxxx)
        string rawToken = $"pat_bugcore_{Guid.NewGuid():N}{Guid.NewGuid():N}";
        string tokenHash = HashToken(rawToken);

        // Clamp expiry to valid range (max 365 days)
        int clampedDays = expiryDays switch
        {
            30 => 30,
            90 => 90,
            180 => 180,
            365 => 365,
            _ => 90
        };

        var token = new ApiToken
        {
            UserId = userId,
            Name = string.IsNullOrWhiteSpace(tokenName) ? "Default API Token" : tokenName.Trim(),
            Hash = tokenHash,
            Scope = scope,
            ProjectId = projectId,
            DateCreated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(clampedDays),
            IsRevoked = false
        };

        _context.ApiTokens.Add(token);
        await _context.SaveChangesAsync();

        return new CreateApiTokenResult
        {
            Token = token,
            RawToken = rawToken
        };
    }

    public async Task RevokeTokenAsync(int tokenId, int userId)
    {
        var token = await _context.ApiTokens.FirstOrDefaultAsync(t => t.Id == tokenId && t.UserId == userId);
        if (token != null)
        {
            token.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(bool IsValid, ApiToken? Token, ApplicationUser? User, string Error)> ValidateTokenAsync(string rawToken, string requestIp)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return (false, null, null, "Token missing.");

        string tokenHash = HashToken(rawToken);

        var token = await _context.ApiTokens
            .Include(t => t.User)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Hash == tokenHash);

        if (token == null)
            return (false, null, null, "Invalid API token credential.");

        if (token.IsRevoked)
            return (false, null, null, "This API token has been revoked.");

        if (DateTime.UtcNow > token.ExpiresAt)
            return (false, null, null, $"This API token expired on {token.ExpiresAt:yyyy-MM-dd HH:mm} UTC.");

        // Update Usage Telemetry
        token.DateUsed = DateTime.UtcNow;
        token.LastUsedIp = string.IsNullOrWhiteSpace(requestIp) ? "127.0.0.1" : requestIp;
        await _context.SaveChangesAsync();

        return (true, token, token.User, string.Empty);
    }

    private static string HashToken(string rawToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
