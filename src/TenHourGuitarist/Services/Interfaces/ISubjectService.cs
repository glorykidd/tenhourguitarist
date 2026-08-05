using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Services.Interfaces;

public interface ISubjectService
{
    Task<List<Subject>> GetAllAsync();
    Task<Subject?> GetByIdAsync(int id);
    Task<Subject> CreateAsync(Subject subject);
    Task UpdateAsync(Subject subject);
    Task DeleteAsync(int id);
}
