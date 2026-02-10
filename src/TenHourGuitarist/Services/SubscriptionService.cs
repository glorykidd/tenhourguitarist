using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class SubscriptionService(ApplicationDbContext db) : ISubscriptionService
{
    public async Task<Subscription?> GetActiveSubscriptionAsync(string userId)
    {
        return await db.Subscriptions
            .Include(s => s.Package)
            .Where(s => s.UserId == userId
                        && s.Status == SubscriptionStatus.Active
                        && (s.EndDate == null || s.EndDate > DateTime.UtcNow))
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasActiveSubscriptionAsync(string userId)
    {
        return await db.Subscriptions
            .AnyAsync(s => s.UserId == userId
                           && s.Status == SubscriptionStatus.Active
                           && (s.EndDate == null || s.EndDate > DateTime.UtcNow));
    }

    public async Task<List<Subscription>> GetByUserIdAsync(string userId)
    {
        return await db.Subscriptions
            .Include(s => s.Package)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Subscription> CreateAsync(Subscription subscription)
    {
        db.Subscriptions.Add(subscription);
        await db.SaveChangesAsync();
        return subscription;
    }

    public async Task UpdateStatusAsync(int id, SubscriptionStatus status)
    {
        var subscription = await db.Subscriptions.FindAsync(id);
        if (subscription is not null)
        {
            subscription.Status = status;
            await db.SaveChangesAsync();
        }
    }

    public async Task UpdateByStripeIdAsync(string stripeSubscriptionId, SubscriptionStatus status, DateTime? endDate = null)
    {
        var subscription = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);

        if (subscription is not null)
        {
            subscription.Status = status;
            if (endDate.HasValue)
            {
                subscription.EndDate = endDate.Value;
            }

            await db.SaveChangesAsync();
        }
    }
}
