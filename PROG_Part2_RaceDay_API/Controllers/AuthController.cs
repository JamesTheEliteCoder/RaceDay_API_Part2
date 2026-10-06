using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.DTOs;
using PROG_Part2_RaceDay_API.Models;

namespace PROG_Part2_RaceDay_API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(
        RaceDayDbContext db,
        IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("register")]
    [EndpointSummary("Register an account")]
    [EndpointDescription("Creates an account for a Participant or Organiser and stores a hashed password.")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        // Normalise email so that the registration and login use the same format
        var email = request.Email.Trim().ToLowerInvariant();

        // Return 409 before the database's unique email index rejects a duplicate
        if (await _db.Users.AnyAsync(user => user.Email == email))
        {
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = request.Role,
            PhoneNumber = request.PhoneNumber.Trim(),

            // The Controller returns 400 if required request fields are invalid
            DateOfBirth = request.DateOfBirth!.Value
        };

        // To store only the hash and never the submitted password
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Return the safe response DTO (which does not include PasswordHash)
        return StatusCode(StatusCodes.Status201Created, ToResponse(user));
    }

    [HttpPost("login")]
    [EndpointSummary("Log in")]
    [EndpointDescription("Verifies the user's credentials and creates a server-side session.")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> Login(LoginRequest request)
    {
        // Use the same email normalisation as registration
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(user => user.Email == email);

        // Use the same response for unknown emails and incorrect passwords
        if (user is null ||
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        // Session values stay server-side, while the browser receives the session cookie
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Role", user.Role);

        return Ok(ToResponse(user));
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth
        };
    }
}