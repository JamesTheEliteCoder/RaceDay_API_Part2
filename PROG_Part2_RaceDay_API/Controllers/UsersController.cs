using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.DTOs;
using PROG_Part2_RaceDay_API.Filters;
using PROG_Part2_RaceDay_API.Models;


namespace PROG_Part2_RaceDay_API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    [SessionAuthorize]
    [EndpointSummary("View my profile")]
    [EndpointDescription("Returns the profile of the signed-in user without exposing the password hash.")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetMyProfile()
    { //start of GetMyProfile method
        var userId = HttpContext.Session.GetInt32("UserId");

        // The filter checks the authentication which also keeps the action safe if it's reused
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _db.Users.FindAsync(userId.Value);

        if (user is null)
        {
            return NotFound(new { message = "User profile not found." });
        }

        // Return the profile DTO so that the PasswordHash is never exposed
        return Ok(new UserResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth
        });
    } //end of GetMyProfile method


    [HttpPut("me")]

    [SessionAuthorize] //to allow any logged in profile to be able to update their profile
    [EndpointSummary("Update my profile")]
    [EndpointDescription("Updates the signed-in user's first name, last name, phone number, and date of birth.")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateMyProfile(
    UpdateProfileRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _db.Users.FindAsync(userId.Value);

        if (user is null)
        {
            return NotFound(new { message = "User profile not found." });
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();
        user.DateOfBirth = request.DateOfBirth!.Value;

        await _db.SaveChangesAsync();

        return Ok(new UserResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth
        });
    }




    // Participants can view only their own enrolments.
    [HttpGet("me/enrolments")]
    [SessionAuthorize("Participant")]
    [EndpointSummary("View my enrolments")]
    [EndpointDescription("Returns enrolments belonging to the signed-in Participant.")]
    [ProducesResponseType(typeof(List<Enrolment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<Enrolment>>> GetMyEnrolments()
    {
        var participantId = HttpContext.Session.GetInt32("UserId");

        if (participantId is null)
        {
            return Unauthorized();
        }

        var enrolments = await _db.Enrolments
            .Where(enrolment => enrolment.ParticipantId == participantId.Value)
            .OrderByDescending(enrolment => enrolment.EnrolmentDate)
            .ToListAsync();

        return Ok(enrolments);
    }




    // Participants can view only results linked to their own enrolments.
    [HttpGet("me/results")]
    [SessionAuthorize("Participant")]
    [EndpointSummary("View my results")]
    [EndpointDescription("Returns results linked to the signed-in Participant's enrolments.")]
    [ProducesResponseType(typeof(List<Result>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<Result>>> GetMyResults()
    {
        var participantId = HttpContext.Session.GetInt32("UserId");

        if (participantId is null)
        {
            return Unauthorized();
        }

        var results = await (
            from result in _db.Results
            join enrolment in _db.Enrolments
                on result.EnrolmentId equals enrolment.EnrolmentId
            where enrolment.ParticipantId == participantId.Value
            orderby result.FinishingPosition
            select result
        ).ToListAsync();

        return Ok(results);
    }










}// end of class