using BugCore.Core.Entities;

namespace BugCore.Core.Interfaces;

public interface ITagService
{
    Task<IEnumerable<Tag>> GetAllTagsAsync();
    Task<Tag?> GetTagByIdAsync(int tagId);
    Task<Tag> CreateTagAsync(string name, string description, int userId);
    Task UpdateTagAsync(int tagId, string name, string description, int userId);
    Task DeleteTagAsync(int tagId, int userId);
    Task AttachTagsToIssueAsync(int issueId, string tagNames, int userId);
    Task DetachTagFromIssueAsync(int issueId, int tagId, int userId);
}
