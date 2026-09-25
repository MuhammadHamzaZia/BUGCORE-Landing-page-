namespace BugCore.Core.Entities;

public class ApiToken : BaseEntity
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string Scope { get; set; } = "full_access"; // read_only, issue_write, project_restricted, full_access
    public int? ProjectId { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(90);
    public DateTime? DateUsed { get; set; }
    public string? LastUsedIp { get; set; }
    public bool IsRevoked { get; set; } = false;

    public virtual ApplicationUser? User { get; set; }
    public virtual Project? Project { get; set; }
}
