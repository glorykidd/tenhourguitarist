using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Services.Interfaces;

public interface ISubscriptionService
{
    Task<Subscription?> GetActiveSubscriptionAsync(string userId);
    Task<bool> HasActiveSubscriptionAsync(string userId);
    Task<List<Subscription>> GetByUserIdAsync(string userId);
    Task<Subscription> CreateAsync(Subscription subscription);
    Task UpdateStatusAsync(int id, SubscriptionStatus status);
    Task UpdateByStripeIdAsync(string stripeSubscriptionId, SubscriptionStatus status, DateTime? endDate = null);
}
