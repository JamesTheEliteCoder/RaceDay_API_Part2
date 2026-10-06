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

    // Organisers can only update events that were created by them, or that belong to them
    [HttpPut("{id:int}")]
    [SessionAuthorize("Organiser")]
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


} //end of class