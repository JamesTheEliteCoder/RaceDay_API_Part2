using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.Models;
using PROG_Part2_RaceDay_API.DTOs;
using PROG_Part2_RaceDay_API.Filters;

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




    // Organisers can add categories to their own events.
    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<ActionResult<Category>> CreateCategory(
        int eventId,
        CreateCategoryRequest request)
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId is null)
        {
            return Unauthorized();
        }

        var raceEvent = await _context.Events.FindAsync(eventId);

        if (raceEvent is null)
        {
            return NotFound();
        }

        if (raceEvent.OrganiserId != organiserId.Value)
        {
            return Forbid();
        }

        if (request.CategoryType == "Age" &&
            request.MinimumAge is null &&
            request.MaximumAge is null)
        {
            return BadRequest("An age category needs a minimum age, maximum age, or both.");
        }

        if (request.MinimumAge is not null &&
            request.MaximumAge is not null &&
            request.MinimumAge > request.MaximumAge)
        {
            return BadRequest("Minimum age cannot be greater than maximum age.");
        }

        if (request.CategoryType == "Distance" &&
            (request.MinimumAge is not null || request.MaximumAge is not null))
        {
            return BadRequest("Distance categories cannot have age limits.");
        }

        var routeBelongsToEvent = await _context.Routes.AnyAsync(
            route => route.RouteId == request.RouteId &&
                     route.EventId == eventId);

        if (!routeBelongsToEvent)
        {
            return BadRequest("The selected route must belong to this event.");
        }

        var category = new Category
        {
            EventId = eventId,
            RouteId = request.RouteId,
            Name = request.Name,
            CategoryType = request.CategoryType,
            MinimumAge = request.MinimumAge,
            MaximumAge = request.MaximumAge
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategories),
            new { eventId },
            category);
    }












}// end of class