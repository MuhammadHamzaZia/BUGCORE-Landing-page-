using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;

namespace BugCore.Infrastructure.Data;

public class BugCoreDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public BugCoreDbContext(DbContextOptions<BugCoreDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectHierarchy> ProjectHierarchies => Set<ProjectHierarchy>();
    public DbSet<ProjectUser> ProjectUsers => Set<ProjectUser>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProjectVersion> ProjectVersions => Set<ProjectVersion>();
    public DbSet<CustomField> CustomFields => Set<CustomField>();
    public DbSet<CustomFieldProject> CustomFieldProjects => Set<CustomFieldProject>();
    public DbSet<CustomFieldValue> CustomFieldValues => Set<CustomFieldValue>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<IssueNote> IssueNotes => Set<IssueNote>();
    public DbSet<IssueHistory> IssueHistories => Set<IssueHistory>();
    public DbSet<IssueRelationship> IssueRelationships => Set<IssueRelationship>();
    public DbSet<IssueMonitor> IssueMonitors => Set<IssueMonitor>();
    public DbSet<IssueAttachment> IssueAttachments => Set<IssueAttachment>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<IssueTag> IssueTags => Set<IssueTag>();
    public DbSet<ApiToken> ApiTokens => Set<ApiToken>();
    public DbSet<ProjectDoc> ProjectDocs => Set<ProjectDoc>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations from the current assembly
        builder.ApplyConfigurationsFromAssembly(typeof(BugCoreDbContext).Assembly);
    }
}
