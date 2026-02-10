using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Services.Interfaces;

public interface IPackageService
{
    Task<List<Package>> GetAllAsync(bool activeOnly = true);
    Task<Package?> GetByIdAsync(int id);
    Task<Package?> GetBySlugAsync(string slug);
    Task<Package> CreateAsync(Package package);
    Task UpdateAsync(Package package);
    Task DeleteAsync(int id);
}
