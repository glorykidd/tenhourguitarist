using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data;
using TenHourGuitarist.Data.Entities;
using TenHourGuitarist.Services.Interfaces;

namespace TenHourGuitarist.Services;

public class ContactService(ApplicationDbContext db) : IContactService
{
    public async Task<List<ContactMessage>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        return await db.ContactMessages
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await db.ContactMessages.CountAsync();
    }

    public async Task<ContactMessage?> GetByIdAsync(int id)
    {
        return await db.ContactMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<ContactMessage> CreateAsync(ContactMessage message)
    {
        db.ContactMessages.Add(message);
        await db.SaveChangesAsync();
        return message;
    }

    public async Task DeleteAsync(int id)
    {
        var message = await db.ContactMessages.FindAsync(id);
        if (message is not null)
        {
            db.ContactMessages.Remove(message);
            await db.SaveChangesAsync();
        }
    }
}
