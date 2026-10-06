using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.DTOs;
using PROG_Part2_RaceDay_API.Filters;
using PROG_Part2_RaceDay_API.Models;

namespace PROG_Part2_RaceDay_API.Controllers;

[ApiController]
[Route("api/enrolment/{enrolmentId:int}/result")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ResultsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // Organisers can record a result only for an enrolment in their own event.
    [HttpPost]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("Record an event result")]
    [EndpointDescription("Records the finishing position and finish time for an enrolment in an event managed by the signed-in Organiser.")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result>> RecordResult(
        int enrolmentId,
        RecordResultRequest request)
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId is null)
        {
            return Unauthorized();
        }

        var enrolment = await _context.Enrolments.FindAsync(enrolmentId);

        if (enrolment is null)
        {
            return NotFound();
        }

        var raceEvent = await _context.Events.FindAsync(enrolment.EventId);

        if (raceEvent is null)
        {
            return NotFound();
        }

        if (raceEvent.OrganiserId != organiserId.Value)
        {
            return Forbid();
        }

        var resultExists = await _context.Results.AnyAsync(
            result => result.EnrolmentId == enrolmentId);

        if (resultExists)
        {
            return Conflict("A result already exists for this enrolment.");
        }

        var result = new Result
        {
            EnrolmentId = enrolmentId,
            FinishingPosition = request.FinishingPosition,
            FinishTime = request.FinishTime
        };

        _context.Results.Add(result);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // The database unique constraint also prevents duplicate results.
            return Conflict("A result already exists for this enrolment.");
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }
}