using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class PackageService(ApplicationDbContext db) : IPackageService
{
    public async Task<List<Package>> GetAllAsync(bool activeOnly = true)
    {
        var query = db.Packages.AsNoTracking();

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive && !p.IsDeleted);
        }

        return await query
            .OrderBy(p => p.Amount)
            .ToListAsync();
    }

    public async Task<Package?> GetByIdAsync(int id)
    {
        return await db.Packages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Package?> GetBySlugAsync(string slug)
    {
        return await db.Packages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<Package> CreateAsync(Package package)
    {
        db.Packages.Add(package);
        await db.SaveChangesAsync();
        return package;
    }

    public async Task UpdateAsync(Package package)
    {
        db.Packages.Update(package);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var package = await db.Packages.FindAsync(id);
        if (package is not null)
        {
            package.IsDeleted = true;
            await db.SaveChangesAsync();
        }
    }
}
