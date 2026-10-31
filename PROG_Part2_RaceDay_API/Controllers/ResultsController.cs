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
            // Return 403 directly because this API uses custom session authorisation.
            return StatusCode(StatusCodes.Status403Forbidden);
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
    } //End of RecordResult Method.



    // Organisers can view results only for their own events.
    [HttpGet("/api/events/{eventId:int}/results")]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("List results for an event")]
    [EndpointDescription("Retrieves all recorded results for an event managed by the signed-in Organiser.")]
    [ProducesResponseType(typeof(List<EventResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<EventResultResponse>>> GetEventResults(
        int eventId)
    {
        // Retrieve the signed-in organiser's ID from the API session.
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId is null)
        {
            return Unauthorized();
        }

        // Find the requested event.
        var raceEvent = await _context.Events.FindAsync(eventId);

        if (raceEvent is null)
        {
            return NotFound();
        }

        // Make sure the event belongs to the signed-in organiser.
        if (raceEvent.OrganiserId != organiserId.Value)
        {
            // Return 403 directly because this API uses custom session authorisation.
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        // Join results to enrolments, participants and categories.
        var results = await (
            from result in _context.Results
            join enrolment in _context.Enrolments
                on result.EnrolmentId equals enrolment.EnrolmentId
            join participant in _context.Users
                on enrolment.ParticipantId equals participant.UserId
            join category in _context.Categories
                on enrolment.CategoryId equals category.CategoryId
            where enrolment.EventId == eventId
            orderby result.FinishingPosition
            select new EventResultResponse
            {
                ResultId = result.ResultId,
                EnrolmentId = enrolment.EnrolmentId,
                ParticipantId = participant.UserId,
                ParticipantName = participant.FirstName + " " + participant.LastName,
                CategoryName = category.Name,
                FinishingPosition = result.FinishingPosition,
                FinishTime = result.FinishTime
            }
        ).ToListAsync();

        return Ok(results);
    }//End of GetEventResults Method.




    // Organisers can update results only for enrolments in their own events.
    [HttpPut]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("Update an event result")]
    [EndpointDescription("Updates the finishing position and finish time for an enrolment in an event managed by the signed-in Organiser.")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> UpdateResult(
        int enrolmentId,
        RecordResultRequest request)
    {
        // Retrieve the signed-in organiser's ID from the API session.
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId is null)
        {
            return Unauthorized();
        }

        // Find the enrolment associated with this result.
        var enrolment = await _context.Enrolments.FindAsync(enrolmentId);

        if (enrolment is null)
        {
            return NotFound();
        }

        // Find the event associated with the enrolment.
        var raceEvent = await _context.Events.FindAsync(enrolment.EventId);

        if (raceEvent is null)
        {
            return NotFound();
        }

        // Make sure the event belongs to the signed-in organiser.
        if (raceEvent.OrganiserId != organiserId.Value)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        // Find the existing result for this enrolment.
        var result = await _context.Results.FirstOrDefaultAsync(
            item => item.EnrolmentId == enrolmentId);

        if (result is null)
        {
            return NotFound();
        }

        // Update the finishing position and finish time.
        result.FinishingPosition = request.FinishingPosition;
        result.FinishTime = request.FinishTime;

        await _context.SaveChangesAsync();

        return Ok(result);

    }//End of UpdateResult Method.





} //End of Class