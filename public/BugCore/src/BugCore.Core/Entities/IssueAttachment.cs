namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_bug_file_table. Manages bug file attachments and screenshots.
/// </summary>
public class IssueAttachment : BaseEntity
{
    public int IssueId { get; set; }
    public virtual Issue Issue { get; set; } = null!;

    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiskFileName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileFolder { get; set; } = string.Empty;
    public int FileSize { get; set; } = 0;
    public string FileType { get; set; } = string.Empty;
    public byte[]? Content { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}
