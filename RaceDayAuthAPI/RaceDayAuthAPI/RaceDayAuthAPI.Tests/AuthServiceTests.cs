
using Microsoft.EntityFrameworkCore;
using RaceDayAuthAPI.Data;
using RaceDayAuthAPI.DTOs;
using RaceDayAuthAPI.Services;
using Xunit;

namespace RaceDayAuthAPI.Tests
{
    public class AuthServiceTests
    {
        // Create an isolated database for each test
        private RaceDayExistingDbContext CreateDbContext()
        {
            var options =
                new DbContextOptionsBuilder<RaceDayExistingDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

            return new RaceDayExistingDbContext(options);
        }

        private RegisterRequest CreateValidRequest()
        {
            return new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@example.com",
                Password = "Password123!",
                Role = "Participant"
            };
        }

        // TEST 1: Valid registration
        [Fact]
        public async Task Register_ValidUser_ReturnsSuccess()
        {
            using var context = CreateDbContext();
            var service = new AuthService(context);

            var request = CreateValidRequest();

            var result = await service.RegisterAsync(request);

            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal("Registration successful.", result.Message);
            Assert.Equal(1, await context.Users.CountAsync());
        }

        // TEST 2: Duplicate email
        [Fact]
        public async Task Register_DuplicateEmail_ReturnsFailure()
        {
            using var context = CreateDbContext();
            var service = new AuthService(context);

            var request = CreateValidRequest();

            await service.RegisterAsync(request);

            var secondResult = await service.RegisterAsync(request);

            Assert.False(secondResult.Success);
            Assert.Single(await context.Users.ToListAsync());
        }

        // TEST 3: Invalid role
        [Fact]
        public async Task Register_InvalidRole_ReturnsFailure()
        {
            using var context = CreateDbContext();
            var service = new AuthService(context);

            var request = CreateValidRequest();
            request.Role = "SuperAdmin";

            var result = await service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Empty(await context.Users.ToListAsync());
        }

        // TEST 4: Password is hashed
        [Fact]
        public async Task Register_Password_IsHashed()
        {
            using var context = CreateDbContext();
            var service = new AuthService(context);

            var request = CreateValidRequest();

            var result = await service.RegisterAsync(request);

            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.NotEqual(
                request.Password,
                result.User!.PasswordHash);
        }

        // TEST 5: Correct login
        [Fact]
        public async Task Login_CorrectCredentials_ReturnsSuccess()
        {
            using var context = CreateDbContext();
            var service = new AuthService(context);

            await service.RegisterAsync(CreateValidRequest());

            var login = new LoginRequest
            {
                Email = "participant@example.com",
                Password = "Password123!"
            };

            var result = await service.LoginAsync(login);

            Assert.True(result.Success);
            Assert.NotNull(result.User);
        }

        // TEST 6: Incorrect password
        [Fact]
        public async Task Login_WrongPassword_ReturnsFailure()
        {
            using var context = CreateDbContext();
            var service = new AuthService(context);

            await service.RegisterAsync(CreateValidRequest());

            var login = new LoginRequest
            {
                Email = "participant@example.com",
                Password = "WrongPassword"
            };

            var result = await service.LoginAsync(login);

            Assert.False(result.Success);
            Assert.Null(result.User);
        }

        // TEST 7: Unknown email
        [Fact]
        public async Task Login_UnknownEmail_ReturnsFailure()
        {
            using var context = CreateDbContext();
            var service = new AuthService(context);

            var login = new LoginRequest
            {
                Email = "unknown@example.com",
                Password = "Password123!"
            };

            var result = await service.LoginAsync(login);

            Assert.False(result.Success);
            Assert.Null(result.User);
        }
    }
}
