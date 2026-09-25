using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Interfaces;

namespace BugCore.Web.Controllers;

[Authorize]
public class TagController : Controller
{
    private readonly ITagService _tagService;
    private readonly IUserContextService _userContext;

    public TagController(ITagService tagService, IUserContextService userContext)
    {
        _tagService = tagService;
        _userContext = userContext;
    }

    /// <summary>
    /// View Tag Details and Related Issues (tag_view_page.php)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int tagId)
    {
        var tag = await _tagService.GetTagByIdAsync(tagId);
        ViewBag.TagId = tagId;
        return View(tag);
    }

    /// <summary>
    /// Create new Tag (tag_create.php)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string tagName, string description)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        if (string.IsNullOrWhiteSpace(tagName))
        {
            TempData["Error"] = "Tag name cannot be empty.";
            return RedirectToAction(nameof(Details), new { tagId = 0 });
        }

        var created = await _tagService.CreateTagAsync(tagName, description ?? string.Empty, userId);
        TempData["Success"] = $"Tag '{created.Name}' created successfully.";
        return RedirectToAction("Index", "Issue");
    }

    /// <summary>
    /// Attach Tag to Issue (tag_attach.php)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Attach(int issueId, string tagString)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        await _tagService.AttachTagsToIssueAsync(issueId, tagString, userId);
        TempData["Success"] = $"Tags attached to Issue #{issueId:D7}.";
        return RedirectToAction("Details", "Issue", new { id = issueId });
    }

    /// <summary>
    /// Detach Tag from Issue (tag_detach.php)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Detach(int issueId, int tagId)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        await _tagService.DetachTagFromIssueAsync(issueId, tagId, userId);
        TempData["Success"] = $"Tag detached from Issue #{issueId:D7}.";
        return RedirectToAction("Details", "Issue", new { id = issueId });
    }

    /// <summary>
    /// Delete Tag (tag_delete.php)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int tagId)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        await _tagService.DeleteTagAsync(tagId, userId);
        TempData["Success"] = $"Tag #{tagId} deleted.";
        return RedirectToAction("Index", "Issue");
    }
}
