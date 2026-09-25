using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Web.Filters;

namespace BugCore.Web.Controllers;

[Authorize]
[BugCoreAuthorize(AccessLevel.Administrator, globalOnly: true)]
public class CustomFieldController : Controller
{
    private readonly ICustomFieldService _customFieldService;

    public CustomFieldController(ICustomFieldService customFieldService)
    {
        _customFieldService = customFieldService;
    }

    /// <summary>
    /// Custom Field Management List View (manage_custom_field_page.php).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var fields = await _customFieldService.GetAllFieldsAsync();
        return View(fields);
    }

    /// <summary>
    /// Display Custom Field Creation Form (manage_custom_field_create_page.php).
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        var model = new CustomField
        {
            Type = CustomFieldType.String,
            ReadAccessLevel = AccessLevel.Viewer,
            WriteAccessLevel = AccessLevel.Developer,
            DisplayOnReport = true,
            DisplayOnUpdate = true,
            DisplayOnView = true,
            RequireOnReport = false,
            RequireOnUpdate = false
        };

        return View(model);
    }

    /// <summary>
    /// Save New Custom Field Definition (manage_custom_field_create.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomField field)
    {
        if (string.IsNullOrWhiteSpace(field.Name))
        {
            ModelState.AddModelError(nameof(field.Name), "Custom field name is required.");
        }

        if (!ModelState.IsValid)
        {
            return View(field);
        }

        field.Name = field.Name.Trim();
        field.PossibleValues = field.PossibleValues ?? string.Empty;
        field.DefaultValue = field.DefaultValue ?? string.Empty;
        field.RegularExpression = field.RegularExpression ?? string.Empty;

        var created = await _customFieldService.CreateFieldAsync(field);
        TempData["Success"] = $"Custom field '{created.Name}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Display Custom Field Edit Form (manage_custom_field_edit_page.php).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var field = await _customFieldService.GetFieldByIdAsync(id);
        if (field == null)
        {
            return NotFound();
        }

        return View(field);
    }

    /// <summary>
    /// Save Custom Field Edits (manage_custom_field_update.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomField field)
    {
        if (string.IsNullOrWhiteSpace(field.Name))
        {
            ModelState.AddModelError(nameof(field.Name), "Custom field name is required.");
            return View(field);
        }

        var existing = await _customFieldService.GetFieldByIdAsync(field.Id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Name = field.Name.Trim();
        existing.Type = field.Type;
        existing.PossibleValues = field.PossibleValues ?? string.Empty;
        existing.DefaultValue = field.DefaultValue ?? string.Empty;
        existing.RegularExpression = field.RegularExpression ?? string.Empty;
        existing.ReadAccessLevel = field.ReadAccessLevel;
        existing.WriteAccessLevel = field.WriteAccessLevel;
        existing.DisplayOnReport = field.DisplayOnReport;
        existing.DisplayOnUpdate = field.DisplayOnUpdate;
        existing.DisplayOnView = field.DisplayOnView;
        existing.RequireOnReport = field.RequireOnReport;
        existing.RequireOnUpdate = field.RequireOnUpdate;

        await _customFieldService.UpdateFieldAsync(existing);
        TempData["Success"] = $"Custom field '{existing.Name}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Delete Custom Field Definition (manage_custom_field_delete.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var field = await _customFieldService.GetFieldByIdAsync(id);
        if (field != null)
        {
            await _customFieldService.DeleteFieldAsync(id);
            TempData["Success"] = $"Custom field '{field.Name}' deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
