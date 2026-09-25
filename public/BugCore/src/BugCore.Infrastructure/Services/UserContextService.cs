using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;

namespace BugCore.Infrastructure.Services;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAccessControlService _accessControl;

    private const string ProjectCookieKey = "BugCore_Current_Project_Id";

    public UserContextService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        IAccessControlService accessControl)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _accessControl = accessControl;
    }

    public int? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            return null;
        }

        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim != null && int.TryParse(idClaim.Value, out int id))
        {
            return id;
        }

        return null;
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null) return null;
        return await _userManager.GetUserAsync(principal);
    }

    public int? GetActiveProjectId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        if (context.Request.Cookies.TryGetValue(ProjectCookieKey, out string? val) &&
            int.TryParse(val, out int projId) && projId > 0)
        {
            return projId;
        }

        return null;
    }

    public void SetActiveProjectId(int? projectId)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        if (projectId.HasValue && projectId.Value > 0)
        {
            context.Response.Cookies.Append(ProjectCookieKey, projectId.Value.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            });
        }
        else
        {
            context.Response.Cookies.Delete(ProjectCookieKey);
        }
    }

    public async Task<AccessLevel> GetActiveProjectAccessLevelAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return AccessLevel.Viewer;

        var projectId = GetActiveProjectId();
        return await _accessControl.GetEffectiveAccessLevelAsync(userId.Value, projectId);
    }
}
