namespace BugCore.Core.Enums;

/// <summary>
/// Mirrors BugCoreBT issue workflow status enumeration.
/// </summary>
public enum IssueStatus
{
    New = 10,
    Feedback = 20,
    Acknowledged = 30,
    Confirmed = 40,
    Assigned = 50,
    Resolved = 80,
    Closed = 90
}
