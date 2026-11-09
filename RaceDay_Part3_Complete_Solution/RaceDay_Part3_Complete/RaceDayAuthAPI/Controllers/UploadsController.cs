using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAuthAPI.Data;
using RaceDayAuthAPI.Services;
using System.Security.Claims;

namespace RaceDayAuthAPI.Controllers;

[ApiController]
[Route("api/uploads")]
public sealed class UploadsController : ControllerBase
{
    private readonly AzureImageService _images;
    private readonly RaceDayExistingDbContext _db;

    public UploadsController(AzureImageService images, RaceDayExistingDbContext db)
    {
        _images = images;
        _db = db;
    }

    [HttpPost("event/{eventId:int}")]
    [Authorize(Roles = "Organiser")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> EventBanner(int eventId, [FromForm] IFormFile? file, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId)) return Unauthorized();
        var item = await _db.Events.FirstOrDefaultAsync(x => x.EventId == eventId, ct);
        if (item is null) return NotFound(new { message = "Event not found." });
        if (item.OrganiserId != userId) return Forbid();
        if (file is null || AzureImageService.Inspect(file) is null)
            return BadRequest(new { message = "Choose a valid JPG, PNG or WebP image under 5 MB." });
        try
        {
            var upload = await _images.UploadAsync(file, ct);
            item.BannerImageLink = upload.AzureUrl;
            await _db.SaveChangesAsync(ct);
            return Ok(new { url = upload.AzureUrl, imagePath = $"/api/uploads/{upload.BlobName}" });
        }
        catch (InvalidOperationException) { return StatusCode(503, new { message = "Azure Storage is not configured." }); }
        catch (RequestFailedException) { return StatusCode(502, new { message = "Azure image upload failed." }); }
    }

    [HttpPost("profile")]
    [Authorize]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> ProfilePicture([FromForm] IFormFile? file, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId)) return Unauthorized();
        var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (user is null) return NotFound(new { message = "User not found." });
        if (file is null || AzureImageService.Inspect(file) is null)
            return BadRequest(new { message = "Choose a valid JPG, PNG or WebP image under 5 MB." });
        try
        {
            var upload = await _images.UploadAsync(file, ct);
            user.ProfileImageLink = upload.AzureUrl;
            await _db.SaveChangesAsync(ct);
            return Ok(new { url = upload.AzureUrl, imagePath = $"/api/uploads/{upload.BlobName}" });
        }
        catch (InvalidOperationException) { return StatusCode(503, new { message = "Azure Storage is not configured." }); }
        catch (RequestFailedException) { return StatusCode(502, new { message = "Azure image upload failed." }); }
    }

    // Image streaming works even when the Azure container is private.
    [HttpGet("{blobName}")]
    [AllowAnonymous]
    public async Task<IActionResult> Image(string blobName, CancellationToken ct)
    {
        try
        {
            var image = await _images.DownloadAsync(blobName, ct);
            if (image is null) return NotFound();
            Response.Headers.CacheControl = "public,max-age=3600";
            return File(image.Value.Bytes, image.Value.Mime);
        }
        catch (InvalidOperationException) { return StatusCode(503); }
        catch (RequestFailedException) { return StatusCode(502); }
    }
}
