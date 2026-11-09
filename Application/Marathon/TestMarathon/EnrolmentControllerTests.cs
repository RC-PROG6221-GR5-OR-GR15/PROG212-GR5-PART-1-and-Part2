using Marathon.Controllers;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace TestMarathon
{
    public class EnrolmentControllerTests
    {
        private const string ParticipantEmail = "participant@raceday.com";

        private static (ApplicationDbcontext db, User profile, Category category) Seed()
        {
            var db = TestSupport.CreateDb();

            db.accounts.Add(new UserAccount { UserAccountId = 1, Email = ParticipantEmail, PasswordHash = "x", Role = "Participant" });

            var profile = new User
            {
                Username = ParticipantEmail,
                FirstName = "Lerato",
                LastName = "Pillay",
                DateOfBirth = new DateOnly(1996, 11, 5),
                PhoneNumber = "0830000000",
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                IsActive = true
            };
            db.users.Add(profile);

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

            var category = new Category
            {
                EventId = ev.EventId,
                CategoryName = "Senior Men",
                MinAge = 18,
                MaxAge = 39,
                GenderRestrictions = "Male",
                MaxParticipants = 1,
                EntryFee = 450.00m,
                CreatedBy = "org@raceday.com",
                CreatedAt = DateTime.UtcNow
            };
            db.categories.Add(category);
            db.SaveChanges();

            return (db, profile, category);
        }

        private static EnrolmentController Controller(ApplicationDbcontext db) =>
            new EnrolmentController(db).WithUser(TestSupport.Principal(1, ParticipantEmail, "Participant"));

        [Fact]
        public void Enrol_ValidParticipant_Succeeds()
        {
            var (db, _, category) = Seed();

            var result = Controller(db).EnrolUserInCategory(new EnrollDTO { CategoryId = category.CategoryId, AmountPaid = 450m });

            Assert.IsType<OkObjectResult>(result);
            var enrolment = Assert.Single(db.enrolments.ToList());
            Assert.Equal("Pending", enrolment.enrolment_status);
            Assert.Equal("Paid", enrolment.payment_status);
        }

        [Fact]
        public void Enrol_PartialPayment_MarksPaymentPending()
        {
            var (db, _, category) = Seed();

            Controller(db).EnrolUserInCategory(new EnrollDTO { CategoryId = category.CategoryId, AmountPaid = 100m });

            Assert.Equal("Pending", Assert.Single(db.enrolments.ToList()).payment_status);
        }

        [Fact]
        public void Enrol_UnknownCategory_ReturnsNotFound()
        {
            var (db, _, _) = Seed();

            var result = Controller(db).EnrolUserInCategory(new EnrollDTO { CategoryId = Guid.NewGuid(), AmountPaid = 450m });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void Enrol_NegativeAmount_ReturnsBadRequest()
        {
            var (db, _, category) = Seed();

            var result = Controller(db).EnrolUserInCategory(new EnrollDTO { CategoryId = category.CategoryId, AmountPaid = -1m });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Enrol_DuplicateCategory_ReturnsConflict()
        {
            var (db, _, category) = Seed();
            var controller = Controller(db);
            controller.EnrolUserInCategory(new EnrollDTO { CategoryId = category.CategoryId, AmountPaid = 450m });

            var result = controller.EnrolUserInCategory(new EnrollDTO { CategoryId = category.CategoryId, AmountPaid = 450m });

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public void Enrol_CategoryFull_ReturnsConflict()
        {
            var (db, profile, category) = Seed();

            // Fill the single available place with a different participant.
            var other = new User
            {
                Username = "other@raceday.com",
                FirstName = "Other",
                LastName = "Runner",
                DateOfBirth = new DateOnly(1990, 1, 1),
                PhoneNumber = "0810000000",
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                IsActive = true
            };
            db.users.Add(other);
            db.enrolments.Add(new Enrolment
            {
                UserId = other.UserId,
                EventId = category.EventId,
                CategoryId = category.CategoryId,
                enrolment_date = DateOnly.FromDateTime(DateTime.UtcNow),
                enrolment_status = "Pending",
                payment_status = "Pending",
                amount_paid = 0m,
                paid_date = DateOnly.FromDateTime(DateTime.UtcNow)
            });
            db.SaveChanges();

            var result = Controller(db).EnrolUserInCategory(new EnrollDTO { CategoryId = category.CategoryId, AmountPaid = 450m });

            Assert.IsType<ConflictObjectResult>(result);
            _ = profile;
        }

        [Fact]
        public void Enrol_AccountWithoutProfile_ReturnsNotFound()
        {
            var db = TestSupport.CreateDb();
            db.accounts.Add(new UserAccount { UserAccountId = 9, Email = "noprofile@raceday.com", PasswordHash = "x", Role = "Participant" });
            db.SaveChanges();
            var controller = new EnrolmentController(db)
                .WithUser(TestSupport.Principal(9, "noprofile@raceday.com", "Participant"));

            var result = controller.EnrolUserInCategory(new EnrollDTO { CategoryId = Guid.NewGuid(), AmountPaid = 10m });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetMyEnrolments_ReturnsOnlyOwnRecords()
        {
            var (db, _, category) = Seed();
            Controller(db).EnrolUserInCategory(new EnrollDTO { CategoryId = category.CategoryId, AmountPaid = 450m });

            var result = Controller(db).GetMyEnrolments();

            var ok = Assert.IsType<OkObjectResult>(result);
            var rows = Assert.IsAssignableFrom<IEnumerable<Enrolment>>(ok.Value).ToList();
            Assert.Single(rows);
        }

        [Fact]
        public void GetEnrolmentsByUserId_ForAnotherUser_ReturnsForbid()
        {
            var (db, _, _) = Seed();

            var result = Controller(db).GetEnrolmentsByUserId(Guid.NewGuid());

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void UpdateEnrolmentStatus_UnknownId_ReturnsNotFound()
        {
            var (db, _, _) = Seed();

            var result = Controller(db).UpdateEnrolmentStatus(Guid.NewGuid(),
                new StatusDTO { EnrolmentStatus = "Confirmed", PaymentStatus = "Paid" });

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
