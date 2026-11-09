
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDayAuthAPI.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDayAuthAPI.Tests
{
    public class ApiIntegrationTests
    {
        private WebApplicationFactory<Program> CreateFactory()
        {
            string databaseName =
                "RaceDayTest_" + Guid.NewGuid();

            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration(
                        (context, configuration) =>
                        {
                            configuration.AddInMemoryCollection(
                                new Dictionary<string, string?>
                                {
                                    ["Jwt:Key"] =
                                        "RaceDay-Test-Key-Only-For-Integration-Tests-2026-Secure-Example-12345",
                                    ["Jwt:Issuer"] = "RaceDayApi",
                                    ["Jwt:Audience"] = "RaceDayClient"
                                });
                        });

                    builder.ConfigureTestServices(services =>
                    {
                        services.RemoveAll<
                            DbContextOptions<RaceDayExistingDbContext>>();

                        services.RemoveAll<RaceDayExistingDbContext>();

                        services.AddDbContext<RaceDayExistingDbContext>(
                            options =>
                                options.UseInMemoryDatabase(databaseName));
                    });
                });
        }

        [Fact]
        public async Task GetProfile_WithoutToken_ReturnsUnauthorized()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/users/me");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsCreated()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            var request = new
            {
                firstName = "Test",
                lastName = "Participant",
                email = "integration@example.com",
                password = "Password123!",
                role = "Participant"
            };

            var response = await client.PostAsJsonAsync(
                "/api/auth/register", request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidToken_CanAccessProfile()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            // Register a new user
            var registerRequest = new
            {
                firstName = "Test",
                lastName = "Participant",
                email = "jwt-test@example.com",
                password = "Password123!",
                role = "Participant"
            };

            var registerResponse = await client.PostAsJsonAsync(
                "/api/auth/register", registerRequest);

            Assert.True(
              registerResponse.StatusCode == HttpStatusCode.Created,
              $"REGISTRATION FAILED: {registerResponse.StatusCode}. " +
              await registerResponse.Content.ReadAsStringAsync());

            // Login and retrieve JWT
            var loginRequest = new
            {
                email = "jwt-test@example.com",
                password = "Password123!"
            };

            var loginResponse = await client.PostAsJsonAsync(
                "/api/auth/login", loginRequest);

            Assert.True( loginResponse.StatusCode == HttpStatusCode.OK,
                  $"LOGIN FAILED: {loginResponse.StatusCode}. " +
                 await loginResponse.Content.ReadAsStringAsync());

            using var loginJson = JsonDocument.Parse(
                await loginResponse.Content.ReadAsStringAsync());

            string token = loginJson.RootElement
                .GetProperty("token")
                .GetString()!;

            Assert.False(string.IsNullOrWhiteSpace(token));

            // Use JWT to access protected profile
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var profileResponse = await client.GetAsync(
                "/api/users/me");
            var authenticateHeader =  profileResponse.Headers.WwwAuthenticate.ToString();

            Assert.True(
                profileResponse.StatusCode == HttpStatusCode.OK,
                $"PROFILE FAILED: {profileResponse.StatusCode}. " +
                $"WWW-Authenticate: {authenticateHeader}. " +
                await profileResponse.Content.ReadAsStringAsync());
            
        }
    }
}
