using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class PodcastService(ApplicationDbContext db) : IPodcastService
{
    public async Task<List<Podcast>> GetAllAsync(bool publishedOnly = true, int page = 1, int pageSize = 10)
    {
        var query = db.Podcasts
            .Include(p => p.Author)
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
        var query = db.Podcasts.AsQueryable();

        if (publishedOnly)
        {
            query = query.Where(p => p.IsPublished);
        }

        return await query.CountAsync();
    }

    public async Task<Podcast?> GetByIdAsync(int id)
    {
        return await db.Podcasts
            .Include(p => p.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Podcast?> GetBySlugAsync(string slug)
    {
        return await db.Podcasts
            .Include(p => p.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<Podcast> CreateAsync(Podcast podcast)
    {
        db.Podcasts.Add(podcast);
        await db.SaveChangesAsync();
        return podcast;
    }

    public async Task UpdateAsync(Podcast podcast)
    {
        db.Podcasts.Update(podcast);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var podcast = await db.Podcasts.FindAsync(id);
        if (podcast is not null)
        {
            db.Podcasts.Remove(podcast);
            await db.SaveChangesAsync();
        }
    }
}
