using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class OrderService(ApplicationDbContext db) : IOrderService
{
    public async Task<List<Order>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        return await db.Orders
            .Include(o => o.User)
            .Include(o => o.Package)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await db.Orders.CountAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await db.Orders
            .Include(o => o.User)
            .Include(o => o.Package)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetByOrderRefIdAsync(Guid orderRefId)
    {
        return await db.Orders
            .Include(o => o.User)
            .Include(o => o.Package)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderRefId == orderRefId);
    }

    public async Task<Order?> GetByStripeSessionIdAsync(string sessionId)
    {
        return await db.Orders
            .Include(o => o.User)
            .Include(o => o.Package)
            .FirstOrDefaultAsync(o => o.StripeSessionId == sessionId);
    }

    public async Task<List<Order>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20)
    {
        return await db.Orders
            .Include(o => o.User)
            .Include(o => o.Package)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetUserOrderCountAsync(string userId)
    {
        return await db.Orders
            .Where(o => o.UserId == userId)
            .CountAsync();
    }

    public async Task<Order> CreateAsync(Order order)
    {
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        var existing = await db.Orders.FindAsync(order.Id);
        if (existing is null) return;
        db.Entry(existing).CurrentValues.SetValues(order);
        await db.SaveChangesAsync();
    }
}
