using Microsoft.EntityFrameworkCore;
using WebDiaryAPI.Data;
using WebDiaryAPI.Data.Repository;
using WebDiaryAPI.Models;

namespace WebDiaryAPITests.Data.Repository;

public class DiaryEntriesRepositoryTests
{
    private ApplicationDbContext _dbContext;
    private DiaryEntriesRepository _repository;
    private IEnumerable<DiaryEntry> _testEntries;
    
    [SetUp]
    public async Task Setup()
    {
        _testEntries = GetTestEntries();
        _dbContext = GetInMemoryDbContext();
        
        await PopulateDb(_dbContext, _testEntries);
        
        _repository = new DiaryEntriesRepository(_dbContext);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _repository.DisposeAsync();
        await _dbContext.DisposeAsync();
    }

    #region GetByIdAsync Tests
    
    [Test]
    public async Task GetByIdAsync_ReturnsDiaryEntry_WhenExists()
    {
        const int testEntryId = 1;
        
        var expectedExists = 
            await _dbContext.DiaryEntries.AnyAsync(de => de.Id == testEntryId);
        
        var actual = 
            await _repository.GetByIdAsync(testEntryId);

        Assert.Multiple(() =>
        {
            Assert.That(expectedExists, Is.True);
            Assert.That(actual, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        const int invalidId = 3;
        
        await Assert.ThatAsync(() =>
            _repository.GetByIdAsync(invalidId), 
            Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_ArgumentOutOfRangeException_WhenIdIsZero()
    {
        const int invalidId = 0;
        
        await Assert.ThatAsync(() => 
            _repository.GetByIdAsync(invalidId), 
            Throws.InstanceOf<ArgumentOutOfRangeException>()
            );
    }
    
    [Test]
    public async Task GetByIdAsync_ArgumentOutOfRangeException_WhenIdIsNegative()
    {
        const int invalidId = -1;
        
        await Assert.ThatAsync(() => 
            _repository.GetByIdAsync(invalidId), 
            Throws.InstanceOf<ArgumentOutOfRangeException>()
        );
    }
    
    #endregion

    #region GetAllEntriesAsync Tests

    [Test]
    public async Task GetAllEntriesAsync_ReturnsListOfDiaryEntries()
    {
        const int expectedCount = 2;
        
        var rawResult = await _repository.GetAllEntriesAsync();
        var actual = rawResult.ToList();
        
        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.InstanceOf<IEnumerable<DiaryEntry>>());
            Assert.That(actual, Has.Count.EqualTo(expectedCount));
        });
    }

    #endregion
    
    #region AddAsyncTests

    [Test]
    public async Task AddAsync_AddsEntryToTheDatabase_WhenValidEntryPassed()
    {
        var expected = new DiaryEntry
        {
            Content = "This is a test entry",
            Title = "Test Entry",
            Created = DateTime.Now
        };
        
        await _repository.AddAsync(expected);
        var actual = await _dbContext.DiaryEntries.FindAsync(expected.Id);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task AddAsync_ThrowsNullException_WhenEntryIsNull()
    {
        await Assert.ThatAsync(() => 
            _repository.AddAsync(default),
            Throws.ArgumentNullException);
    }

    #endregion

    #region UpdateAsync Tests

    [Test]
    public async Task UpdateAsync_UpdatesEntry_WhenValidEntryPassed()
    {
        const int expectedId = 1;
        const string expectedContent = "New Content";
        
        var testEntry = await _dbContext.DiaryEntries.FindAsync(expectedId);
        Assert.That(testEntry, Is.Not.Null);
        
        var originalContent = testEntry.Content;
        
        testEntry.Content = expectedContent;
        
        await _repository.UpdateAsync(testEntry);
        var actualEntry = await _dbContext.DiaryEntries.FindAsync(expectedId);
        Assert.That(actualEntry, Is.Not.Null);
        
        var actualContent = actualEntry.Content;
    
        Assert.Multiple(() =>
        {
            Assert.That(originalContent, Is.Not.EqualTo(expectedContent));
            Assert.That(actualContent, Is.EqualTo(expectedContent));
        });
    }

    [Test]
    public async Task UpdateAsync_ThrowsArgumentNullException_WhenPassedEntryIsNull()
    {
        await Assert.ThatAsync(() =>
            _repository.UpdateAsync(default),
            Throws.ArgumentNullException);
    }

    #endregion

    #region DeleteByIdAsync Tests

    [Test]
    public async Task DeleteByIdAsync_DeletesEntry_WhenIdIsValid()
    {
        const int expectedId = 1;
        var expected = await _dbContext.DiaryEntries.FindAsync(expectedId);

        await _repository.DeleteByIdAsync(expectedId);
        var actual = await _dbContext.DiaryEntries.FindAsync(expectedId);
        
        Assert.Multiple(() =>
        {
            Assert.That(expected, Is.Not.Null);
            Assert.That(actual, Is.Null);
        });
    }

    [Test]
    public async Task DeleteByIdAsync_ThrowsArgumentNullException_WhenIdIsNotValid()
    {
        const int invalidId = 3;

        await Assert.ThatAsync(() =>
            _repository.DeleteByIdAsync(invalidId), Throws.ArgumentNullException);
    }

    [Test]
    public async Task DeleteByIdAsync_ThrowsOutOfRangeException_WhenIdIsZero()
    {
        const int invalidId = 0;
        
        await Assert.ThatAsync(() => 
            _repository.DeleteByIdAsync(invalidId), 
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }
    
    [Test]
    public async Task DeleteByIdAsync_ThrowsOutOfRangeException_WhenIdIsNegative()
    {
        const int invalidId = -1;
        
        await Assert.ThatAsync(() => 
                _repository.DeleteByIdAsync(invalidId), 
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    #endregion

    #region EntryExists Tests

    [Test]
    public async Task EntryExists_ReturnsFalse_WhenEntryDoesNotExist()
    {
        const int nonExistentId = -1;
        
        var result = await Task.Run(() => _repository.EntryExists(nonExistentId));
        
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task EntryExists_ReturnsTrue_WhenEntryExist()
    {
        const int existentId = 1;
        
        var result = await Task.Run(() => _repository.EntryExists(existentId));
        
        Assert.That(result, Is.True);
    }

    #endregion
    
    private static ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(
                databaseName: $"TestDb_{Guid.NewGuid().ToString()}")
            .Options;

        return new ApplicationDbContext(options);
    }
    
    private static IEnumerable<DiaryEntry> GetTestEntries() =>
    [
        new ()
        {
            Title = "What a day",
            Content = "Strange day",
            Created = DateTime.Now
        },
        new ()
        {
            Title = "Another day",
            Content = "Another strange day",
            Created = DateTime.Now
        }
    ];

    private static async Task PopulateDb(ApplicationDbContext dbContext, params IEnumerable<DiaryEntry> entries)
    {
        await dbContext.AddRangeAsync(entries);
        await dbContext.SaveChangesAsync();
    }
}