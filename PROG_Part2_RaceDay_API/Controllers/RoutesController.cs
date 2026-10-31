using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.DTOs;
using PROG_Part2_RaceDay_API.Filters;
using PROG_Part2_RaceDay_API.Models;

namespace PROG_Part2_RaceDay_API.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/routes")]
public class RoutesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public RoutesController(RaceDayDbContext context)
    {
        _context = context;
    }

    // Organisers can add routes to their own events.
    [HttpPost]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("Create an event route")]
    [EndpointDescription("Creates a route for an event managed by the signed-in Organiser.")]
    [ProducesResponseType(typeof(RaceRoute), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RaceRoute>> CreateRoute(
        int eventId,
        RaceRouteRequest request)
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
            // Return 403 directly because this API uses custom session authorisation.
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var route = new RaceRoute
        {
            EventId = eventId,
            RouteName = request.RouteName,
            AreasCovered = request.AreasCovered,
            DistanceKm = request.DistanceKm,
            StartLocation = request.StartLocation,
            FinishLocation = request.FinishLocation,
            Description = request.Description,
            MapUrl = request.MapUrl
        };

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(UpdateRoute),
            new { eventId, routeId = route.RouteId },
            route);
    }

    // Organisers can update only routes belonging to their own events.
    [HttpPut("{routeId:int}")]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("Update an event route")]
    [EndpointDescription("Updates a route belonging to an event managed by the signed-in Organiser.")]
    [ProducesResponseType(typeof(RaceRoute), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RaceRoute>> UpdateRoute(
        int eventId,
        int routeId,
        RaceRouteRequest request)
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
            // Return 403 directly because this API uses custom session authorisation.
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var route = await _context.Routes.FirstOrDefaultAsync(
            item => item.RouteId == routeId &&
                    item.EventId == eventId);

        if (route is null)
        {
            return NotFound();
        }

        route.RouteName = request.RouteName;
        route.AreasCovered = request.AreasCovered;
        route.DistanceKm = request.DistanceKm;
        route.StartLocation = request.StartLocation;
        route.FinishLocation = request.FinishLocation;
        route.Description = request.Description;
        route.MapUrl = request.MapUrl;

        await _context.SaveChangesAsync();

        return Ok(route);
    }



    // Authenticated users can view routes belonging to a specific event.
    [HttpGet]
    [SessionAuthorize]
    [EndpointSummary("List routes for an event")]
    [EndpointDescription("Returns all routes associated with the requested event. Requires an authenticated session.")]
    [ProducesResponseType(typeof(List<RaceRoute>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<RaceRoute>>> GetRoutes(int eventId)
    {
        // Confirm that the requested event exists.
        var eventExists = await _context.Events
            .AnyAsync(raceEvent => raceEvent.EventId == eventId);

        if (!eventExists)
        {
            return NotFound();
        }

        // Retrieve routes associated with this event only.
        var routes = await _context.Routes
            .Where(route => route.EventId == eventId)
            .OrderBy(route => route.RouteId)
            .ToListAsync();

        return Ok(routes);
    }


}//end of class.