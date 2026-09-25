namespace BugCore.Core.Entities;

public class ProjectDoc : BaseEntity
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public byte[]? Content { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }

    public virtual Project? Project { get; set; }
    public virtual ApplicationUser? User { get; set; }
}
