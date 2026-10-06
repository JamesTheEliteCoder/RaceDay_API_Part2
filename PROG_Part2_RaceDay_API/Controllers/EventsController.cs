using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.Models;
using PROG_Part2_RaceDay_API.DTOs;
using PROG_Part2_RaceDay_API.Filters;

namespace PROG_Part2_RaceDay_API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventsController(RaceDayDbContext context)
    {
        _context = context;
    }


    // Authenticated users to view the list of available events
    [HttpGet]
    [SessionAuthorize]
    [EndpointSummary("List of available events")]
    [EndpointDescription("Here you can view all the available RaceDay events. (Requires an authenticated session).")]
    [ProducesResponseType(typeof(List<Event>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<Event>>> GetEvents()
    {
        var events = await _context.Events
            .OrderBy(raceEvent => raceEvent.EventDate)
            .ToListAsync();

        return Ok(events);
    }



    // Public endpoint that returns one event, or error code 404 if it does not exist
    [HttpGet("{id:int}")]
    [SessionAuthorize]
    [EndpointSummary("Get an event by ID")]
    [EndpointDescription("Here you can view the details of the requested event. (An authenticated session is required) .")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Event>> GetEvent(int id)
    {
        var raceEvent = await _context.Events.FindAsync(id);

        if (raceEvent is null)
        {
            return NotFound();
        }

        return Ok(raceEvent);
    }

    // Organisers can create events. The organiser ID comes from the session.
    [HttpPost]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("Create an event")]
    [EndpointDescription("Creates an event for the signed-in Organiser. The request provides the event details, and the Organiser ID comes from the session.")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Event>> CreateEvent(CreateEventRequest request)
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId is null)
        {
            return Unauthorized();
        }

        var raceEvent = new Event
        {
            OrganiserId = organiserId.Value,
            Name = request.Name,
            Description = request.Description,
            EventDate = request.EventDate,
            Venue = request.Venue,
            City = request.City,
            Province = request.Province,
            DistanceKm = request.DistanceKm,
            EventType = request.EventType
        };

        _context.Events.Add(raceEvent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = raceEvent.EventId },
            raceEvent);
    }

    // Organisers can only update events that were created by them, or that belong to them
    [HttpPut("{id:int}")]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("Update an event")]
    [EndpointDescription("Updates an event managed by the signed-in Organiser. This request must include the event ID.")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Event>> UpdateEvent(
        int id,
        UpdateEventRequest request)
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId is null)
        {
            return Unauthorized();
        }

        var raceEvent = await _context.Events.FindAsync(id);

        if (raceEvent is null)
        {
            return NotFound();
        }

        if (raceEvent.OrganiserId != organiserId.Value)
        {
            return Forbid();
        }

        raceEvent.Name = request.Name;
        raceEvent.Description = request.Description;
        raceEvent.EventDate = request.EventDate;
        raceEvent.Venue = request.Venue;
        raceEvent.City = request.City;
        raceEvent.Province = request.Province;
        raceEvent.DistanceKm = request.DistanceKm;
        raceEvent.EventType = request.EventType;

        await _context.SaveChangesAsync();

        return Ok(raceEvent);
    }


    // Organisers can delete only their own events.
    [HttpDelete("{id:int}")]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("Delete an event")]
    [EndpointDescription("Deletes an event managed by the signed-in Organiser. (Deletion will be blocked while related records exist).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId is null)
        {
            return Unauthorized();
        }

        var raceEvent = await _context.Events.FindAsync(id);

        if (raceEvent is null)
        {
            return NotFound();
        }

        if (raceEvent.OrganiserId != organiserId.Value)
        {
            return Forbid();
        }

        _context.Events.Remove(raceEvent);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Related routes, categories, or enrolments can prevent deletion.
            return Conflict(new
            {
                message = "This event cannot be deleted while it has related records."
            });
        }

        return NoContent();
    }










} //end of class