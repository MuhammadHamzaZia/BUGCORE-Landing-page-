namespace BugCore.Core.Enums;

/// <summary>
/// Mirrors BugCoreBT access thresholds defined in core/constant_inc.php.
/// Values are strictly hierarchical.
/// </summary>
public enum AccessLevel
{
    Any = 0,
    Viewer = 10,
    Reporter = 25,
    Updater = 40,
    Modifier = 40,
    Developer = 55,
    Manager = 70,
    Administrator = 90,
    Nobody = 100
}
