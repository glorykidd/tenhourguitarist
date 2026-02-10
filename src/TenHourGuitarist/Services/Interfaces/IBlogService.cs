using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Services.Interfaces;

public interface IBlogService
{
    Task<List<BlogPost>> GetAllAsync(bool publishedOnly = true, int page = 1, int pageSize = 10);
    Task<int> GetTotalCountAsync(bool publishedOnly = true);
    Task<BlogPost?> GetByIdAsync(int id);
    Task<BlogPost?> GetBySlugAsync(string slug);
    Task<List<BlogPost>> GetByCategoryAsync(int categoryId, int page = 1, int pageSize = 10);
    Task<BlogPost> CreateAsync(BlogPost post);
    Task UpdateAsync(BlogPost post);
    Task DeleteAsync(int id);
    Task<List<BlogCategory>> GetCategoriesAsync();
    Task<BlogCategory?> GetCategoryByIdAsync(int id);
    Task<BlogCategory> CreateCategoryAsync(BlogCategory category);
    Task UpdateCategoryAsync(BlogCategory category);
    Task DeleteCategoryAsync(int id);
}
