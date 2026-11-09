using Marathon.Controllers;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace TestMarathon
{
    public class EventsControllerTests
    {
        private static Event NewEvent(int createdBy) => new()
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
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        private static NewEventDTO NewEventDto() => new()
        {
            EventName = "Joburg City Cycle Tour",
            EventDate = new DateOnly(2026, 11, 15),
            Location = "Johannesburg",
            Venue = "FNB Stadium",
            DistanceKm = 60.00m,
            EventType = "Cycle",
            EventFee = 280.00m
        };

        [Fact]
        public void GetEvents_ReturnsAllEvents()
        {
            var db = TestSupport.CreateDb();
            db.events.AddRange(NewEvent(1), NewEvent(2));
            db.SaveChanges();
            var controller = new EventsController(db).WithUser(TestSupport.Principal(1, "o@x.com", "Organiser"));

            var result = controller.GetEvents();

            var ok = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<IEnumerable<Event>>(ok.Value).ToList();
            Assert.Equal(2, events.Count);
        }

        [Fact]
        public void GetEventById_UnknownId_ReturnsNotFound()
        {
            var db = TestSupport.CreateDb();
            var controller = new EventsController(db).WithUser(TestSupport.Principal(1, "o@x.com", "Organiser"));

            var result = controller.GetEventById(Guid.NewGuid());

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void CreateEvent_StampsCreatorFromToken()
        {
            var db = TestSupport.CreateDb();
            var controller = new EventsController(db).WithUser(TestSupport.Principal(42, "o@x.com", "Organiser"));

            var result = controller.CreateEvent(NewEventDto());

            Assert.IsType<CreatedAtActionResult>(result);
            var saved = Assert.Single(db.events.ToList());
            Assert.Equal(42, saved.CreatedBy);
            Assert.Equal("Upcoming", saved.Status);
        }

        [Fact]
        public void UpdateEvent_UnknownId_ReturnsNotFound()
        {
            var db = TestSupport.CreateDb();
            var controller = new EventsController(db).WithUser(TestSupport.Principal(1, "o@x.com", "Organiser"));

            var result = controller.UpdateEvent(Guid.NewGuid(), new UpdateEventDTO
            {
                EventName = "X",
                EventDate = new DateOnly(2026, 1, 1),
                Location = "L",
                Venue = "V",
                DistanceKm = 5m,
                EventType = "Run",
                EventFee = 10m
            });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void DeleteEvent_ExistingId_RemovesEvent()
        {
            var db = TestSupport.CreateDb();
            var existing = NewEvent(1);
            db.events.Add(existing);
            db.SaveChanges();
            var controller = new EventsController(db).WithUser(TestSupport.Principal(1, "o@x.com", "Organiser"));

            var result = controller.RemoveEvent(existing.EventId);

            Assert.IsType<OkObjectResult>(result);
            Assert.Empty(db.events.ToList());
        }
    }
}
