using Microsoft.EntityFrameworkCore;
using WebDiaryAPI.Models;

namespace WebDiaryAPI.Data.Repository;

public class DiaryEntriesRepository : IDiaryEntriesRepository
{
    private readonly ApplicationDbContext _context;
    
    public DiaryEntriesRepository(ApplicationDbContext context) => 
        _context = context;
    
    public async Task<DiaryEntry?> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than zero.");
        }

        return await _context.DiaryEntries.FindAsync(id);
    }

    public async Task<IEnumerable<DiaryEntry>> GetAllEntriesAsync() =>
        await _context.DiaryEntries.ToListAsync();

    public async Task AddAsync(DiaryEntry? entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Id != 0)
        {
#pragma warning disable
            throw new ArgumentOutOfRangeException(nameof(entry.Id), "Id must be zero or not be set when adding a new entry.");
#pragma warning restore
        }
        
        await _context.DiaryEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DiaryEntry? entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _context.DiaryEntries.Update(entry);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(DiaryEntry? entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _context.DiaryEntries.Remove(entry);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than zero.");
        }
        
        var entryToDelete = await _context.DiaryEntries.FirstOrDefaultAsync(de => de.Id == id);
        ArgumentNullException.ThrowIfNull(entryToDelete);
        
        await DeleteAsync(entryToDelete);
    }
}