using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class BlogService(ApplicationDbContext db) : IBlogService
{
    public async Task<List<BlogPost>> GetAllAsync(bool publishedOnly = true, int page = 1, int pageSize = 10)
    {
        var query = db.BlogPosts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .AsNoTracking();

        if (publishedOnly)
        {
            query = query.Where(p => p.IsPublished);
        }

        return await query
            .OrderByDescending(p => p.PublishedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(bool publishedOnly = true)
    {
        var query = db.BlogPosts.AsQueryable();

        if (publishedOnly)
        {
            query = query.Where(p => p.IsPublished);
        }

        return await query.CountAsync();
    }

    public async Task<BlogPost?> GetByIdAsync(int id)
    {
        return await db.BlogPosts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<BlogPost?> GetBySlugAsync(string slug)
    {
        return await db.BlogPosts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<List<BlogPost>> GetByCategoryAsync(int categoryId, int page = 1, int pageSize = 10)
    {
        return await db.BlogPosts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsPublished)
            .OrderByDescending(p => p.PublishedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<BlogPost> CreateAsync(BlogPost post)
    {
        db.BlogPosts.Add(post);
        await db.SaveChangesAsync();
        return post;
    }

    public async Task UpdateAsync(BlogPost post)
    {
        db.BlogPosts.Update(post);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var post = await db.BlogPosts.FindAsync(id);
        if (post is not null)
        {
            db.BlogPosts.Remove(post);
            await db.SaveChangesAsync();
        }
    }

    // Blog categories

    public async Task<List<BlogCategory>> GetCategoriesAsync()
    {
        return await db.BlogCategories
            .Include(c => c.Parent)
            .Include(c => c.Children)
            .OrderBy(c => c.Title)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<BlogCategory?> GetCategoryByIdAsync(int id)
    {
        return await db.BlogCategories
            .Include(c => c.Parent)
            .Include(c => c.Children)
            .Include(c => c.BlogPosts)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<BlogCategory> CreateCategoryAsync(BlogCategory category)
    {
        db.BlogCategories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(BlogCategory category)
    {
        db.BlogCategories.Update(category);
        await db.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await db.BlogCategories.FindAsync(id);
        if (category is not null)
        {
            db.BlogCategories.Remove(category);
            await db.SaveChangesAsync();
        }
    }
}
