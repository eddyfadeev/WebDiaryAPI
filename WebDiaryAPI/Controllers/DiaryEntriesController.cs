using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDiaryAPI.Data.Repository;
using WebDiaryAPI.Models;

namespace WebDiaryAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DiaryEntriesController : ControllerBase
{
    private readonly IDiaryEntriesRepository _repository;

    public DiaryEntriesController(IDiaryEntriesRepository repository) =>
        _repository = repository;

    [HttpGet("all")]
    public async Task<IActionResult> GetDiaryEntriesAsync()
    { 
        var result = await _repository.GetAllEntriesAsync();
        
        if (!result.Any())
        {
            return NotFound();
        }
        
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEntryById([FromRoute]int id)
    {
        DiaryEntry? entry = default;

        try
        {
            entry = await _repository.GetByIdAsync(id);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }

        if (entry is null)
        {
            return NotFound("No matching entry found.");
        }

        return Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEntry([FromBody]DiaryEntry entry)
    {
        try
        {
            await _repository.AddAsync(entry);
        }
        catch (ArgumentNullException)
        {
            ModelState.AddModelError("entry", "Entry cannot be null");
        }
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var uri = new Uri($"api/DiaryEntries/{entry.Id}", UriKind.Relative);

        return Created(uri, entry);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEntry([FromRoute] int id)
    {
        try
        {
            await _repository.DeleteByIdAsync(id);
        }
        catch (ArgumentNullException)
        {
            return NotFound("No entry found with given Id. Delete failed.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ModelState.AddModelError("entry", ex.Message);
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEntry(int id, [FromBody] DiaryEntry entry)
    {
        if (id != entry.Id)
        {
            ModelState.AddModelError("id", "Id in the URL does not match Id in the body");
        }
        else
        {
            try
            {
                await _repository.UpdateAsync(entry);
            }
            catch (ArgumentNullException ex)
            {
                ModelState.AddModelError("entry", ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                return _repository.EntryExists(id) switch
                {
                    true => NotFound(),
                    false => StatusCode(StatusCodes.Status500InternalServerError)
                };
            }
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        return NoContent();
    }
}