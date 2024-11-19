using Microsoft.AspNetCore.Mvc;
using Moq;
using WebDiaryAPI.Controllers;
using WebDiaryAPI.Data.Repository;
using WebDiaryAPI.Models;

namespace WebDiaryAPITests.Controllers;

public class DiaryEntriesControllerTests
{
    private List<DiaryEntry> _testEntries;
    private Mock<IDiaryEntriesRepository> _mockRepository;
    private DiaryEntriesController _controller;
    
    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IDiaryEntriesRepository>();
        _controller = new DiaryEntriesController(_mockRepository.Object);
        _testEntries = 
        [
            new DiaryEntry
            {
                Id = 1, 
                Title = "Great day!", 
                Content = "The day was sunny", 
                Created = DateTime.Now
            },
            new DiaryEntry
            {
                Id = 2,
                Title = "Bad day!",
                Content = "The day was rainy",
                Created = DateTime.Now.AddDays(-1)
            }
        ];
    }

    [Test]
    public async Task GetDiaryEntriesAsync_ReturnsOkResultWithEntries()
    {
        var entries = new List<DiaryEntry>(_testEntries).AsReadOnly();

        _mockRepository.Setup(repo => 
            repo.GetAllEntriesAsync()).ReturnsAsync(entries);
        
        var result = await _controller.GetDiaryEntriesAsync();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.EqualTo(entries));
    }
}