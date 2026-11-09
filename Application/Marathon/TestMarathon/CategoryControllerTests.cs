using Marathon.Controllers;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace TestMarathon
{
    public class CategoryControllerTests
    {
        private static Event SeedEvent(ApplicationDbcontext db)
        {
            var ev = new Event
            {
                EventName = "Cape Town Marathon",
                EventDate = new DateOnly(2026, 10, 18),
                Now = DateTime.UtcNow,
                Location = "Cape Town",
                Venue = "Green Point Stadium",
                DistanceKm = 42.20m,
                EventType = "Run",
                EventFee = 450.00m,
                Status = "Open",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.events.Add(ev);
            db.SaveChanges();
            return ev;
        }

        private static NewCategoryDTO NewCategory(Guid eventId) => new()
        {
            EventId = eventId,
            CategoryName = "Senior Men",
            MinAge = 18,
            MaxAge = 39,
            GenderRestrictions = "Male",
            MaxParticipants = 100,
            EntryFee = 450.00m
        };

        private static CategoryController Controller(ApplicationDbcontext db) =>
            new CategoryController(db).WithUser(TestSupport.Principal(1, "org@raceday.com", "Organiser"));

        [Fact]
        public void CreateCategory_UnknownEvent_ReturnsBadRequest()
        {
            var db = TestSupport.CreateDb();

            var result = Controller(db).CreateCategory(NewCategory(Guid.NewGuid()));

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateCategory_MinAgeAboveMaxAge_ReturnsBadRequest()
        {
            var db = TestSupport.CreateDb();
            var ev = SeedEvent(db);
            var dto = NewCategory(ev.EventId);
            dto.MinAge = 50;
            dto.MaxAge = 20;

            var result = Controller(db).CreateCategory(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateCategory_Valid_ReturnsCreatedAndStampsCreator()
        {
            var db = TestSupport.CreateDb();
            var ev = SeedEvent(db);

            var result = Controller(db).CreateCategory(NewCategory(ev.EventId));

            Assert.IsType<CreatedAtActionResult>(result);
            var saved = Assert.Single(db.categories.ToList());
            Assert.Equal("org@raceday.com", saved.CreatedBy);
        }

        [Fact]
        public void CreateCategory_DuplicateName_ReturnsConflict()
        {
            var db = TestSupport.CreateDb();
            var ev = SeedEvent(db);
            var controller = Controller(db);
            controller.CreateCategory(NewCategory(ev.EventId));

            var result = controller.CreateCategory(NewCategory(ev.EventId));

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public void UpdateCategory_UnknownId_ReturnsNotFound()
        {
            var db = TestSupport.CreateDb();

            var result = Controller(db).EditCatery(Guid.NewGuid(), new Edit_CategoryDTO { CategoryName = "X" });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetCategories_ReturnsSeededRows()
        {
            var db = TestSupport.CreateDb();
            var ev = SeedEvent(db);
            Controller(db).CreateCategory(NewCategory(ev.EventId));

            var result = Controller(db).GetAllCategories();

            var ok = Assert.IsType<OkObjectResult>(result);
            var rows = Assert.IsAssignableFrom<IEnumerable<Category>>(ok.Value).ToList();
            Assert.Single(rows);
        }
    }
}
