using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Services.Interfaces;

public interface IContactService
{
    Task<List<ContactMessage>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<int> GetTotalCountAsync();
    Task<ContactMessage?> GetByIdAsync(int id);
    Task<ContactMessage> CreateAsync(ContactMessage message);
    Task DeleteAsync(int id);
}
