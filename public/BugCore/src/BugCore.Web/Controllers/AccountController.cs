using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Web.ViewModels.Account;

namespace BugCore.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null || !user.Enabled)
        {
            ModelState.AddModelError(string.Empty, "Invalid login credentials or account disabled.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Username, 
            model.Password, 
            model.RememberMe, 
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            user.LastVisit = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError(string.Empty, "Invalid password.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Signup()
    {
        return View(new SignupViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Username,
            Email = model.Email,
            RealName = model.RealName,
            GlobalAccessLevel = AccessLevel.Reporter,
            Enabled = true,
            DateCreated = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Reporter");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Dashboard");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAnonymous(string? returnUrl = null)
    {
        // Sign in guest user or redirect (BugCoreBT $g_anonymous_account default: 'guest')
        var guestUser = await _userManager.FindByNameAsync("guest");
        if (guestUser == null)
        {
            guestUser = new ApplicationUser
            {
                UserName = "guest",
                Email = "guest@localhost",
                RealName = "Anonymous Guest",
                GlobalAccessLevel = AccessLevel.Viewer,
                Enabled = true,
                EmailConfirmed = true,
                DateCreated = DateTime.UtcNow
            };
            var createResult = await _userManager.CreateAsync(guestUser, "guest");
            if (!createResult.Succeeded)
            {
                await _userManager.CreateAsync(guestUser, "Guest_Password123!");
            }
            await _userManager.AddToRoleAsync(guestUser, "Viewer");
        }

        if (guestUser.Enabled)
        {
            await _signInManager.SignInAsync(guestUser, isPersistent: false);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Dashboard");
        }

        TempData["Error"] = "Anonymous login is not enabled on this server.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
