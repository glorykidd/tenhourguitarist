using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class SubjectService(ApplicationDbContext db) : ISubjectService
{
    public async Task<List<Subject>> GetAllAsync()
    {
        return await db.Subjects
            .OrderBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Subject?> GetByIdAsync(int id)
    {
        return await db.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Subject> CreateAsync(Subject subject)
    {
        db.Subjects.Add(subject);
        await db.SaveChangesAsync();
        return subject;
    }

    public async Task UpdateAsync(Subject subject)
    {
        var existing = await db.Subjects.FindAsync(subject.Id);
        if (existing is null) return;
        db.Entry(existing).CurrentValues.SetValues(subject);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var subject = await db.Subjects.FindAsync(id);
        if (subject is not null)
        {
            db.Subjects.Remove(subject);
            await db.SaveChangesAsync();
        }
    }
}
