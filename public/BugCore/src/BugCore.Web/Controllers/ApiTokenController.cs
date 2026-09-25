using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Interfaces;

namespace BugCore.Web.Controllers;

[Authorize]
public class ApiTokenController : Controller
{
    private readonly IApiTokenService _apiTokenService;
    private readonly IUserContextService _userContext;
    private readonly IProjectService _projectService;

    public ApiTokenController(
        IApiTokenService apiTokenService, 
        IUserContextService userContext,
        IProjectService projectService)
    {
        _apiTokenService = apiTokenService;
        _userContext = userContext;
        _projectService = projectService;
    }

    /// <summary>
    /// API Tokens Page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userContext.GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var tokens = await _apiTokenService.GetTokensForUserAsync(userId.Value);
        var projects = await _projectService.GetAllProjectsAsync();

        ViewBag.Projects = projects;
        return View(tokens);
    }

    /// <summary>
    /// Create Personal Access Token (PAT)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string scope, int expiryDays, int? projectId)
    {
        var userId = _userContext.GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _apiTokenService.CreateTokenAsync(userId.Value, name, scope, expiryDays, projectId);

        TempData["NewRawToken"] = result.RawToken;
        TempData["NewTokenName"] = result.Token.Name;
        TempData["NewTokenExpiry"] = result.Token.ExpiresAt.ToString("yyyy-MM-dd HH:mm UTC");
        TempData["Success"] = $"Personal Access Token '{result.Token.Name}' generated successfully!";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Revoke API Token
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(int tokenId)
    {
        var userId = _userContext.GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await _apiTokenService.RevokeTokenAsync(tokenId, userId.Value);
        TempData["Success"] = $"API token #{tokenId} successfully revoked.";
        return RedirectToAction(nameof(Index));
    }
}
