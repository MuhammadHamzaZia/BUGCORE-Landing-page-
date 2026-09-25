using BugCore.Core.Entities;
using BugCore.Core.Enums;

namespace BugCore.Web.ViewModels.Projects;

public class ProjectListViewModel
{
    public IEnumerable<(Project Project, int Level)> HierarchicalProjects { get; set; } = new List<(Project, int)>();
    public IEnumerable<Category> GlobalCategories { get; set; } = new List<Category>();
    public IEnumerable<ApplicationUser> AllUsers { get; set; } = new List<ApplicationUser>();
    public bool CanCreateProject { get; set; }
    public bool CanManageSite { get; set; }
}

public class ProjectCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; } = 10; // Development
    public ProjectViewState ViewState { get; set; } = ProjectViewState.Public;
    public bool Enabled { get; set; } = true;
    public bool InheritCategories { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public int? ParentProjectId { get; set; }
    public Project? ParentProject { get; set; }
}

public class ProjectDetailsViewModel
{
    public Project Project { get; set; } = null!;
    public IEnumerable<ProjectHierarchy> Subprojects { get; set; } = new List<ProjectHierarchy>();
    public IEnumerable<Project> EligibleSubprojects { get; set; } = new List<Project>();
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    public IEnumerable<ProjectVersion> Versions { get; set; } = new List<ProjectVersion>();
    public IEnumerable<CustomFieldProject> LinkedCustomFields { get; set; } = new List<CustomFieldProject>();
    public IEnumerable<CustomField> AvailableCustomFields { get; set; } = new List<CustomField>();
    public IEnumerable<ProjectUser> AssignedUsers { get; set; } = new List<ProjectUser>();
    public IEnumerable<ApplicationUser> UnassignedUsers { get; set; } = new List<ApplicationUser>();
    public IEnumerable<ApplicationUser> GlobalUsers { get; set; } = new List<ApplicationUser>();
    public bool ShowGlobalUsers { get; set; }
    public IEnumerable<Project> OtherProjects { get; set; } = new List<Project>();
    public bool CanManage { get; set; }
}

public class CategoryEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; } = 0; // Active
    public int? DefaultAssigneeId { get; set; }
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }
    public IEnumerable<ApplicationUser> AvailableUsers { get; set; } = new List<ApplicationUser>();
}

public class VersionEditViewModel
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DateOrder { get; set; }
    public bool Released { get; set; }
    public bool Obsolete { get; set; }
}
