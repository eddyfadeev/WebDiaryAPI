using Microsoft.EntityFrameworkCore;
using WebDiaryAPI.Data;
using WebDiaryAPI.Data.Repository;
using WebDiaryAPI.Models;

namespace WebDiaryAPITests.Data.Repository;

public class DiaryEntriesRepositoryTests
{
    private DiaryEntriesRepository _repository;
    private List<DiaryEntry> _testEntries;
    
    [SetUp]
    public async Task Setup()
    {
        _testEntries = 
        [
            new DiaryEntry
            {
                Title = "What a day",
                Content = "Strange day",
                Created = DateTime.Now
            },
            new DiaryEntry
            {
                Title = "Another day",
                Content = "Another strange day",
                Created = DateTime.Now
            }
        ];
        
        ApplicationDbContext dbContext = GetInMemoryDbContext();
        await dbContext.AddRangeAsync(_testEntries);
        await dbContext.SaveChangesAsync();
        
        _repository = new DiaryEntriesRepository(dbContext);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _repository.DisposeAsync();
    }

    #region GetByIdAsync Tests
    
    [Test]
    public async Task GetByIdAsync_ReturnsDiaryEntry_WhenExists()
    {
        var expected = _testEntries[0];

        await Assert.ThatAsync(() =>
            _repository.GetByIdAsync(expected.Id), 
            Is.EqualTo(expected)
            );
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        await Assert.ThatAsync(() =>
            _repository.GetByIdAsync(3), 
            Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_ArgumentOutOfRangeException_WhenIdIsZero()
    {
        await Assert.ThatAsync(() => 
            _repository.GetByIdAsync(0), 
            Throws.InstanceOf<ArgumentOutOfRangeException>()
            );
    }
    
    [Test]
    public async Task GetByIdAsync_ArgumentOutOfRangeException_WhenIdIsNegative()
    {
        await Assert.ThatAsync(() => 
            _repository.GetByIdAsync(-1), 
            Throws.InstanceOf<ArgumentOutOfRangeException>()
        );
    }
    
    #endregion

    #region GetAllEntriesAsync Tests

    [Test]
    public async Task GetAllEntriesAsync_ReturnsListOfDiaryEntries()
    {
        await Assert.ThatAsync(() => 
            _repository.GetAllEntriesAsync(),
            Is.TypeOf<List<DiaryEntry>>()
        );
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
}