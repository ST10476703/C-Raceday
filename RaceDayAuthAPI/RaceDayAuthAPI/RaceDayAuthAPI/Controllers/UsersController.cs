

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAuthAPI.Data;
using RaceDayAuthAPI.DTOs;
using System.Security.Claims;

namespace RaceDayAuthAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayExistingDbContext _context;

        public UsersController(RaceDayExistingDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET: api/users/me
        // Retrieve the logged-in user's profile
        // ==========================================

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            string? userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid authentication token."
                });
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                userId = user.UserId,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                phone = user.Phone,
                profileImageLink = user.ProfileImageLink,
                role = user.Role,
                dateRegistered = user.DateRegistered
            });
        }

        // ==========================================
        // PUT: api/users/me
        // Update the logged-in user's profile
        // ==========================================

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(
            UpdateProfileRequest request)
        {
            string? userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid authentication token."
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // Validate names
            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new
                {
                    message = "First name and last name are required."
                });
            }

            // Update basic information
            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();

            // Update optional phone number
            user.Phone = string.IsNullOrWhiteSpace(request.Phone)
                ? null
                : request.Phone.Trim();

            // Update optional profile image link
            user.ProfileImageLink =
                string.IsNullOrWhiteSpace(request.ProfileImageLink)
                    ? null
                    : request.ProfileImageLink.Trim();

            // Save changes to SQL Server
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                userId = user.UserId,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                phone = user.Phone,
                profileImageLink = user.ProfileImageLink,
                role = user.Role
            });
        }
    }
}

