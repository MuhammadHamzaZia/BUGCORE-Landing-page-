using BugCore.Core.Entities;

namespace BugCore.Core.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetCategoriesForProjectAsync(int projectId);
    Task<IEnumerable<Category>> GetGlobalCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(int id);
    Task<bool> CopyCategoriesAsync(int sourceProjectId, int targetProjectId, bool excludeInherited = false);
}
