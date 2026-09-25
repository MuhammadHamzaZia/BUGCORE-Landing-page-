using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Interfaces;

namespace BugCore.Web.Controllers;

[Authorize]
public class ProjectDocController : Controller
{
    private readonly IProjectDocService _docService;
    private readonly IProjectService _projectService;
    private readonly IUserContextService _userContext;

    public ProjectDocController(
        IProjectDocService docService,
        IProjectService projectService,
        IUserContextService userContext)
    {
        _docService = docService;
        _projectService = projectService;
        _userContext = userContext;
    }

    /// <summary>
    /// Project Documents Listing (proj_doc_page.php)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int? projectId)
    {
        var activeProjectId = projectId ?? _userContext.GetActiveProjectId() ?? 0;
        var docs = await _docService.GetDocsForProjectAsync(activeProjectId);
        return View(docs);
    }

    /// <summary>
    /// Update Project Document (proj_doc_update.php)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int fileId, string title, string description, IFormFile? file)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;

        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["Error"] = "Document title cannot be empty.";
            return RedirectToAction(nameof(Index));
        }

        byte[]? fileBytes = null;
        string? fileName = null;
        string? fileType = null;

        if (file != null && file.Length > 0)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
            fileName = file.FileName;
            fileType = file.ContentType;
        }

        await _docService.UpdateDocAsync(fileId, title, description, fileName, fileType, fileBytes, userId);
        TempData["Success"] = $"Project document '{title}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Delete Project Document (proj_doc_delete.php)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int fileId)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        await _docService.DeleteDocAsync(fileId, userId);
        TempData["Success"] = $"Document #{fileId} deleted.";
        return RedirectToAction(nameof(Index));
    }
}
