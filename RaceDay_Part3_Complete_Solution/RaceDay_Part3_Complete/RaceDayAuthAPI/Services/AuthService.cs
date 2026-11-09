using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDayAuthAPI.Data;
using RaceDayAuthAPI.DatabaseModels;
using RaceDayAuthAPI.DTOs;

namespace RaceDayAuthAPI.Services
{
    public class AuthService
    {
        private readonly RaceDayExistingDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(RaceDayExistingDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, User? User)>
            RegisterAsync(RegisterRequest request)
        {
            if (request.Role != "Participant" &&
                request.Role != "Organiser")
            {
                return (false,
                    "Role must be Participant or Organiser.", null);
            }

            string email = request.Email.Trim();

            bool emailExists = await _context.Users.AnyAsync(u =>
                u.Email == email);

            if (emailExists)
            {
                return (false,
                    "A user with this email already exists.", null);
            }

            User newUser = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Role = request.Role,
                DateRegistered = DateTime.UtcNow
            };

            newUser.PasswordHash =
                _passwordHasher.HashPassword(
                    newUser, request.Password);

            _context.Users.Add(newUser);

            await _context.SaveChangesAsync();

            return (true, "Registration successful.", newUser);
        }

        public async Task<(bool Success, string Message, User? User)>
            LoginAsync(LoginRequest request)
        {
            string email = request.Email.Trim();

            User? user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == email);

            if (user == null)
            {
                return (false, "Invalid email or password.", null);
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return (false, "Invalid email or password.", null);
            }

            return (true, "Login successful.", user);
        }
    }
}

