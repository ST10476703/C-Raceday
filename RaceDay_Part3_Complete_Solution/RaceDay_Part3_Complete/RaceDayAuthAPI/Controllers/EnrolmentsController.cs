
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
    [Route("api")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayExistingDbContext _context;

        public EnrolmentsController(RaceDayExistingDbContext context)
        {
            _context = context;
        }

        // POST: api/events/1/enrolments
        [HttpPost("events/{eventId:int}/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(
            int eventId, EnrolmentRequest request)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int participantId))
            {
                return Unauthorized();
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == request.CategoryId &&
                    c.EventId == eventId);

            if (category == null)
            {
                return BadRequest(new
                {
                    message = "Category does not belong to this event."
                });
            }

            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantId == participantId &&
                    e.EventId == eventId);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this event."
                });
            }

            var enrolment = new Enrolment
            {
                ParticipantId = participantId,
                EventId = eventId,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Confirmed"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Enrolment successful.",
                enrolmentId = enrolment.EnrolmentId,
                eventId = enrolment.EventId,
                categoryId = enrolment.CategoryId,
                status = enrolment.Status
            });
        }

        // GET: api/enrolments/me
        [HttpGet("enrolments/me")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int participantId))
            {
                return Unauthorized();
            }

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.ParticipantId == participantId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.EventId,
                    eventName = e.Event.Name,
                    eventDate = e.Event.EventDate,
                    e.CategoryId,
                    categoryName = e.Category.Name,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/events/1/enrolments
        [HttpGet("events/{eventId:int}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int organiserId))
            {
                return Unauthorized();
            }

            var eventItem = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (eventItem.OrganiserId != organiserId)
            {
                return Forbid();
            }

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.EventId == eventId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    participantName =
                        e.Participant.FirstName + " " +
                        e.Participant.LastName,
                    participantEmail = e.Participant.Email,
                    e.CategoryId,
                    categoryName = e.Category.Name,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}
