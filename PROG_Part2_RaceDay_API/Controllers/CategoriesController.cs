using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.Models;

namespace PROG_Part2_RaceDay_API.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public CategoriesController(RaceDayDbContext context)
    {
        _context = context;
    }

    // Public endpoint that lists the categories for an event.
    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetCategories(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(raceEvent => raceEvent.EventId == eventId);

        if (!eventExists)
        {
            return NotFound();
        }

        var categories = await _context.Categories
            .Where(category => category.EventId == eventId)
            .OrderBy(category => category.CategoryId)
            .ToListAsync();

        return Ok(categories);
    }
}