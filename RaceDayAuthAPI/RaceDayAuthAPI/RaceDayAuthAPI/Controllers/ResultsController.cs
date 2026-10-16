
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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayExistingDbContext _context;

        public ResultsController(RaceDayExistingDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int userId) ? userId : 0;
        }


        // POST /api/enrolments/{enrolmentId}/result
        [HttpPost("enrolments/{enrolmentId:int}/result")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(
            int enrolmentId, ResultRequest request)
        {
            int organiserId = GetCurrentUserId();

            if (organiserId == 0)
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.EnrolmentId == enrolmentId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.EventId,
                    OrganiserId = e.Event.OrganiserId
                })
                .FirstOrDefaultAsync();

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            if (enrolment.OrganiserId != organiserId)
                return Forbid();

            bool resultExists = await _context.Results
                .AnyAsync(r => r.EnrolmentId == enrolmentId);

            if (resultExists)
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });

            var result = new Result
            {
                EnrolmentId = enrolmentId,
                FinishTime = TimeOnly.FromTimeSpan(request.FinishTime!.Value),
                FinishPosition = request.FinishPosition!.Value,
                RecordedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Result recorded successfully.",
                resultId = result.ResultId,
                enrolmentId = result.EnrolmentId,
                finishTime = result.FinishTime,
                finishPosition = result.FinishPosition
            });
        }

        // PUT /api/results/{resultId}
        [HttpPut("results/{resultId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(
            int resultId, ResultRequest request)
        {
            int organiserId = GetCurrentUserId();

            if (organiserId == 0)
                return Unauthorized();

            var result = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e.Event)
                .FirstOrDefaultAsync(r => r.ResultId == resultId);

            if (result == null)
                return NotFound(new { message = "Result not found." });

            if (result.Enrolment.Event.OrganiserId != organiserId)
                return Forbid();

            result.FinishTime = TimeOnly.FromTimeSpan(request.FinishTime!.Value);
            result.FinishPosition = request.FinishPosition!.Value;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result updated successfully.",
                resultId = result.ResultId
            });
        }

        // GET /api/results/me
        [HttpGet("results/me")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            int participantId = GetCurrentUserId();

            if (participantId == 0)
                return Unauthorized();

            var results = await _context.Results
                .AsNoTracking()
                .Where(r => r.Enrolment.ParticipantId == participantId)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    eventId = r.Enrolment.EventId,
                    eventName = r.Enrolment.Event.Name,
                    categoryName = r.Enrolment.Category.Name,
                    r.FinishTime,
                    r.FinishPosition,
                    r.RecordedAt
                })
                .OrderBy(r => r.FinishPosition)
                .ToListAsync();

            return Ok(results);
        }
    }
}
