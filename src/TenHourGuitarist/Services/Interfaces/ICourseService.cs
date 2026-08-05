using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Services.Interfaces;

public interface ICourseService
{
    Task<List<Course>> GetAllAsync(bool publishedOnly = true);
    Task<Course?> GetByIdAsync(int id);
    Task<Course?> GetBySlugAsync(string slug);
    Task<List<Course>> SearchAsync(string? query, CourseLevel? level, int page = 1, int pageSize = 12);
    Task<int> GetTotalCountAsync(string? query, CourseLevel? level);
    Task<Course> CreateAsync(Course course);
    Task UpdateAsync(Course course);
    Task DeleteAsync(int id);
}
