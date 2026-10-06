using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.Models;
using PROG_Part2_RaceDay_API.DTOs;
using PROG_Part2_RaceDay_API.Filters;

namespace PROG_Part2_RaceDay_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // Public endpoint that lists events in date order.
    [HttpGet]
    public async Task<ActionResult<List<Event>>> GetEvents()
    {
        var events = await _context.Events
            .OrderBy(raceEvent => raceEvent.EventDate)
            .ToListAsync();

        return Ok(events);
    }



    // Public endpoint that returns one event, or error code 404 if it does not exist
    [HttpGet("{id:int}")]
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




} //end of class