using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class ProjectDocService : IProjectDocService
{
    private readonly BugCoreDbContext _context;

    public ProjectDocService(BugCoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectDoc>> GetDocsForProjectAsync(int projectId)
    {
        return await _context.ProjectDocs
            .Where(d => d.ProjectId == projectId)
            .Include(d => d.User)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProjectDoc?> GetDocByIdAsync(int docId)
    {
        return await _context.ProjectDocs
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == docId);
    }

    public async Task<ProjectDoc> AddDocAsync(int projectId, string title, string description, string fileName, string fileType, byte[] content, int userId)
    {
        var doc = new ProjectDoc
        {
            ProjectId = projectId,
            Title = title,
            Description = description,
            FileName = fileName,
            FileType = fileType,
            FileSize = content?.Length ?? 0,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        _context.ProjectDocs.Add(doc);
        await _context.SaveChangesAsync();
        return doc;
    }

    public async Task UpdateDocAsync(int docId, string title, string description, string? fileName, string? fileType, byte[]? content, int userId)
    {
        var doc = await _context.ProjectDocs.FindAsync(docId);
        if (doc != null)
        {
            doc.Title = title;
            doc.Description = description;
            if (content != null && content.Length > 0)
            {
                doc.FileName = fileName ?? doc.FileName;
                doc.FileType = fileType ?? doc.FileType;
                doc.FileSize = content.Length;
                doc.Content = content;
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteDocAsync(int docId, int userId)
    {
        var doc = await _context.ProjectDocs.FindAsync(docId);
        if (doc != null)
        {
            _context.ProjectDocs.Remove(doc);
            await _context.SaveChangesAsync();
        }
    }
}
