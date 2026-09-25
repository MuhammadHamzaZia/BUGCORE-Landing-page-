namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_custom_field_project_table.
/// Maps custom fields to projects with custom display sequence ordering.
/// </summary>
public class CustomFieldProject
{
    public int CustomFieldId { get; set; }
    public virtual CustomField CustomField { get; set; } = null!;

    public int ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;

    public int Sequence { get; set; } = 0;
}
