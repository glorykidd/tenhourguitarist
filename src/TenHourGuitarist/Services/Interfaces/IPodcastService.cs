using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Services.Interfaces;

public interface IPodcastService
{
    Task<List<Podcast>> GetAllAsync(bool publishedOnly = true, int page = 1, int pageSize = 10);
    Task<int> GetTotalCountAsync(bool publishedOnly = true);
    Task<Podcast?> GetByIdAsync(int id);
    Task<Podcast?> GetBySlugAsync(string slug);
    Task<Podcast> CreateAsync(Podcast podcast);
    Task UpdateAsync(Podcast podcast);
    Task DeleteAsync(int id);
}
