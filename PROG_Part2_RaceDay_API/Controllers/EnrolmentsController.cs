using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.DTOs;
using PROG_Part2_RaceDay_API.Filters;
using PROG_Part2_RaceDay_API.Models;

namespace PROG_Part2_RaceDay_API.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/enrolments")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrolmentsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // Participants enrol in an event by selecting one of its categories.
    [HttpPost]
    [SessionAuthorize("Participant")]
    [EndpointSummary("Enrol in an event")]
    [EndpointDescription("Enrols the signed-in Participant in the event using the selected category ID.")]
    [ProducesResponseType(typeof(Enrolment), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Enrolment>> EnrolInEvent(
        int eventId,
        CreateEnrolmentRequest request)
    {
        var participantId = HttpContext.Session.GetInt32("UserId");

        if (participantId is null)
        {
            return Unauthorized();
        }

        var raceEvent = await _context.Events.FindAsync(eventId);

        if (raceEvent is null)
        {
            return NotFound("The event does not exist.");
        }

        var category = await _context.Categories.FindAsync(request.CategoryId);

        if (category is null)
        {
            return NotFound("The category does not exist.");
        }

        if (category.EventId != eventId)
        {
            return BadRequest("The selected category does not belong to this event.");
        }

        var alreadyEnrolled = await _context.Enrolments.AnyAsync(enrolment =>
            enrolment.ParticipantId == participantId.Value &&
            enrolment.EventId == eventId &&
            enrolment.CategoryId == request.CategoryId);

        if (alreadyEnrolled)
        {
            return Conflict("You are already enrolled in this event category.");
        }

        var enrolment = new Enrolment
        {
            ParticipantId = participantId.Value,
            EventId = eventId,
            CategoryId = request.CategoryId,
            EnrolmentDate = DateTime.Now,
            Status = "Confirmed"
        };

        _context.Enrolments.Add(enrolment);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // The database unique constraint also protects against duplicate requests.
            return Conflict("You are already enrolled in this event category.");
        }

        return Created($"/api/events/{eventId}/enrolments", enrolment);
    }



    // Organisers can view enrolments only for events they manage.
    [HttpGet]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("List enrolments for an event")]
    [EndpointDescription("Returns enrolments for an event managed by the signed-in Organiser.")]
    [ProducesResponseType(typeof(List<Enrolment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Enrolment>>> GetEventEnrolments(int eventId)
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

        var enrolments = await _context.Enrolments
            .Where(enrolment => enrolment.EventId == eventId)
            .OrderBy(enrolment => enrolment.EnrolmentId)
            .ToListAsync();

        return Ok(enrolments);
    }






}//end of class