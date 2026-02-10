using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class LessonService(ApplicationDbContext db) : ILessonService
{
    public async Task<List<Lesson>> GetByCourseIdAsync(int courseId)
    {
        return await db.Lessons
            .Include(l => l.Course)
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Lesson?> GetByIdAsync(int id)
    {
        return await db.Lessons
            .Include(l => l.Course)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Lesson?> GetBySlugAsync(string courseSlug, string lessonSlug)
    {
        return await db.Lessons
            .Include(l => l.Course)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Course.Slug == courseSlug && l.Slug == lessonSlug);
    }

    public async Task<List<Lesson>> GetFreeLessonsAsync()
    {
        return await db.Lessons
            .Include(l => l.Course)
            .Where(l => l.Course.IsFree)
            .OrderBy(l => l.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Lesson> CreateAsync(Lesson lesson)
    {
        db.Lessons.Add(lesson);
        await db.SaveChangesAsync();
        return lesson;
    }

    public async Task UpdateAsync(Lesson lesson)
    {
        db.Lessons.Update(lesson);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var lesson = await db.Lessons.FindAsync(id);
        if (lesson is not null)
        {
            db.Lessons.Remove(lesson);
            await db.SaveChangesAsync();
        }
    }

    public async Task RecordViewAsync(string userId, int lessonId, int courseId)
    {
        var history = new LessonHistory
        {
            UserId = userId,
            LessonId = lessonId,
            CourseId = courseId,
            ViewedAt = DateTime.UtcNow
        };

        db.LessonHistories.Add(history);
        await db.SaveChangesAsync();
    }

    public async Task<List<LessonHistory>> GetUserHistoryAsync(string userId, int page = 1, int pageSize = 20)
    {
        return await db.LessonHistories
            .Include(h => h.Lesson)
            .Include(h => h.Course)
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.ViewedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetUserHistoryCountAsync(string userId)
    {
        return await db.LessonHistories
            .Where(h => h.UserId == userId)
            .CountAsync();
    }
}
