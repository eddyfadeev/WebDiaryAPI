using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebDiaryAPI.Controllers;
using WebDiaryAPI.Data.Repository;
using WebDiaryAPI.Models;

namespace WebDiaryAPITests.Controllers;

public class DiaryEntriesControllerTests
{
    private Mock<IDiaryEntriesRepository> _mockRepository;
    private DiaryEntriesController _controller;
    private List<DiaryEntry> _testEntries;
    
    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IDiaryEntriesRepository>();
        _controller = new DiaryEntriesController(_mockRepository.Object);
        _testEntries = GetTestEntries();
    }

    #region GetDiaryEntriesAsync Tests
    
    [Test]
    public async Task GetDiaryEntriesAsync_ReturnsOkResult_WithEntries()
    {
        var expected = new List<DiaryEntry>(_testEntries).AsReadOnly();

        _mockRepository.Setup(repo => 
            repo.GetAllEntriesAsync()).ReturnsAsync(expected);
        
        var result = await _controller.GetDiaryEntriesAsync();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetDiaryEntriesAsync_ReturnsNotFound_WhenNoEntries()
    {
        _mockRepository.Setup(repo =>
            repo.GetAllEntriesAsync()).ReturnsAsync(new List<DiaryEntry>());

        var result = await _controller.GetDiaryEntriesAsync();
        
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
    
    #endregion

    #region GetEntryByIdAsync Tests
    
    [Test]
    public async Task GetEntryByIdAsync_ReturnsOkResult_WithEntry()
    {
        const int expectedId = 1;
        var expected = _testEntries[0];

        _mockRepository.Setup(repo =>
            repo.GetByIdAsync(expectedId)).ReturnsAsync(expected);

        var result = await _controller.GetEntryById(expectedId);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetEntryByIdAsync_ReturnsBadRequestResult_WithNegativeId()
    {
        const int invalidId = -1;
        
        _mockRepository.Setup(repo =>
            repo.GetByIdAsync(invalidId)).Throws<ArgumentOutOfRangeException>();

        var result = await _controller.GetEntryById(invalidId);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetEntryByIdAsync_ReturnsBadRequestResult_WhenIdIsZero()
    {
        const int invalidId = 0;
        
        _mockRepository.Setup(repo =>
            repo.GetByIdAsync(invalidId)).Throws<ArgumentOutOfRangeException>();

        var result = await _controller.GetEntryById(invalidId);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetEntryByIdAsync_ReturnsNotFoundResult()
    {
        const int expectedId = 1;
        
        _mockRepository.Setup(repo =>
            repo.GetByIdAsync(1)).ReturnsAsync((DiaryEntry?)default);

        var result = await _controller.GetEntryById(expectedId);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }
    
    #endregion

    #region CreateEntryAsync Tests

    [Test]
    public async Task CreateEntryAsync_ReturnsCreatedWithLocation()
    {
        var expected = _testEntries[0];
        
        _mockRepository.Setup(repo =>
            repo.AddAsync(expected)).Returns(Task.CompletedTask);

        var result = await _controller.CreateEntry(expected);
        
        Assert.That(result, Is.InstanceOf<CreatedResult>());
        var createdResult = result as CreatedResult;
        Assert.That(createdResult?.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task CreateEntryAsync_ReturnsBadRequestResult_WithNull()
    {
        _mockRepository.Setup(repo =>
            repo.AddAsync(default)).Throws<ArgumentNullException>();

        var result = await _controller.CreateEntry(default);
        
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    #endregion

    #region DeleteEntryAsync Tests

    [Test]
    public async Task DeleteEntryAsync_ReturnsNoContent_WhenSuccessfulDelete()
    {
        const int expectedId = 1;
        
        _mockRepository.Setup(repo =>
            repo.DeleteByIdAsync(expectedId)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteEntry(expectedId);
        
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteEntryAsync_ReturnsNotFound_WhenEntryDoesNotExist()
    {
        const int expectedId = 1;
        _mockRepository.Setup(repo =>
            repo.DeleteByIdAsync(expectedId)).Throws<ArgumentNullException>();

        var result = await _controller.DeleteEntry(expectedId);
        
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteEntryAsync_ReturnsBadRequest_WhenIdIsZero()
    {
        const int invalidId = 0;
        
        _mockRepository.Setup(repo =>
            repo.DeleteByIdAsync(invalidId)).Throws<ArgumentOutOfRangeException>();
        
        var result = await _controller.DeleteEntry(invalidId);
        
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task DeleteEntryAsync_ReturnsBadRequest_WithNegativeId()
    {
        const int invalidId = -1;
        
        _mockRepository.Setup(repo =>
            repo.DeleteByIdAsync(invalidId)).Throws<ArgumentOutOfRangeException>();
        
        var result = await _controller.DeleteEntry(invalidId);
        
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    #endregion

    #region UpdateEntryAsync Tests

    [Test]
    public async Task UpdateEntryAsync_ReturnsNoContent_WhenSuccessfulUpdate()
    {
        var entryToUpdate = _testEntries[0];

        _mockRepository.Setup(repo =>
            repo.UpdateAsync(entryToUpdate)).Returns(Task.CompletedTask);
        
        var result = await _controller.UpdateEntry(entryToUpdate.Id, entryToUpdate);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task UpdateEntryAsync_ReturnsBadRequest_WhenEntryIsNull()
    {
        _mockRepository.Setup(repo =>
            repo.UpdateAsync(null)).Returns(Task.CompletedTask);
            
        var result = await _controller.UpdateEntry(0, null);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task UpdateEntryAsync_ReturnsBadRequest_WhenIdMismatch()
    {
        const int invalidId = 2;
        var entryToUpdate = _testEntries[0];
        
        var result = await _controller.UpdateEntry(invalidId, entryToUpdate);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateEntryAsync_ReturnsNotFound_WhenNoMatchingEntryFound()
    {
        var testEntry = _testEntries[0];

        _mockRepository.Setup(repo =>
            repo.UpdateAsync(testEntry)).Throws<DbUpdateConcurrencyException>();
        _mockRepository.Setup(repo =>
            repo.EntryExists(testEntry.Id)).Returns(false);
        
        var result = await _controller.UpdateEntry(testEntry.Id, testEntry);
        
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
    
    [Test]
    public async Task UpdateEntryAsync_ReturnsInternalServerError_WhenMatchingEntryFound_WithDbException()
    {
        var testEntry = _testEntries[0];

        _mockRepository.Setup(repo =>
            repo.UpdateAsync(testEntry)).Throws<DbUpdateConcurrencyException>();
        _mockRepository.Setup(repo =>
            repo.EntryExists(testEntry.Id)).Returns(true);
        
        var result = await _controller.UpdateEntry(testEntry.Id, testEntry);
        
        Assert.That(result, Is.InstanceOf<StatusCodeResult>());
        var statusCode = result as StatusCodeResult;
        Assert.That(statusCode?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    #endregion

    private static List<DiaryEntry> GetTestEntries() =>
    [
        new ()
        {
            Id = 1,
            Title = "Great day!",
            Content = "The day was sunny",
            Created = DateTime.Now
        },
        new ()
        {
            Id = 2,
            Title = "Bad day!",
            Content = "The day was rainy",
            Created = DateTime.Now.AddDays(-1)
        }
    ];
}