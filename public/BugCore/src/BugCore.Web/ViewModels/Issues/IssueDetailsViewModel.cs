using BugCore.Core.Entities;
using BugCore.Core.Enums;

namespace BugCore.Web.ViewModels.Issues;

public class IssueDetailsViewModel
{
    public Issue Issue { get; set; } = null!;
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanAddNote { get; set; }
    public bool CanAttachFile { get; set; }
    public bool CanChangeStatus { get; set; }
    public bool CanAssign { get; set; }
    public bool IsMonitored { get; set; }
    public AccessLevel UserAccessLevel { get; set; }

    public IEnumerable<ApplicationUser> AssignableUsers { get; set; } = new List<ApplicationUser>();
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    public IEnumerable<ProjectVersion> Versions { get; set; } = new List<ProjectVersion>();
    public IEnumerable<Project> AvailableProjects { get; set; } = new List<Project>();
    public IEnumerable<CustomFieldProject> LinkedCustomFields { get; set; } = new List<CustomFieldProject>();
}
