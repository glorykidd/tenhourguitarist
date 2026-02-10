using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Services.Interfaces;

public interface IOrderService
{
    Task<List<Order>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<int> GetTotalCountAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByOrderRefIdAsync(Guid orderRefId);
    Task<Order?> GetByStripeSessionIdAsync(string sessionId);
    Task<List<Order>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20);
    Task<int> GetUserOrderCountAsync(string userId);
    Task<Order> CreateAsync(Order order);
    Task UpdateAsync(Order order);
}
