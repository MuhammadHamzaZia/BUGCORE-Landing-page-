using System.ComponentModel.DataAnnotations.Schema;

namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_project_hierarchy_table. Manages parent/child project nesting.
/// </summary>
public class ProjectHierarchy
{
    public int ParentProjectId { get; set; }
    public virtual Project ParentProject { get; set; } = null!;

    public int ChildProjectId { get; set; }
    public virtual Project ChildProject { get; set; } = null!;

    public bool InheritChild { get; set; } = false;

    [NotMapped]
    public bool InheritParent { get => InheritChild; set => InheritChild = value; }
}
