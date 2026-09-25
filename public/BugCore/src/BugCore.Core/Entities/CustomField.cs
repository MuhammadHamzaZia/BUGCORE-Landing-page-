using System.ComponentModel.DataAnnotations.Schema;
using BugCore.Core.Enums;

namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_custom_field_table. Defines custom field properties and schema.
/// </summary>
public class CustomField : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public CustomFieldType Type { get; set; } = CustomFieldType.String;
    public string PossibleValues { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public string ValidRegexp { get; set; } = string.Empty;

    public AccessLevel AccessLevelRead { get; set; } = AccessLevel.Viewer;
    public AccessLevel AccessLevelWrite { get; set; } = AccessLevel.Updater;

    public int LengthMin { get; set; } = 0;
    public int LengthMax { get; set; } = 255;

    public bool DisplayReport { get; set; } = true;
    public bool DisplayUpdate { get; set; } = true;
    public bool DisplayResolved { get; set; } = false;
    public bool DisplayClosed { get; set; } = false;

    public bool RequireReport { get; set; } = false;
    public bool RequireUpdate { get; set; } = false;
    public bool RequireResolved { get; set; } = false;
    public bool RequireClosed { get; set; } = false;

    // [NotMapped] Helper Aliases for Views & Controller compatibility
    [NotMapped]
    public string RegularExpression { get => ValidRegexp; set => ValidRegexp = value; }

    [NotMapped]
    public AccessLevel ReadAccessLevel { get => AccessLevelRead; set => AccessLevelRead = value; }

    [NotMapped]
    public AccessLevel WriteAccessLevel { get => AccessLevelWrite; set => AccessLevelWrite = value; }

    [NotMapped]
    public bool DisplayOnReport { get => DisplayReport; set => DisplayReport = value; }

    [NotMapped]
    public bool DisplayOnUpdate { get => DisplayUpdate; set => DisplayUpdate = value; }

    [NotMapped]
    public bool DisplayOnView { get => DisplayResolved; set => DisplayResolved = value; }

    [NotMapped]
    public bool RequireOnReport { get => RequireReport; set => RequireReport = value; }

    [NotMapped]
    public bool RequireOnUpdate { get => RequireUpdate; set => RequireUpdate = value; }

    public virtual ICollection<CustomFieldProject> ProjectLinks { get; set; } = new List<CustomFieldProject>();
    public virtual ICollection<CustomFieldValue> FieldValues { get; set; } = new List<CustomFieldValue>();
}
