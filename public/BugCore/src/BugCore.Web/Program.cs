using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;
using BugCore.Infrastructure.Services;
using BugCore.Web.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add Database Context (Defaulting to In-Memory / SQLite / LocalDB for high portability)
builder.Services.AddDbContext<BugCoreDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=(localdb)\\mssqllocaldb;Database=BugCoreDb;Trusted_Connection=True;MultipleActiveResultSets=true";
    options.UseSqlServer(connectionString);
});

// Configure Identity & Authentication
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<BugCoreDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
});

// Register Domain & Infrastructure Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IVersionService, VersionService>();
builder.Services.AddScoped<ICustomFieldService, CustomFieldService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IApiTokenService, ApiTokenService>();
builder.Services.AddScoped<IProjectDocService, ProjectDocService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISlackRequestVerifier, SlackRequestVerifier>();
builder.Services.AddScoped<ISlackService, SlackService>();
builder.Services.AddScoped<ITeamsService, TeamsService>();
builder.Services.AddScoped<IGitWebhookService, GitWebhookService>();
builder.Services.AddScoped<IEmailGatewayService, EmailGatewayService>();

// Register Filters & MVC Services
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ProjectContextFilter>();
});

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BugCoreDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        await DbInitializer.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the BugCoreNet database.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
