using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class FreeResourceService(ApplicationDbContext db) : IFreeResourceService
{
    public async Task<List<FreeResource>> GetAllAsync(int page = 1, int pageSize = 12)
    {
        return await db.FreeResources
            .Include(r => r.Author)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await db.FreeResources.CountAsync();
    }

    public async Task<FreeResource?> GetByIdAsync(int id)
    {
        return await db.FreeResources
            .Include(r => r.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<FreeResource?> GetBySlugAsync(string slug)
    {
        return await db.FreeResources
            .Include(r => r.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Slug == slug);
    }

    public async Task<FreeResource> CreateAsync(FreeResource resource)
    {
        db.FreeResources.Add(resource);
        await db.SaveChangesAsync();
        return resource;
    }

    public async Task UpdateAsync(FreeResource resource)
    {
        var existing = await db.FreeResources.FindAsync(resource.Id);
        if (existing is null) return;
        db.Entry(existing).CurrentValues.SetValues(resource);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var resource = await db.FreeResources.FindAsync(id);
        if (resource is not null)
        {
            db.FreeResources.Remove(resource);
            await db.SaveChangesAsync();
        }
    }
}
