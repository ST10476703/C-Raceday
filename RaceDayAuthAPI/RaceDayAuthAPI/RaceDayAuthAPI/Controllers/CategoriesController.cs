
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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayExistingDbContext _context;

        public CategoriesController(RaceDayExistingDbContext context)
        {
            _context = context;
        }

        [HttpGet("events/{eventId:int}/categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.EventId == eventId)
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.Name,
                    c.CategoryType,
                    c.MinimumAge,
                    c.MaximumAge,
                    c.Distance
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost("events/{eventId:int}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(
            int eventId, CategoryRequest request)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            int organiserId = GetCurrentUserId();

            if (eventItem.OrganiserId != organiserId)
                return Forbid();

            if (request.MinimumAge.HasValue &&
                request.MaximumAge.HasValue &&
                request.MinimumAge > request.MaximumAge)
            {
                return BadRequest(new
                {
                    message = "Minimum age cannot exceed maximum age."
                });
            }

            var category = new Category
            {
                EventId = eventId,
                Name = request.Name,
                CategoryType = request.CategoryType,
                MinimumAge = request.MinimumAge,
                MaximumAge = request.MaximumAge,
                Distance = request.Distance
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Category created successfully.",
                categoryId = category.CategoryId
            });
        }

        [HttpPut("categories/{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int id, CategoryRequest request)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == category.EventId);

            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            if (eventItem.OrganiserId != GetCurrentUserId())
                return Forbid();

            if (request.MinimumAge.HasValue &&
                request.MaximumAge.HasValue &&
                request.MinimumAge > request.MaximumAge)
            {
                return BadRequest(new
                {
                    message = "Minimum age cannot exceed maximum age."
                });
            }

            category.Name = request.Name;
            category.CategoryType = request.CategoryType;
            category.MinimumAge = request.MinimumAge;
            category.MaximumAge = request.MaximumAge;
            category.Distance = request.Distance;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Category updated successfully." });
        }

        [HttpDelete("categories/{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == category.EventId);

            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            if (eventItem.OrganiserId != GetCurrentUserId())
                return Forbid();

            bool hasEnrolments = await _context.Enrolments
                .AnyAsync(e => e.CategoryId == id);

            if (hasEnrolments)
            {
                return Conflict(new
                {
                    message = "Cannot delete a category with enrolments."
                });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int GetCurrentUserId()
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int userId) ? userId : 0;
        }
    }
}
