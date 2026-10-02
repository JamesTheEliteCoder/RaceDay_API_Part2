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
    public async Task<ActionResult<UserResponse>> GetMyProfile()
    {
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
    }
}