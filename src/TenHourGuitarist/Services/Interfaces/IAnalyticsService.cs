using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Services.Interfaces;

public interface IAnalyticsService
{
    Task RecordVisitAsync();
    Task<int> GetTodayVisitCountAsync();
    Task<List<SiteVisit>> GetVisitHistoryAsync(int days = 30);
    Task<int> GetTotalUsersAsync();
    Task<int> GetTotalSubscribersAsync();
    Task<int> GetTotalCoursesAsync();
    Task<decimal> GetTotalRevenueAsync();
}
