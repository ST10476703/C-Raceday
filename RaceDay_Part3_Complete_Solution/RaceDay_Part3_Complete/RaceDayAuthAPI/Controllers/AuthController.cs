
using Microsoft.AspNetCore.Mvc;
using RaceDayAuthApi.Services;
using RaceDayAuthAPI.DTOs;
using RaceDayAuthAPI.Services;

namespace RaceDayAuthAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly JwtService _jwtService;

        public AuthController(
            AuthService authService,
            JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return StatusCode(201, new
            {
                message = result.Message,
                userId = result.User!.UserId,
                firstName = result.User.FirstName,
                lastName = result.User.LastName,
                email = result.User.Email,
                role = result.User.Role
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return Unauthorized(new
                {
                    message = result.Message
                });
            }

            string token = _jwtService.GenerateToken(result.User!);

            return Ok(new
            {
                message = "Login successful.",
                token,
                tokenType = "Bearer",
                expiresIn = 7200,
                userId = result.User!.UserId,
                email = result.User.Email,
                role = result.User.Role
            });
        }
    }
}
