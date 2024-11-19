using Microsoft.EntityFrameworkCore;
using WebDiaryAPI.Models;

namespace WebDiaryAPI.Data.Repository;

public class DiaryEntriesRepository : IDiaryEntriesRepository, IDisposable, IAsyncDisposable
{
    private readonly ApplicationDbContext _context;
    private bool _disposed = false;
    
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

        entry.Id = 0;
        
        await _context.DiaryEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DiaryEntry? entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _context.Entry(entry).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than zero.");
        }
        
        var entryToDelete = await _context.DiaryEntries.FindAsync(id);
        ArgumentNullException.ThrowIfNull(entryToDelete);
        
        _context.Entry(entryToDelete).State = EntityState.Deleted;
        await _context.SaveChangesAsync();
    }
    
    public bool EntryExists(int id) => 
        _context.DiaryEntries.Any(e => e.Id == id);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }
        
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _context.DisposeAsync().ConfigureAwait(false);
            
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}