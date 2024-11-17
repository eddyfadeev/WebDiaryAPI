using WebDiaryAPI.Models;

namespace WebDiaryAPI.Data.Repository;

public interface IDiaryEntriesRepository
{
    Task<DiaryEntry?> GetByIdAsync(int id);
    Task<IEnumerable<DiaryEntry>> GetAllEntriesAsync();
    Task AddAsync(DiaryEntry? entry);
    Task UpdateAsync(DiaryEntry? entry);
    Task DeleteAsync(DiaryEntry? entry);
    Task DeleteByIdAsync(int id);
}