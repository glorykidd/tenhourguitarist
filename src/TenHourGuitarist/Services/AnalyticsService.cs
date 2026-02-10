using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class AnalyticsService(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager) : IAnalyticsService
{
    public async Task RecordVisitAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var visit = await db.SiteVisits
            .FirstOrDefaultAsync(v => v.Date == today);

        if (visit is not null)
        {
            visit.Count++;
        }
        else
        {
            visit = new SiteVisit
            {
                Date = today,
                Count = 1
            };
            db.SiteVisits.Add(visit);
        }

        await db.SaveChangesAsync();
    }

    public async Task<int> GetTodayVisitCountAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var visit = await db.SiteVisits
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Date == today);

        return visit?.Count ?? 0;
    }

    public async Task<List<SiteVisit>> GetVisitHistoryAsync(int days = 30)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));

        return await db.SiteVisits
            .Where(v => v.Date >= startDate)
            .OrderBy(v => v.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetTotalUsersAsync()
    {
        return await db.Users
            .Where(u => !u.IsDeleted)
            .CountAsync();
    }

    public async Task<int> GetTotalSubscribersAsync()
    {
        var subscriberRole = "Subscriber";
        var usersInRole = await userManager.GetUsersInRoleAsync(subscriberRole);
        return usersInRole.Count;
    }

    public async Task<int> GetTotalCoursesAsync()
    {
        return await db.Courses
            .Where(c => c.IsPublished)
            .CountAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await db.Orders
            .Where(o => o.PaymentStatus == PaymentStatus.Success)
            .SumAsync(o => o.Amount);
    }
}
