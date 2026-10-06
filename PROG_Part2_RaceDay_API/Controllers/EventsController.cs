using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.Models;

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






} //end of class