using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class TagService : ITagService
{
    private readonly BugCoreDbContext _context;

    public TagService(BugCoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        return await _context.Tags.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<Tag?> GetTagByIdAsync(int tagId)
    {
        return await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagId);
    }

    public async Task<Tag> CreateTagAsync(string name, string description, int userId)
    {
        var tag = new Tag
        {
            Name = name.Trim(),
            Description = description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task UpdateTagAsync(int tagId, string name, string description, int userId)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag != null)
        {
            tag.Name = name.Trim();
            tag.Description = description;
            tag.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteTagAsync(int tagId, int userId)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag != null)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AttachTagsToIssueAsync(int issueId, string tagNames, int userId)
    {
        if (string.IsNullOrWhiteSpace(tagNames)) return;

        var names = tagNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var name in names)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
            if (tag == null)
            {
                tag = await CreateTagAsync(name, string.Empty, userId);
            }

            var exists = await _context.IssueTags.AnyAsync(it => it.IssueId == issueId && it.TagId == tag.Id);
            if (!exists)
            {
                _context.IssueTags.Add(new IssueTag
                {
                    IssueId = issueId,
                    TagId = tag.Id,
                    DateAttached = DateTime.UtcNow
                });
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task DetachTagFromIssueAsync(int issueId, int tagId, int userId)
    {
        var issueTag = await _context.IssueTags.FirstOrDefaultAsync(it => it.IssueId == issueId && it.TagId == tagId);
        if (issueTag != null)
        {
            _context.IssueTags.Remove(issueTag);
            await _context.SaveChangesAsync();
        }
    }
}
