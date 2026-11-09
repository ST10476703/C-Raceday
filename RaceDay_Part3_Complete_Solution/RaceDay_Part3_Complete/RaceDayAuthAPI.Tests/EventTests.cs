
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAuthAPI.Controllers;
using RaceDayAuthAPI.Data;
using RaceDayAuthAPI.DatabaseModels;
using RaceDayAuthAPI.DTOs;
using System.Security.Claims;
using Xunit;

namespace RaceDayAuthAPI.Tests
{
    public class EventTests
    {
        private RaceDayExistingDbContext CreateDbContext()
        {
            var options =
                new DbContextOptionsBuilder<RaceDayExistingDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

            return new RaceDayExistingDbContext(options);
        }

        private EventsController CreateController(
            RaceDayExistingDbContext context,
            int userId = 1,
            string role = "Organiser")
        {
            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.ToString()),

                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(
                claims, "TestAuthentication");

            var principal = new ClaimsPrincipal(identity);

            return new EventsController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = principal
                    }
                }
            };
        }

        private EventRequest CreateValidRequest()
        {
            return new EventRequest
            {
                Name = "Pretoria Marathon",
                Description = "Annual running event",
                EventDate = new DateTime(2026, 11, 14),
                Location = "Pretoria",
                Distance = 10,
                EventType = "Running",
                BannerImageLink = null
            };
        }

        // TEST 1 - Get all events
        [Fact]
        public async Task GetEvents_ReturnsOk()
        {
            using var context = CreateDbContext();

            var controller = CreateController(context);

            var result = await controller.GetEvents();

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 2 - Event does not exist
        [Fact]
        public async Task GetEvent_InvalidId_ReturnsNotFound()
        {
            using var context = CreateDbContext();

            var controller = CreateController(context);

            var result = await controller.GetEvent(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // TEST 3 - Organiser creates an event
        [Fact]
        public async Task CreateEvent_ValidRequest_SavesEvent()
        {
            using var context = CreateDbContext();

            var controller = CreateController(context);

            var request = CreateValidRequest();

            var result = await controller.CreateEvent(request);

            var response = Assert.IsType<ObjectResult>(result);

            Assert.Equal(201, response.StatusCode);

            Assert.Equal(1, await context.Events.CountAsync());

            var savedEvent = await context.Events.FirstAsync();

            Assert.Equal("Pretoria Marathon", savedEvent.Name);
            Assert.Equal(1, savedEvent.OrganiserId);
        }

        // TEST 4 - Another organiser cannot update an event
        [Fact]
        public async Task UpdateEvent_DifferentOrganiser_ReturnsForbidden()
        {
            using var context = CreateDbContext();

            context.Events.Add(new Event
            {
                EventId = 1,
                OrganiserId = 1,
                Name = "Pretoria Marathon",
                Description = "Running event",
                EventDate = new DateOnly(2026, 11, 14),
                Location = "Pretoria",
                Distance = 10,
                EventType = "Running",
                DateCreated = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            // Organiser 2 attempts to edit Organiser 1's event
            var controller = CreateController(
                context, userId: 2);

            var result = await controller.UpdateEvent(
                1, CreateValidRequest());

            Assert.IsType<ForbidResult>(result);
        }

        // TEST 5 - Organiser can delete their own event
        [Fact]
        public async Task DeleteEvent_Owner_RemovesEvent()
        {
            using var context = CreateDbContext();

            context.Events.Add(new Event
            {
                EventId = 1,
                OrganiserId = 1,
                Name = "Test Event",
                Description = "Testing deletion",
                EventDate = new DateOnly(2026, 11, 14),
                Location = "Pretoria",
                Distance = 5,
                EventType = "Running",
                DateCreated = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, userId: 1);

            var result = await controller.DeleteEvent(1);

            Assert.IsType<NoContentResult>(result);

            Assert.Empty(await context.Events.ToListAsync());
        }
    }
}
