using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_Part2_RaceDay_API.Data;
using PROG_Part2_RaceDay_API.DTOs;
using PROG_Part2_RaceDay_API.Filters;
using PROG_Part2_RaceDay_API.Models;
using PROG_Part2_RaceDay_API.Services;
using System.IO;
using System.Linq;

namespace PROG_Part2_RaceDay_API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB
    private readonly RaceDayDbContext _context;
    private readonly IBlobStorageService _blobStorageService;

    public EventsController(
     RaceDayDbContext context,
     IBlobStorageService blobStorageService)
    {
        _context = context;
        _blobStorageService = blobStorageService;
    }


    // Authenticated users to view the list of available events
    [HttpGet]
    [SessionAuthorize]
    [EndpointSummary("List of available events")]
    [EndpointDescription("Here you can view all the available RaceDay events. (Requires an authenticated session).")]
    [ProducesResponseType(typeof(List<Event>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<Event>>> GetEvents()
    {
        var events = await _context.Events
            .OrderBy(raceEvent => raceEvent.EventDate)
            .ToListAsync();

        return Ok(events);
    } //end of GetEvents method.


    // Retrieve an event banner from private Azure Blob Storage.
    [HttpGet("{id:int}/banner")]
    [SessionAuthorize]
    [EndpointSummary("Get an event banner")]
    [EndpointDescription(
        "Retrieves an event's banner image from private Azure Blob Storage.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBanner(int id)
    {
        // Find the requested event.
        var raceEvent = await _context.Events.FindAsync(id);

        if (raceEvent is null || string.IsNullOrWhiteSpace(raceEvent.BannerImageUrl))
        {
            return NotFound();
        }

        // Retrieve the image from Azure Blob Storage.
        var image = await _blobStorageService.GetImageAsync(
            raceEvent.BannerImageUrl);

        if (image is null)
        {
            return NotFound();
        }

        // Return the image using its stored content type.
        return File(image.Value.Content, image.Value.ContentType);
    }

    // Organisers can view only the events that belong to them.
    [HttpGet("mine")]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("List my events")]
    [EndpointDescription(
        "Returns events belonging to the signed-in Organiser.")]
    [ProducesResponseType(typeof(List<Event>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<Event>>> GetMyEvents()
    {
        // Retrieve the organiser's ID from the authenticated API session.
        var organiserId = HttpContext.Session.GetInt32("UserId");

        // Return 401 if the session does not contain a user ID.
        if (organiserId is null)
        {
            return Unauthorized();
        }

        // Filter events in the database so organisers see only their own.
        var events = await _context.Events
            .Where(raceEvent => raceEvent.OrganiserId == organiserId.Value)
            .OrderBy(raceEvent => raceEvent.EventDate)
            .ToListAsync();

        // Return the organiser's events, including an empty list if none exist.
        return Ok(events);
    }

    // Public endpoint that returns one event, or error code 404 if it does not exist
    [HttpGet("{id:int}")]
    [SessionAuthorize]
    [EndpointSummary("Get an event by ID")]
    [EndpointDescription("Here you can view the details of the requested event. (An authenticated session is required) .")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Event>> GetEvent(int id)
    {
        var raceEvent = await _context.Events.FindAsync(id);

        if (raceEvent is null)
        {
            return NotFound();
        }

        return Ok(raceEvent);
    }

    // Organisers can create events. The organiser ID comes from the session.
    [HttpPost]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("Create an event")]
    [EndpointDescription("Creates an event for the signed-in Organiser. The request provides the event details, and the Organiser ID comes from the session.")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Event>> CreateEvent(CreateEventRequest request)
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId is null)
        {
            return Unauthorized();
        }

        var raceEvent = new Event
        {
            OrganiserId = organiserId.Value,
            Name = request.Name,
            Description = request.Description,
            EventDate = request.EventDate,
            Venue = request.Venue,
            City = request.City,
            Province = request.Province,
            DistanceKm = request.DistanceKm,
            EventType = request.EventType
        };

        _context.Events.Add(raceEvent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = raceEvent.EventId },
            raceEvent);
    }

    // Organisers can only update events that were created by them, or that belong to them
    [HttpPut("{id:int}")]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("Update an event")]
    [EndpointDescription("Updates an event managed by the signed-in Organiser. This request must include the event ID.")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Event>> UpdateEvent(
        int id,
        UpdateEventRequest request)
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId is null)
        {
            return Unauthorized();
        }

        var raceEvent = await _context.Events.FindAsync(id);

        if (raceEvent is null)
        {
            return NotFound();
        }

        if (raceEvent.OrganiserId != organiserId.Value)
        {
            // Return 403 directly because this API uses custom session authorisation.
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        raceEvent.Name = request.Name;
        raceEvent.Description = request.Description;
        raceEvent.EventDate = request.EventDate;
        raceEvent.Venue = request.Venue;
        raceEvent.City = request.City;
        raceEvent.Province = request.Province;
        raceEvent.DistanceKm = request.DistanceKm;
        raceEvent.EventType = request.EventType;

        await _context.SaveChangesAsync();

        return Ok(raceEvent);
    }


    // Organisers can delete only their own events.
    [HttpDelete("{id:int}")]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("Delete an event")]
    [EndpointDescription("Deletes an event managed by the signed-in Organiser. (Deletion will be blocked while related records exist).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId is null)
        {
            return Unauthorized();
        }

        var raceEvent = await _context.Events.FindAsync(id);

        if (raceEvent is null)
        {
            return NotFound();
        }

        if (raceEvent.OrganiserId != organiserId.Value)
        {
            // Return 403 directly because this API uses custom session authorisation.
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        _context.Events.Remove(raceEvent);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Related routes, categories, or enrolments can prevent deletion.
            return Conflict(new
            {
                message = "This event cannot be deleted while it has related records."
            });
        }

        return NoContent();
    }



    // Organisers can upload a banner image for their own events.
    [HttpPost("{id:int}/banner")]
    [SessionAuthorize("Organiser")]
    [EndpointSummary("Upload an event banner")]
    [EndpointDescription("Uploads a banner image to Azure Blob Storage for an event owned by the signed-in Organiser.")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Event>> UploadBanner(
        int id,
        IFormFile image)
    {
        // Retrieve the Organiser ID from the authenticated API session.
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId is null)
        {
            return Unauthorized();
        }

        // Find the requested event.
        var raceEvent = await _context.Events.FindAsync(id);

        if (raceEvent is null)
        {
            return NotFound();
        }

        // Organisers may only upload banners for their own events.
        if (raceEvent.OrganiserId != organiserId.Value)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        // Ensure that an image was supplied.
        if (image is null || image.Length == 0)
        {
            return BadRequest(new { message = "Please select an image to upload." });
        }

        // Enforce the maximum upload size.
        if (image.Length > MaxImageSize)
        {
            return BadRequest(new { message = "The image cannot exceed 5 MB." });
        }

        // Only allow supported image content types.
        var allowedContentTypes = new[]
        {
    "image/jpeg",
    "image/png",
    "image/webp"
};

        if (!allowedContentTypes.Contains(image.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { message = "Only JPEG, PNG and WebP images are allowed." });
        }

        // Read the uploaded file into memory for signature validation.
        await using var imageStream = new MemoryStream();

        await image.CopyToAsync(imageStream);

        // Read the first 12 bytes of the uploaded file.
        var header = imageStream.ToArray().Take(12).ToArray();

        // Verify that the actual file format matches its declared content type.
        if (!IsValidImageSignature(header, image.ContentType))
        {
            return BadRequest(new
            {
                message = "The uploaded file is not a valid JPEG, PNG or WebP image."
            });
        }

        // Reset the stream so Azure receives the entire image.
        imageStream.Position = 0;

        // Upload the verified image to Azure Blob Storage.
        var imageUrl = await _blobStorageService.UploadImageAsync(
            imageStream,
            image.FileName,
            image.ContentType,
            "event-banners");

        // Store the resulting URL against the event.
        raceEvent.BannerImageUrl = imageUrl;

        await _context.SaveChangesAsync();

        return Ok(raceEvent);
    }//end of UploadBanner method.


    //Helper method for UploadBanner
    // Verify that the file's binary signature matches its declared image type.
    private static bool IsValidImageSignature(byte[] header, string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            // JPEG signature
            "image/jpeg" =>
                header.Length >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF,

            // PNG signature
            "image/png" =>
                header.Length >= 8 &&
                header.Take(8).SequenceEqual(new byte[]
                {
                0x89, 0x50, 0x4E, 0x47,
                0x0D, 0x0A, 0x1A, 0x0A
                }),

            // WebP signature
            "image/webp" =>
                header.Length >= 12 &&
                header.Take(4).SequenceEqual("RIFF"u8.ToArray()) &&
                header.Skip(8).Take(4).SequenceEqual("WEBP"u8.ToArray()),

            _ => false
        };
    } //end of IsValidImageSignature method.



} //end of class