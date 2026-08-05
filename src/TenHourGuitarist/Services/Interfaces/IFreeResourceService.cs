using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Services.Interfaces;

public interface IFreeResourceService
{
    Task<List<FreeResource>> GetAllAsync(int page = 1, int pageSize = 12);
    Task<int> GetTotalCountAsync();
    Task<FreeResource?> GetByIdAsync(int id);
    Task<FreeResource?> GetBySlugAsync(string slug);
    Task<FreeResource> CreateAsync(FreeResource resource);
    Task UpdateAsync(FreeResource resource);
    Task DeleteAsync(int id);
}
