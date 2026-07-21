using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class CourseService(ApplicationDbContext db) : ICourseService
{
    public async Task<List<Course>> GetAllAsync(bool publishedOnly = true)
    {
        var query = db.Courses
            .Include(c => c.Instructor)
            .AsNoTracking();

        if (publishedOnly)
        {
            query = query.Where(c => c.IsPublished && !c.IsFree);
        }

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await db.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Lessons.OrderBy(l => l.DisplayOrder))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Course?> GetBySlugAsync(string slug)
    {
        return await db.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Lessons.OrderBy(l => l.DisplayOrder))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<List<Course>> SearchAsync(string? query, CourseLevel? level, int page = 1, int pageSize = 12)
    {
        var dbQuery = db.Courses
            .Include(c => c.Instructor)
            .Where(c => c.IsPublished && !c.IsFree)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            dbQuery = dbQuery.Where(c => c.Name.Contains(query));
        }

        if (level.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.Level == level.Value);
        }

        return await dbQuery
            .OrderBy(c => c.DisplayOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? query, CourseLevel? level)
    {
        var dbQuery = db.Courses
            .Where(c => c.IsPublished && !c.IsFree)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            dbQuery = dbQuery.Where(c => c.Name.Contains(query));
        }

        if (level.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.Level == level.Value);
        }

        return await dbQuery.CountAsync();
    }

    public async Task<Course> CreateAsync(Course course)
    {
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return course;
    }

    public async Task UpdateAsync(Course course)
    {
        var existing = await db.Courses.FindAsync(course.Id);
        if (existing is null) return;
        db.Entry(existing).CurrentValues.SetValues(course);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var course = await db.Courses.FindAsync(id);
        if (course is not null)
        {
            db.Courses.Remove(course);
            await db.SaveChangesAsync();
        }
    }
}
