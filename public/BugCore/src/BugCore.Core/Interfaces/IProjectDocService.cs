using BugCore.Core.Entities;

namespace BugCore.Core.Interfaces;

public interface IProjectDocService
{
    Task<IEnumerable<ProjectDoc>> GetDocsForProjectAsync(int projectId);
    Task<ProjectDoc?> GetDocByIdAsync(int docId);
    Task<ProjectDoc> AddDocAsync(int projectId, string title, string description, string fileName, string fileType, byte[] content, int userId);
    Task UpdateDocAsync(int docId, string title, string description, string? fileName, string? fileType, byte[]? content, int userId);
    Task DeleteDocAsync(int docId, int userId);
}
