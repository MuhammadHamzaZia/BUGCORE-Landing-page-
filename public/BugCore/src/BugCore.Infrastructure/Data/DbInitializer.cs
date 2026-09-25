using Microsoft.AspNetCore.Identity;
using BugCore.Core.Entities;
using BugCore.Core.Enums;

namespace BugCore.Infrastructure.Data;

/// <summary>
/// Seeds standard BugCoreBT system roles and default administrator account.
/// No pre-populated projects or demo issues are created, ensuring a clean initial state.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(
        BugCoreDbContext context, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole<int>> roleManager)
    {
        try
        {
            await SeedDataAsync(context, userManager, roleManager);
        }
        catch (Exception)
        {
            context.ChangeTracker.Clear();
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            context.ChangeTracker.Clear();
            await SeedDataAsync(context, userManager, roleManager);
        }
    }

    private static async Task SeedDataAsync(
        BugCoreDbContext context, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole<int>> roleManager)
    {
        await context.Database.EnsureCreatedAsync();

        // 1. Seed Roles (BugCoreBT standard access levels)
        string[] roles = ["Administrator", "Manager", "Developer", "Updater", "Reporter", "Viewer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        // 2. Seed Default Administrator Account (administrator / root)
        var adminUser = await userManager.FindByNameAsync("administrator");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "administrator",
                Email = "root@localhost",
                RealName = "System Administrator",
                GlobalAccessLevel = AccessLevel.Administrator,
                Enabled = true,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "root_Password123!");
            if (!result.Succeeded)
            {
                result = await userManager.CreateAsync(adminUser, "root");
            }
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }

        // Global default categories if needed for unassigned categories
        if (!context.Categories.Any(c => c.ProjectId == null || c.ProjectId == 0))
        {
            context.Categories.AddRange(
                new Category { Name = "General Defect", ProjectId = null, Status = 0 },
                new Category { Name = "Documentation", ProjectId = null, Status = 0 }
            );
            await context.SaveChangesAsync();
        }
    }
}
