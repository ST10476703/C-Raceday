
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
    public class EnrolmentTests
    {
        private RaceDayExistingDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayExistingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayExistingDbContext(options);
        }

        private EnrolmentsController CreateController(
            RaceDayExistingDbContext context,
            int userId = 2,
            string role = "Participant")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            return new EnrolmentsController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(claims, "Test"))
                    }
                }
            };
        }

        private async Task SeedData(RaceDayExistingDbContext context)
        {
            context.Events.Add(new Event
            {
                EventId = 1,
                OrganiserId = 1,
                Name = "Pretoria Run",
                Description = "Race event",
                EventDate = new DateOnly(2026, 11, 14),
                Location = "Pretoria",
                Distance = 10,
                EventType = "Running",
                DateCreated = DateTime.UtcNow
            });

            context.Categories.Add(new Category
            {
                CategoryId = 1,
                EventId = 1,
                Name = "10km Open",
                CategoryType = "Running",
                Distance = 10
            });

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateEnrolment_ValidRequest_Succeeds()
        {
            using var context = CreateContext();
            await SeedData(context);

            var controller = CreateController(context);

            var result = await controller.CreateEnrolment(
                1, new EnrolmentRequest { CategoryId = 1 });

            var response = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, response.StatusCode);
            Assert.Single(await context.Enrolments.ToListAsync());
        }

        [Fact]
        public async Task CreateEnrolment_Duplicate_ReturnsConflict()
        {
            using var context = CreateContext();
            await SeedData(context);

            var controller = CreateController(context);
            var request = new EnrolmentRequest { CategoryId = 1 };

            await controller.CreateEnrolment(1, request);
            var result = await controller.CreateEnrolment(1, request);

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task CreateEnrolment_InvalidCategory_ReturnsBadRequest()
        {
            using var context = CreateContext();
            await SeedData(context);

            var controller = CreateController(context);

            var result = await controller.CreateEnrolment(
                1, new EnrolmentRequest { CategoryId = 999 });

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
