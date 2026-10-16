
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAuthAPI.Data;
using RaceDayAuthAPI.DatabaseModels;
using RaceDayAuthAPI.DTOs;
using System.Security.Claims;

namespace RaceDayAuthAPI.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayExistingDbContext _context;

        public EventsController(RaceDayExistingDbContext context)
        {
            _context = context;
        }

        // GET: api/events
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.Distance,
                    e.EventType,
                    e.BannerImageLink,
                    e.OrganiserId
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/1
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.Distance,
                    e.EventType,
                    e.BannerImageLink,
                    e.OrganiserId
                })
                .FirstOrDefaultAsync();

            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            return Ok(eventItem);
        }

        // POST: api/events
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(EventRequest request)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int organiserId))
            {
                return Unauthorized();
            }

            var eventItem = new Event
            {
                OrganiserId = organiserId,
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                EventDate = DateOnly.FromDateTime(request.EventDate),
                Location = request.Location,
                Distance = request.Distance,
                EventType = request.EventType,
                BannerImageLink = request.BannerImageLink,
                DateCreated = DateTime.UtcNow
            };

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Event created successfully.",
                eventId = eventItem.EventId
            });
        }

        // PUT: api/events/1
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(
            int id, EventRequest request)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int organiserId))
            {
                return Unauthorized();
            }

            if (eventItem.OrganiserId != organiserId)
                return Forbid();

            eventItem.Name = request.Name;
            eventItem.Description = request.Description ?? string.Empty;
            eventItem.EventDate = DateOnly.FromDateTime(request.EventDate);
            eventItem.Location = request.Location;
            eventItem.Distance = request.Distance;
            eventItem.EventType = request.EventType;
            eventItem.BannerImageLink = request.BannerImageLink;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event updated successfully." });
        }

        // DELETE: api/events/1
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int organiserId))
            {
                return Unauthorized();
            }

            if (eventItem.OrganiserId != organiserId)
                return Forbid();

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
