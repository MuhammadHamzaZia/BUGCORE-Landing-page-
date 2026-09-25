using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Web.ViewModels.Issues;

namespace BugCore.Web.Controllers;

[Authorize]
public class IssueController : Controller
{
    private readonly IIssueService _issueService;
    private readonly IProjectService _projectService;
    private readonly ICategoryService _categoryService;
    private readonly IVersionService _versionService;
    private readonly ICustomFieldService _customFieldService;
    private readonly IUserContextService _userContext;
    private readonly IAccessControlService _accessControl;
    private readonly UserManager<ApplicationUser> _userManager;

    public IssueController(
        IIssueService issueService,
        IProjectService projectService,
        ICategoryService categoryService,
        IVersionService versionService,
        ICustomFieldService customFieldService,
        IUserContextService userContext,
        IAccessControlService accessControl,
        UserManager<ApplicationUser> userManager)
    {
        _issueService = issueService;
        _projectService = projectService;
        _categoryService = categoryService;
        _versionService = versionService;
        _customFieldService = customFieldService;
        _userContext = userContext;
        _accessControl = accessControl;
        _userManager = userManager;
    }

    /// <summary>
    /// View All Issues Grid with Multi-Column Filtering and Pagination (view_all_bug_page.php).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(
        int? projectId,
        int? categoryId,
        IssueStatus? status,
        IssuePriority? priority,
        IssueSeverity? severity,
        ResolutionType? resolution,
        int? handlerId,
        int? reporterId,
        ProjectViewState? viewState,
        string? search,
        string sortField = "LastUpdated",
        string sortDirection = "DESC",
        int page = 1,
        int pageSize = 50)
    {
        var activeProjectId = projectId ?? _userContext.GetActiveProjectId();
        var userId = _userContext.GetCurrentUserId() ?? 0;

        var filter = new IssueFilterCriteria
        {
            ProjectId = activeProjectId,
            CategoryId = categoryId,
            Status = status,
            Priority = priority,
            Severity = severity,
            Resolution = resolution,
            HandlerId = handlerId,
            ReporterId = reporterId,
            ViewState = viewState,
            Search = search,
            SortField = sortField,
            SortDirection = sortDirection,
            Page = page,
            PageSize = pageSize
        };

        var issues = await _issueService.GetIssuesAsync(filter);
        var total = await _issueService.GetIssueCountAsync(filter);

        var accessibleProjects = await _projectService.GetAccessibleProjectsAsync(userId);
        var categories = activeProjectId.HasValue && activeProjectId.Value > 0
            ? await _categoryService.GetCategoriesForProjectAsync(activeProjectId.Value)
            : await _categoryService.GetGlobalCategoriesAsync();

        var users = await _userManager.Users.Where(u => u.Enabled).OrderBy(u => u.UserName).ToListAsync();

        var viewModel = new IssueListViewModel
        {
            Issues = issues,
            TotalCount = total,
            CurrentPage = page,
            PageSize = pageSize,
            ProjectId = activeProjectId,
            CategoryId = categoryId,
            Status = status,
            Priority = priority,
            Severity = severity,
            Resolution = resolution,
            HandlerId = handlerId,
            ReporterId = reporterId,
            ViewState = viewState,
            SearchQuery = search,
            SortField = sortField,
            SortDirection = sortDirection,
            Projects = accessibleProjects,
            Categories = categories,
            Users = users
        };

        return View(viewModel);
    }

    /// <summary>
    /// Issue Details View (bug_view_page.php) with Notes, Monitors, Relationships, Attachments, and History.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var issue = await _issueService.GetIssueByIdAsync(id, includeDetails: true);
        if (issue == null)
        {
            return NotFound();
        }

        var userId = _userContext.GetCurrentUserId() ?? 0;
        var userLevel = await _accessControl.GetEffectiveAccessLevelAsync(userId, issue.ProjectId);

        var accessibleProjects = await _projectService.GetAccessibleProjectsAsync(userId);
        var categories = await _categoryService.GetCategoriesForProjectAsync(issue.ProjectId);
        var versions = await _versionService.GetVersionsForProjectAsync(issue.ProjectId);
        var linkedCustomFields = await _customFieldService.GetLinkedFieldsForProjectAsync(issue.ProjectId);
        var users = await _userManager.Users.Where(u => u.Enabled).OrderBy(u => u.UserName).ToListAsync();

        var viewModel = new IssueDetailsViewModel
        {
            Issue = issue,
            CanUpdate = await _accessControl.CanUpdateIssueAsync(userId, id),
            CanDelete = await _accessControl.CanDeleteIssueAsync(userId, id),
            CanAddNote = await _accessControl.CanAddNoteAsync(userId, id),
            CanAttachFile = userLevel >= AccessLevel.Reporter,
            CanChangeStatus = await _accessControl.CanChangeStatusAsync(userId, id, IssueStatus.Assigned),
            CanAssign = userLevel >= AccessLevel.Updater,
            IsMonitored = await _issueService.IsMonitoredByUserAsync(id, userId),
            UserAccessLevel = userLevel,
            AssignableUsers = users,
            Categories = categories,
            Versions = versions,
            AvailableProjects = accessibleProjects,
            LinkedCustomFields = linkedCustomFields
        };

        return View(viewModel);
    }

    /// <summary>
    /// Edit Issue Details Form (bug_update_page.php).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var issue = await _issueService.GetIssueByIdAsync(id, includeDetails: true);
        if (issue == null) return NotFound();

        var userId = _userContext.GetCurrentUserId() ?? 0;
        if (!await _accessControl.CanUpdateIssueAsync(userId, id))
        {
            return Forbid();
        }

        var categories = await _categoryService.GetCategoriesForProjectAsync(issue.ProjectId);
        var versions = await _versionService.GetVersionsForProjectAsync(issue.ProjectId);
        var linkedFields = await _customFieldService.GetLinkedFieldsForProjectAsync(issue.ProjectId);
        var users = await _userManager.Users.Where(u => u.Enabled).OrderBy(u => u.UserName).ToListAsync();

        var customDict = issue.CustomValues.ToDictionary(cv => cv.CustomFieldId, cv => cv.Value);

        var viewModel = new EditIssueViewModel
        {
            Id = issue.Id,
            ProjectId = issue.ProjectId,
            ProjectName = issue.Project?.Name ?? "Project",
            CategoryId = issue.CategoryId,
            Summary = issue.Summary,
            Description = issue.Description,
            StepsToReproduce = issue.StepsToReproduce,
            AdditionalInformation = issue.AdditionalInformation,
            Severity = issue.Severity,
            Priority = issue.Priority,
            Reproducibility = issue.Reproducibility,
            Status = issue.Status,
            Resolution = issue.Resolution,
            ViewState = issue.ViewState,
            TargetVersion = issue.TargetVersion,
            FixedInVersion = issue.FixedInVersion,
            Build = issue.Build,
            HandlerId = issue.HandlerId,
            Categories = categories,
            Versions = versions,
            Users = users,
            LinkedCustomFields = linkedFields,
            CustomFields = customDict
        };

        return View(viewModel);
    }

    /// <summary>
    /// Save Edited Issue Details (bug_update.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditIssueViewModel model)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        if (!await _accessControl.CanUpdateIssueAsync(userId, model.Id))
        {
            return Forbid();
        }

        var issue = await _issueService.GetIssueByIdAsync(model.Id, includeDetails: false);
        if (issue == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.Categories = await _categoryService.GetCategoriesForProjectAsync(issue.ProjectId);
            model.Versions = await _versionService.GetVersionsForProjectAsync(issue.ProjectId);
            model.LinkedCustomFields = await _customFieldService.GetLinkedFieldsForProjectAsync(issue.ProjectId);
            model.Users = await _userManager.Users.Where(u => u.Enabled).OrderBy(u => u.UserName).ToListAsync();
            return View(model);
        }

        issue.CategoryId = model.CategoryId;
        issue.Summary = model.Summary;
        issue.Description = model.Description;
        issue.StepsToReproduce = model.StepsToReproduce ?? string.Empty;
        issue.AdditionalInformation = model.AdditionalInformation ?? string.Empty;
        issue.Severity = model.Severity;
        issue.Priority = model.Priority;
        issue.Reproducibility = model.Reproducibility;
        issue.Status = model.Status;
        issue.Resolution = model.Resolution;
        issue.ViewState = model.ViewState;
        issue.TargetVersion = model.TargetVersion ?? string.Empty;
        issue.FixedInVersion = model.FixedInVersion ?? string.Empty;
        issue.Build = model.Build ?? string.Empty;
        issue.HandlerId = model.HandlerId;

        await _issueService.UpdateIssueAsync(issue, userId);

        TempData["Success"] = $"Issue #{issue.Id:D7} updated successfully.";
        return RedirectToAction(nameof(Details), new { id = issue.Id });
    }

    /// <summary>
    /// Report an Issue Form (bug_report_page.php).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create(int? projectId = null)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        var projects = (await _projectService.GetAccessibleProjectsAsync(userId)).ToList();

        var selectedProjectId = projectId ?? _userContext.GetActiveProjectId() ?? projects.FirstOrDefault()?.Id ?? 0;

        var categories = selectedProjectId > 0
            ? await _categoryService.GetCategoriesForProjectAsync(selectedProjectId)
            : Enumerable.Empty<Category>();

        var versions = selectedProjectId > 0
            ? await _versionService.GetVersionsForProjectAsync(selectedProjectId)
            : Enumerable.Empty<ProjectVersion>();

        var linkedFields = selectedProjectId > 0
            ? await _customFieldService.GetLinkedFieldsForProjectAsync(selectedProjectId)
            : Enumerable.Empty<CustomFieldProject>();

        var users = await _userManager.Users.Where(u => u.Enabled).OrderBy(u => u.UserName).ToListAsync();

        var viewModel = new CreateIssueViewModel
        {
            ProjectId = selectedProjectId,
            Projects = projects,
            Categories = categories,
            Versions = versions,
            LinkedCustomFields = linkedFields,
            Users = users
        };

        return View(viewModel);
    }

    /// <summary>
    /// Handle Issue Submission (bug_report.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateIssueViewModel model)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;

        if (!await _accessControl.CanReportIssueAsync(userId, model.ProjectId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            model.Projects = await _projectService.GetAccessibleProjectsAsync(userId);
            model.Categories = await _categoryService.GetCategoriesForProjectAsync(model.ProjectId);
            model.Versions = await _versionService.GetVersionsForProjectAsync(model.ProjectId);
            model.LinkedCustomFields = await _customFieldService.GetLinkedFieldsForProjectAsync(model.ProjectId);
            model.Users = await _userManager.Users.Where(u => u.Enabled).OrderBy(u => u.UserName).ToListAsync();
            return View(model);
        }

        var issue = new Issue
        {
            ProjectId = model.ProjectId,
            CategoryId = model.CategoryId,
            Summary = model.Summary,
            Description = model.Description,
            StepsToReproduce = model.StepsToReproduce ?? string.Empty,
            AdditionalInformation = model.AdditionalInformation ?? string.Empty,
            Severity = model.Severity,
            Priority = model.Priority,
            Reproducibility = model.Reproducibility,
            Status = model.HandlerId.HasValue ? IssueStatus.Assigned : IssueStatus.New,
            Resolution = ResolutionType.Open,
            ViewState = model.ViewState,
            TargetVersion = model.TargetVersion ?? string.Empty,
            FixedInVersion = string.Empty,
            Build = model.Build ?? string.Empty,
            HandlerId = model.HandlerId
        };

        var created = await _issueService.CreateIssueAsync(issue, userId, model.CustomFields);

        // Upload attachment if present
        if (model.Attachment != null && model.Attachment.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await model.Attachment.CopyToAsync(memoryStream);
            await _issueService.AddAttachmentAsync(
                created.Id,
                model.Attachment.FileName,
                model.Attachment.ContentType,
                memoryStream.ToArray(),
                model.AttachmentDescription ?? string.Empty,
                userId);
        }

        TempData["Success"] = $"Issue #{created.Id:D7} created successfully.";
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    /// <summary>
    /// Update Issue Status and Resolution (bug_change_status_page.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, IssueStatus status, ResolutionType? resolution, string? note)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        if (!await _accessControl.CanChangeStatusAsync(userId, id, status))
        {
            return Forbid();
        }

        await _issueService.ChangeStatusAsync(id, status, resolution, note, userId);
        TempData["Success"] = $"Issue #{id:D7} status updated to {status}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Reassign Issue Handler (bug_assign.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int id, int? handlerId)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        var issue = await _issueService.GetIssueByIdAsync(id, includeDetails: false);
        if (issue == null) return NotFound();

        var userLevel = await _accessControl.GetEffectiveAccessLevelAsync(userId, issue.ProjectId);
        if (userLevel < AccessLevel.Updater)
        {
            return Forbid();
        }

        await _issueService.AssignHandlerAsync(id, handlerId, userId);
        TempData["Success"] = "Issue assignee updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Add Bug Note (bugnote_add.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNote(int id, string noteText, bool isPrivate = false, int timeTracking = 0)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        if (!await _accessControl.CanAddNoteAsync(userId, id))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(noteText))
        {
            TempData["Error"] = "Note content cannot be empty.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await _issueService.AddNoteAsync(id, noteText, isPrivate, timeTracking, userId);
        TempData["Success"] = "Bug note recorded successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Delete Bug Note (bugnote_delete.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNote(int id, int noteId)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        var issue = await _issueService.GetIssueByIdAsync(id, includeDetails: false);
        if (issue == null) return NotFound();

        var userLevel = await _accessControl.GetEffectiveAccessLevelAsync(userId, issue.ProjectId);
        if (userLevel < AccessLevel.Developer)
        {
            return Forbid();
        }

        await _issueService.DeleteNoteAsync(noteId, userId);
        TempData["Success"] = "Bug note removed.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Toggle Monitor for Current User (bug_monitor_add.php / delete).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleMonitor(int id)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        var isMonitored = await _issueService.ToggleMonitorAsync(id, userId);

        TempData["Success"] = isMonitored ? "Issue added to your monitored list." : "Issue removed from monitored list.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Stick / Unstick Issue (bug_stick.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSticky(int id, bool sticky)
    {
        var currentUserId = _userContext.GetCurrentUserId() ?? 0;
        var issue = await _issueService.GetIssueByIdAsync(id, includeDetails: false);
        if (issue == null) return NotFound();

        var userLevel = await _accessControl.GetEffectiveAccessLevelAsync(currentUserId, issue.ProjectId);
        if (userLevel < AccessLevel.Modifier)
        {
            return Forbid();
        }

        await _issueService.ToggleStickyAsync(id, sticky, currentUserId);
        TempData["Success"] = sticky ? "Issue marked as sticky." : "Issue un-stickied.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Add Another User to Monitor Issue (bug_monitor_add.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMonitorUser(int id, int monitorUserId)
    {
        var currentUserId = _userContext.GetCurrentUserId() ?? 0;
        var issue = await _issueService.GetIssueByIdAsync(id, includeDetails: false);
        if (issue == null) return NotFound();

        var userLevel = await _accessControl.GetEffectiveAccessLevelAsync(currentUserId, issue.ProjectId);
        if (userLevel < AccessLevel.Developer)
        {
            return Forbid();
        }

        await _issueService.AddMonitorUserAsync(id, monitorUserId);
        TempData["Success"] = "User added as issue monitor.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Remove User from Monitoring Issue (bug_monitor_delete.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMonitorUser(int id, int monitorUserId)
    {
        var currentUserId = _userContext.GetCurrentUserId() ?? 0;
        var issue = await _issueService.GetIssueByIdAsync(id, includeDetails: false);
        if (issue == null) return NotFound();

        var userLevel = await _accessControl.GetEffectiveAccessLevelAsync(currentUserId, issue.ProjectId);
        if (userLevel < AccessLevel.Developer && currentUserId != monitorUserId)
        {
            return Forbid();
        }

        await _issueService.RemoveMonitorUserAsync(id, monitorUserId);
        TempData["Success"] = "User removed from issue monitors.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Add Issue Relationship (bug_relationship_add.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRelationship(int id, int destIssueId, RelationshipType relType)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        if (!await _accessControl.CanUpdateIssueAsync(userId, id))
        {
            return Forbid();
        }

        if (id == destIssueId)
        {
            TempData["Error"] = "Cannot create a relationship between an issue and itself.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var targetIssue = await _issueService.GetIssueByIdAsync(destIssueId, includeDetails: false);
        if (targetIssue == null)
        {
            TempData["Error"] = $"Target issue #{destIssueId:D7} does not exist.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var added = await _issueService.AddRelationshipAsync(id, destIssueId, relType, userId);
        if (added)
        {
            TempData["Success"] = $"Relationship with issue #{destIssueId:D7} added.";
        }
        else
        {
            TempData["Error"] = "Relationship already exists between these issues.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Delete Issue Relationship (bug_relationship_delete.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRelationship(int id, int relationshipId)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        if (!await _accessControl.CanUpdateIssueAsync(userId, id))
        {
            return Forbid();
        }

        await _issueService.DeleteRelationshipAsync(relationshipId, userId);
        TempData["Success"] = "Relationship deleted.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Upload File Attachment (bug_file_add.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(int id, IFormFile file, string? description)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        var issue = await _issueService.GetIssueByIdAsync(id, includeDetails: false);
        if (issue == null) return NotFound();

        var userLevel = await _accessControl.GetEffectiveAccessLevelAsync(userId, issue.ProjectId);
        if (userLevel < AccessLevel.Reporter)
        {
            return Forbid();
        }

        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a valid file to upload.";
            return RedirectToAction(nameof(Details), new { id });
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        await _issueService.AddAttachmentAsync(
            id,
            file.FileName,
            file.ContentType,
            ms.ToArray(),
            description ?? string.Empty,
            userId);

        TempData["Success"] = $"File '{file.FileName}' attached successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Download File Attachment (file_download.php).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DownloadAttachment(int id)
    {
        var attachment = await _issueService.GetAttachmentByIdAsync(id);
        if (attachment == null || attachment.Content == null)
        {
            return NotFound();
        }

        var userId = _userContext.GetCurrentUserId() ?? 0;
        var issue = await _issueService.GetIssueByIdAsync(attachment.IssueId, includeDetails: false);
        if (issue == null) return NotFound();

        var userLevel = await _accessControl.GetEffectiveAccessLevelAsync(userId, issue.ProjectId);
        if (userLevel < AccessLevel.Viewer)
        {
            return Forbid();
        }

        return File(attachment.Content, attachment.FileType, attachment.FileName);
    }

    /// <summary>
    /// Delete File Attachment (bug_file_delete.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        var issue = await _issueService.GetIssueByIdAsync(id, includeDetails: false);
        if (issue == null) return NotFound();

        var userLevel = await _accessControl.GetEffectiveAccessLevelAsync(userId, issue.ProjectId);
        if (userLevel < AccessLevel.Developer)
        {
            return Forbid();
        }

        await _issueService.DeleteAttachmentAsync(attachmentId, userId);
        TempData["Success"] = "Attachment deleted.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Delete Issue permanently (bug_actiongroup_page.php -> DELETE).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        if (!await _accessControl.CanDeleteIssueAsync(userId, id))
        {
            return Forbid();
        }

        await _issueService.DeleteIssueAsync(id, userId);
        TempData["Success"] = $"Issue #{id:D7} deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Initiate Batch Action Group Confirmation Page (bug_actiongroup_page.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActionGroup(string actionGroupType, List<int> selectedIssueIds)
    {
        if (selectedIssueIds == null || !selectedIssueIds.Any())
        {
            TempData["Error"] = "No issues selected for batch action.";
            return RedirectToAction(nameof(Index));
        }

        var userId = _userContext.GetCurrentUserId() ?? 0;
        var issues = new List<Issue>();
        foreach (var id in selectedIssueIds)
        {
            var issue = await _issueService.GetIssueByIdAsync(id, includeDetails: false);
            if (issue != null) issues.Add(issue);
        }

        var users = await _userManager.Users.Where(u => u.Enabled).OrderBy(u => u.UserName).ToListAsync();
        var projects = await _projectService.GetAccessibleProjectsAsync(userId);

        var viewModel = new BugActionGroupViewModel
        {
            Action = actionGroupType,
            IssueIds = selectedIssueIds,
            Issues = issues,
            Users = users,
            Projects = projects
        };

        return View("ActionGroup", viewModel);
    }

    /// <summary>
    /// Execute Batch Action Group (bug_actiongroup.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExecuteActionGroup(BugActionGroupViewModel model)
    {
        if (model.IssueIds == null || !model.IssueIds.Any())
        {
            TempData["Error"] = "No issues selected.";
            return RedirectToAction(nameof(Index));
        }

        var userId = _userContext.GetCurrentUserId() ?? 0;

        switch (model.Action?.ToLower())
        {
            case "status":
                if (model.NewStatus.HasValue)
                {
                    await _issueService.BatchChangeStatusAsync(model.IssueIds, model.NewStatus.Value, model.NewResolution, model.ActionNote, userId);
                    TempData["Success"] = $"Status updated to {model.NewStatus.Value} for {model.IssueIds.Count} issues.";
                }
                break;

            case "assign":
                await _issueService.BatchAssignAsync(model.IssueIds, model.NewHandlerId, userId);
                TempData["Success"] = $"Reassigned {model.IssueIds.Count} issues.";
                break;

            case "priority":
                if (model.NewPriority.HasValue)
                {
                    await _issueService.BatchChangePriorityAsync(model.IssueIds, model.NewPriority.Value, userId);
                    TempData["Success"] = $"Priority set to {model.NewPriority.Value} for {model.IssueIds.Count} issues.";
                }
                break;

            case "severity":
                if (model.NewSeverity.HasValue)
                {
                    await _issueService.BatchChangeSeverityAsync(model.IssueIds, model.NewSeverity.Value, userId);
                    TempData["Success"] = $"Severity set to {model.NewSeverity.Value} for {model.IssueIds.Count} issues.";
                }
                break;

            case "viewstate":
                if (model.NewViewState.HasValue)
                {
                    await _issueService.BatchChangeViewStateAsync(model.IssueIds, model.NewViewState.Value, userId);
                    TempData["Success"] = $"View status updated for {model.IssueIds.Count} issues.";
                }
                break;

            case "move":
                if (model.NewProjectId.HasValue)
                {
                    await _issueService.BatchMoveProjectAsync(model.IssueIds, model.NewProjectId.Value, userId);
                    TempData["Success"] = $"Moved {model.IssueIds.Count} issues to project.";
                }
                break;

            case "delete":
                await _issueService.BatchDeleteAsync(model.IssueIds, userId);
                TempData["Success"] = $"Deleted {model.IssueIds.Count} issues.";
                break;

            default:
                TempData["Error"] = "Unrecognized batch action.";
                break;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Display Bug Reminder Page (bug_reminder_page.php).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Reminder(int id)
    {
        var issue = await _issueService.GetIssueByIdAsync(id, includeDetails: false);
        if (issue == null) return NotFound();

        var users = await _userManager.Users.Where(u => u.Enabled).OrderBy(u => u.UserName).ToListAsync();

        var viewModel = new ReminderViewModel
        {
            IssueId = id,
            Issue = issue,
            AvailableUsers = users
        };

        return View(viewModel);
    }

    /// <summary>
    /// Send Bug Reminder (bug_reminder.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reminder(ReminderViewModel model)
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        var issue = await _issueService.GetIssueByIdAsync(model.IssueId, includeDetails: false);
        if (issue == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.Issue = issue;
            model.AvailableUsers = await _userManager.Users.Where(u => u.Enabled).OrderBy(u => u.UserName).ToListAsync();
            return View(model);
        }

        var recipientNames = new List<string>();
        foreach (var uid in model.RecipientUserIds)
        {
            var user = await _userManager.FindByIdAsync(uid.ToString());
            if (user != null) recipientNames.Add(user.UserName ?? user.Id.ToString());
        }

        var reminderNote = $"[REMINDER SENT TO: {string.Join(", ", recipientNames)}]\n\n{model.ReminderText}";
        await _issueService.AddNoteAsync(model.IssueId, reminderNote, isPrivate: false, timeTrackingMinutes: 0, currentUserId: userId);

        TempData["Success"] = $"Reminder for issue #{model.IssueId:D7} sent to {recipientNames.Count} user(s).";
        return RedirectToAction(nameof(Details), new { id = model.IssueId });
    }
}
