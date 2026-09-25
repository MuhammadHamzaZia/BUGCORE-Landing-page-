namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_custom_field_string_table. Stores the concrete value for an issue.
/// </summary>
public class CustomFieldValue
{
    public int CustomFieldId { get; set; }
    public virtual CustomField CustomField { get; set; } = null!;

    public int IssueId { get; set; }
    public virtual Issue Issue { get; set; } = null!;

    public string Value { get; set; } = string.Empty;
}
