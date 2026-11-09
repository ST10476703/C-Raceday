

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
    public class ResultsTests
    {
        private RaceDayExistingDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayExistingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayExistingDbContext(options);
        }

        private ResultsController CreateController(
            RaceDayExistingDbContext context,
            int organiserId = 1)
        {
            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    organiserId.ToString()),
                new Claim(ClaimTypes.Role, "Organiser")
            };

            return new ResultsController(context)
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

            context.Enrolments.Add(new Enrolment
            {
                EnrolmentId = 1,
                ParticipantId = 2,
                EventId = 1,
                CategoryId = 1,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Confirmed"
            });

            await context.SaveChangesAsync();
        }

        private ResultRequest ValidResult()
        {
            return new ResultRequest
            {
                FinishTime = TimeSpan.FromMinutes(75),
                FinishPosition = 3
            };
        }

        [Fact]
        public async Task CreateResult_ValidRequest_Succeeds()
        {
            using var context = CreateContext();
            await SeedData(context);

            var controller = CreateController(context);

            var result = await controller.CreateResult(
                1, ValidResult());

            var response = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, response.StatusCode);
            Assert.Single(await context.Results.ToListAsync());
        }

        [Fact]
        public async Task CreateResult_Duplicate_ReturnsConflict()
        {
            using var context = CreateContext();
            await SeedData(context);

            var controller = CreateController(context);

            await controller.CreateResult(1, ValidResult());
            var result = await controller.CreateResult(1, ValidResult());

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task CreateResult_DifferentOrganiser_ReturnsForbidden()
        {
            using var context = CreateContext();
            await SeedData(context);

            var controller = CreateController(context, organiserId: 99);

            var result = await controller.CreateResult(
                1, ValidResult());

            Assert.IsType<ForbidResult>(result);
        }
    }
}
