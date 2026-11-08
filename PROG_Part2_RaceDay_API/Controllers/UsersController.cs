using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.DTOs;
using PROG_Part2_RaceDay_API.Filters;
using PROG_Part2_RaceDay_API.Models;
using PROG_Part2_RaceDay_API.Services;


namespace PROG_Part2_RaceDay_API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IBlobStorageService _blobStorageService;
    private const long MaxImageSize = 12 * 1024 * 1024; // 12MB
    public UsersController(
        RaceDayDbContext db,
        IBlobStorageService blobStorageService)
    {
        _db = db;
        _blobStorageService = blobStorageService;
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
            DateOfBirth = user.DateOfBirth,
            ProfilePictureUrl = user.ProfilePictureUrl
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
           DateOfBirth = user.DateOfBirth,
           ProfilePictureUrl = user.ProfilePictureUrl
       });
    } //end of UpdateMyProfile method







    // Uploads a profile picture for the currently signed-in user.
    [HttpPost("me/profile-picture")]
    [SessionAuthorize]
    [EndpointSummary("Upload my profile picture")]
    [EndpointDescription("Uploads a profile picture for the signed-in user.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile? file)
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

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Please select an image." });
        }

        if (file.Length > MaxImageSize)
        {
            return BadRequest(new { message = "Image size cannot exceed 12 MB." });
        }

        var allowedContentTypes = new[]
        {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { message = "Only JPEG, PNG and WebP images are allowed." });
        }

        await using var imageStream = file.OpenReadStream();

        if (!await IsValidImageSignature(imageStream, file.ContentType))
        {
            return BadRequest(new { message = "The uploaded file is not a valid supported image." });
        }

        imageStream.Position = 0;

        var imageUrl = await _blobStorageService.UploadImageAsync(
            imageStream,
            file.FileName,
            file.ContentType,
            "profile-pictures");

        user.ProfilePictureUrl = imageUrl;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Profile picture uploaded successfully.",
            profilePictureUrl = user.ProfilePictureUrl
        });
    }// end of UploadProfilePicture method







    // Checks the actual file signature to verify that the uploaded file is an image.
    private static async Task<bool> IsValidImageSignature(
        Stream imageStream,
        string contentType)
    {
        byte[] header = new byte[12];

        int bytesRead = await imageStream.ReadAsync(header.AsMemory(0, header.Length));

        // Reset the stream so it can be read again during the Azure upload.
        imageStream.Position = 0;

        return contentType.ToLowerInvariant() switch
        {
            // JPEG signature: FF D8 FF
            "image/jpeg" =>
                bytesRead >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF,

            // PNG signature: 89 50 4E 47 0D 0A 1A 0A
            "image/png" =>
                bytesRead >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47 &&
                header[4] == 0x0D &&
                header[5] == 0x0A &&
                header[6] == 0x1A &&
                header[7] == 0x0A,

            // WebP signature: RIFF....WEBP
            "image/webp" =>
                bytesRead >= 12 &&
                header[0] == 0x52 &&
                header[1] == 0x49 &&
                header[2] == 0x46 &&
                header[3] == 0x46 &&
                header[8] == 0x57 &&
                header[9] == 0x45 &&
                header[10] == 0x42 &&
                header[11] == 0x50,

            _ => false
        };
    }// end of IsValidImageSignature method






    // Retrieves the profile picture of the currently signed-in user.
    [HttpGet("me/profile-picture")]
    [SessionAuthorize]
    [EndpointSummary("View my profile picture")]
    [EndpointDescription("Returns the private profile picture belonging to the signed-in user.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfilePicture()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _db.Users.FindAsync(userId.Value);

        if (user is null || string.IsNullOrWhiteSpace(user.ProfilePictureUrl))
        {
            return NotFound();
        }

        var image = await _blobStorageService.GetImageAsync(
            user.ProfilePictureUrl);

        if (image is null)
        {
            return NotFound();
        }

        return File(
            image.Value.Content,
            image.Value.ContentType);
    }// end of GetMyProfilePicture method









    // Participants can view only their own enrolments.
    [HttpGet("me/enrolments")]
    [SessionAuthorize("Participant")]
    [EndpointSummary("View my enrolments")]
    [EndpointDescription("Returns enrolments belonging to the signed-in Participant.")]
    [ProducesResponseType(typeof(List<EnrolmentResponse>), StatusCodes.Status200OK)]
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