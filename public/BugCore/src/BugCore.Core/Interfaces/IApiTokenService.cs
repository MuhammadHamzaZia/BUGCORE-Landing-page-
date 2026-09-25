using BugCore.Core.Entities;

namespace BugCore.Core.Interfaces;

public class CreateApiTokenResult
{
    public ApiToken Token { get; set; } = null!;
    public string RawToken { get; set; } = string.Empty;
}

public interface IApiTokenService
{
    Task<IEnumerable<ApiToken>> GetTokensForUserAsync(int userId);
    Task<ApiToken?> GetTokenByIdAsync(int tokenId);
    Task<CreateApiTokenResult> CreateTokenAsync(int userId, string tokenName, string scope = "full_access", int expiryDays = 90, int? projectId = null);
    Task RevokeTokenAsync(int tokenId, int userId);
    Task<(bool IsValid, ApiToken? Token, ApplicationUser? User, string Error)> ValidateTokenAsync(string rawToken, string requestIp);
}
