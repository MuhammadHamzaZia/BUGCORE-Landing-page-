using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Infrastructure.Data;
using BugCore.Web.Filters;

namespace BugCore.Web.Controllers;

[Authorize]
[BugCoreAuthorize(AccessLevel.Administrator, globalOnly: true)]
public class AdminController : Controller
{
    private readonly BugCoreDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(BugCoreDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// BugCoreBT Administration Portal & System Management Overview.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var activeUsers = await _userManager.Users.CountAsync(u => u.Enabled);
        var totalProjects = await _context.Projects.CountAsync();
        var totalIssues = await _context.Issues.CountAsync();
        var openIssues = await _context.Issues.CountAsync(i => i.Status < IssueStatus.Resolved);
        var resolvedIssues = await _context.Issues.CountAsync(i => i.Status >= IssueStatus.Resolved);
        var totalCustomFields = await _context.CustomFields.CountAsync();
        var totalCategories = await _context.Categories.CountAsync();
        var totalVersions = await _context.ProjectVersions.CountAsync();
        var totalNotes = await _context.IssueNotes.CountAsync();

        ViewBag.TotalUsers = totalUsers;
        ViewBag.ActiveUsers = activeUsers;
        ViewBag.TotalProjects = totalProjects;
        ViewBag.TotalIssues = totalIssues;
        ViewBag.OpenIssues = openIssues;
        ViewBag.ResolvedIssues = resolvedIssues;
        ViewBag.TotalCustomFields = totalCustomFields;
        ViewBag.TotalCategories = totalCategories;
        ViewBag.TotalVersions = totalVersions;
        ViewBag.TotalNotes = totalNotes;

        var recentUsers = await _userManager.Users
            .OrderByDescending(u => u.DateCreated)
            .Take(5)
            .ToListAsync();

        var recentProjects = await _context.Projects
            .OrderByDescending(p => p.Id)
            .Take(5)
            .ToListAsync();

        ViewBag.RecentUsers = recentUsers;
        ViewBag.RecentProjects = recentProjects;

        return View();
    }
}
