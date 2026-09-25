using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly BugCoreDbContext _context;

    public CategoryService(BugCoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetCategoriesForProjectAsync(int projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        bool inherit = project?.InheritCategories ?? true;

        var query = _context.Categories
            .Include(c => c.DefaultAssignee)
            .Where(c => c.ProjectId == projectId);

        if (inherit)
        {
            // Also include global categories (ProjectId is null or 0)
            query = _context.Categories
                .Include(c => c.DefaultAssignee)
                .Where(c => c.ProjectId == projectId || c.ProjectId == null || c.ProjectId == 0);
        }

        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetGlobalCategoriesAsync()
    {
        return await _context.Categories
            .Include(c => c.DefaultAssignee)
            .Where(c => c.ProjectId == null || c.ProjectId == 0)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.DefaultAssignee)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var cat = await _context.Categories.FindAsync(id);
        if (cat == null) return false;

        _context.Categories.Remove(cat);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CopyCategoriesAsync(int sourceProjectId, int targetProjectId, bool excludeInherited = false)
    {
        IQueryable<Category> query = _context.Categories;
        if (sourceProjectId == 0)
        {
            query = query.Where(c => c.ProjectId == null || c.ProjectId == 0);
        }
        else if (excludeInherited)
        {
            query = query.Where(c => c.ProjectId == sourceProjectId);
        }
        else
        {
            query = query.Where(c => c.ProjectId == sourceProjectId || c.ProjectId == null || c.ProjectId == 0);
        }

        var categories = await query.ToListAsync();

        foreach (var c in categories)
        {
            var exists = await _context.Categories
                .AnyAsync(tc => (targetProjectId == 0 ? (tc.ProjectId == null || tc.ProjectId == 0) : tc.ProjectId == targetProjectId) 
                                && tc.Name.ToLower() == c.Name.ToLower());

            if (!exists)
            {
                _context.Categories.Add(new Category
                {
                    Name = c.Name,
                    ProjectId = targetProjectId == 0 ? null : targetProjectId,
                    DefaultAssigneeId = c.DefaultAssigneeId,
                    Status = c.Status
                });
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
