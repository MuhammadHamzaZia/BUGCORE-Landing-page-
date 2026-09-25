namespace BugCore.Core.Enums;

/// <summary>
/// Mirrors BugCoreBT reproducibility enumeration.
/// </summary>
public enum ReproducibilityType
{
    Always = 10,
    Sometimes = 30,
    Random = 50,
    HaveNotTried = 70,
    UnableToReproduce = 90,
    NotApplicable = 100
}
