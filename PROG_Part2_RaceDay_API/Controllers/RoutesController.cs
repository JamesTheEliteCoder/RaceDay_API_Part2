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
            return Forbid();
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
            return Forbid();
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
}