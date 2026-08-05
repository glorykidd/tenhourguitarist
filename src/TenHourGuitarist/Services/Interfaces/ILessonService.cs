using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Services.Interfaces;

public interface ILessonService
{
    Task<List<Lesson>> GetByCourseIdAsync(int courseId);
    Task<Lesson?> GetByIdAsync(int id);
    Task<Lesson?> GetBySlugAsync(string courseSlug, string lessonSlug);
    Task<List<Lesson>> GetFreeLessonsAsync();
    Task<Lesson> CreateAsync(Lesson lesson);
    Task UpdateAsync(Lesson lesson);
    Task DeleteAsync(int id);
    Task RecordViewAsync(string userId, int lessonId, int courseId);
    Task<List<LessonHistory>> GetUserHistoryAsync(string userId, int page = 1, int pageSize = 20);
    Task<int> GetUserHistoryCountAsync(string userId);
}
