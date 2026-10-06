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

    // Authenticated users can view categories for a specific event
    [HttpGet]
    [SessionAuthorize]
    [EndpointSummary("List categories for an event")]
    [EndpointDescription("Returns all categories for the requested event. (Requires an authenticated session).")]
    [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [EndpointSummary("Create an event category")]
    [EndpointDescription("Creates an age or distance category for an event managed by the signed-in Organiser. The request includes the category details and route ID.")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        var route = await _context.Routes.FindAsync(request.RouteId);

        if (route is null)
        {
            return NotFound("The route does not exist.");
        }

        if (route.EventId != eventId)
        {
            return BadRequest("The selected route does not belong to this event.");
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