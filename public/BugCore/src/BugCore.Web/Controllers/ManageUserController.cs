using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Web.Filters;
using BugCore.Web.ViewModels.ManageUser;

namespace BugCore.Web.Controllers;

[Authorize]
[BugCoreAuthorize(AccessLevel.Administrator, globalOnly: true)]
public class ManageUserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public ManageUserController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// User Management List View (manage_user_page.php).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? search, AccessLevel? accessLevel)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(u => u.UserName!.ToLower().Contains(q) || 
                                     (u.RealName != null && u.RealName.ToLower().Contains(q)) ||
                                     (u.Email != null && u.Email.ToLower().Contains(q)));
        }

        if (accessLevel.HasValue)
        {
            query = query.Where(u => u.GlobalAccessLevel == accessLevel.Value);
        }

        var users = await query.OrderBy(u => u.UserName).ToListAsync();

        var viewModel = new UserListViewModel
        {
            Users = users,
            SearchQuery = search,
            AccessLevelFilter = accessLevel
        };

        return View(viewModel);
    }

    /// <summary>
    /// Display Create User Form (manage_user_create_page.php).
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new CreateUserViewModel
        {
            AccessLevel = AccessLevel.Reporter,
            Enabled = true,
            Protected = false
        };

        return View(viewModel);
    }

    /// <summary>
    /// Handle User Creation (UserCreateCommand.php / manage_user_create.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _userManager.FindByNameAsync(model.UserName);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.UserName), "A user with this username already exists.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName.Trim(),
            RealName = model.RealName?.Trim() ?? string.Empty,
            Email = model.Email?.Trim(),
            GlobalAccessLevel = model.AccessLevel,
            Enabled = model.Enabled,
            Protected = model.Protected,
            DateCreated = DateTime.UtcNow,
            LastVisit = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Assign ASP.NET Identity Role
            var roleName = model.AccessLevel.ToString();
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
            await _userManager.AddToRoleAsync(user, roleName);

            TempData["Success"] = $"User '{user.UserName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    /// <summary>
    /// Display Edit User Form (manage_user_edit_page.php).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var viewModel = new EditUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            RealName = user.RealName,
            Email = user.Email,
            AccessLevel = user.GlobalAccessLevel,
            Enabled = user.Enabled,
            Protected = user.Protected
        };

        return View(viewModel);
    }

    /// <summary>
    /// Save User Edits (UserUpdateCommand.php / manage_user_update.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.Id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        if (user.Protected && user.UserName != model.UserName)
        {
            TempData["Error"] = "Protected users cannot change their username.";
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        user.UserName = model.UserName.Trim();
        user.RealName = model.RealName?.Trim() ?? string.Empty;
        user.Email = model.Email?.Trim();

        var oldLevel = user.GlobalAccessLevel;
        user.GlobalAccessLevel = model.AccessLevel;
        user.Enabled = model.Enabled;
        user.Protected = model.Protected;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            if (oldLevel != model.AccessLevel)
            {
                var oldRole = oldLevel.ToString();
                var newRole = model.AccessLevel.ToString();

                if (await _userManager.IsInRoleAsync(user, oldRole))
                {
                    await _userManager.RemoveFromRoleAsync(user, oldRole);
                }

                if (!await _roleManager.RoleExistsAsync(newRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>(newRole));
                }
                await _userManager.AddToRoleAsync(user, newRole);
            }

            TempData["Success"] = $"User '{user.UserName}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    /// <summary>
    /// Delete User Account (UserDeleteCommand.php / manage_user_delete.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        if (user.Protected)
        {
            TempData["Error"] = "Protected accounts cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = $"User account '{user.UserName}' deleted.";
        }
        else
        {
            TempData["Error"] = "Failed to delete user account.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Reset User Password (UserResetPasswordCommand.php).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, string? newPassword)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        if (user.Protected)
        {
            TempData["Error"] = "Protected accounts cannot have their password reset.";
            return RedirectToAction(nameof(Index));
        }

        var resetPassword = string.IsNullOrWhiteSpace(newPassword) ? "BugCore123!" : newPassword.Trim();
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, resetPassword);

        if (result.Succeeded)
        {
            TempData["Success"] = $"Password for user '{user.UserName}' reset to: {resetPassword}";
        }
        else
        {
            TempData["Error"] = "Failed to reset password.";
        }

        return RedirectToAction(nameof(Index));
    }
}
