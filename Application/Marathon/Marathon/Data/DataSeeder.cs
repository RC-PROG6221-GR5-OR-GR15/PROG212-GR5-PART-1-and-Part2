using Marathon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Marathon.Data
{
    /// <summary>
    /// Populates the database with realistic demonstration data the first time the
    /// API starts against an empty <c>MarathonDb</c>. Every account uses the password
    /// <c>Passw0rd!</c> so the assessor can sign in immediately.
    /// </summary>
    public static class DataSeeder
    {
        public const string DemoPassword = "Passw0rd!";

        public static async Task SeedAsync(ApplicationDbcontext db, IPasswordHasher<UserAccount> hasher)
        {
            // Idempotent: never seed twice.
            if (await db.accounts.AnyAsync()) return;

            var now = DateTime.UtcNow;

            // ----------------------------------------------------------------- Roles
            var organiserRole = new Role
            {
                RoleName = "Organiser",
                Description = "Creates and manages events, categories and participant enrolments.",
                CreatedAt = now
            };
            var participantRole = new Role
            {
                RoleName = "Participant",
                Description = "Browses events and enrols in race categories.",
                CreatedAt = now
            };
            var adminRole = new Role
            {
                RoleName = "Admin",
                Description = "Full system administration across all events and accounts.",
                CreatedAt = now
            };
            db.roles.AddRange(organiserRole, participantRole, adminRole);
            await db.SaveChangesAsync();

            // -------------------------------------------------------------- Accounts
            var organiserOne = NewAccount("organiser@raceday.com", "Organiser", hasher);
            var organiserTwo = NewAccount("events@capetownmarathon.co.za", "Organiser", hasher);
            var participantOne = NewAccount("participant@raceday.com", "Participant", hasher);
            var participantTwo = NewAccount("thabo.mokoena@raceday.com", "Participant", hasher);
            var participantThree = NewAccount("lindiwe.dlamini@raceday.com", "Participant", hasher);

            db.accounts.AddRange(organiserOne, organiserTwo, participantOne, participantTwo, participantThree);
            await db.SaveChangesAsync();

            // ------------------------------------------------- Profiles + role links
            var organiserOneProfile = NewProfile("organiser@raceday.com", "Nomsa", "Mahlangu",
                new DateOnly(1985, 3, 12), "082 555 0101", now);
            var organiserTwoProfile = NewProfile("events@capetownmarathon.co.za", "Sipho", "Ndlovu",
                new DateOnly(1980, 7, 30), "021 555 0202", now);
            var participantOneProfile = NewProfile("participant@raceday.com", "Lerato", "Pillay",
                new DateOnly(1996, 11, 5), "083 555 0303", now);
            var participantTwoProfile = NewProfile("thabo.mokoena@raceday.com", "Thabo", "Mokoena",
                new DateOnly(1992, 2, 18), "072 555 0404", now);
            var participantThreeProfile = NewProfile("lindiwe.dlamini@raceday.com", "Lindiwe", "Dlamini",
                new DateOnly(1999, 9, 22), "079 555 0505", now);

            db.users.AddRange(organiserOneProfile, organiserTwoProfile,
                participantOneProfile, participantTwoProfile, participantThreeProfile);
            await db.SaveChangesAsync();

            db.userRoles.AddRange(
                NewUserRole(organiserOneProfile.UserId, organiserRole.RoleId, now),
                NewUserRole(organiserTwoProfile.UserId, organiserRole.RoleId, now),
                NewUserRole(participantOneProfile.UserId, participantRole.RoleId, now),
                NewUserRole(participantTwoProfile.UserId, participantRole.RoleId, now),
                NewUserRole(participantThreeProfile.UserId, participantRole.RoleId, now));
            await db.SaveChangesAsync();

            // ---------------------------------------------------------------- Events
            var capeTownMarathon = NewEvent("Cape Town Marathon", new DateOnly(2026, 10, 18),
                "Cape Town", "Green Point Stadium", 42.20m, "Run", 450.00m, "Open",
                organiserOne.UserAccountId, now);
            var twoOceansHalf = NewEvent("Two Oceans Half Marathon", new DateOnly(2026, 4, 5),
                "Cape Town", "UCT Rugby Fields", 21.10m, "Run", 320.00m, "Open",
                organiserTwo.UserAccountId, now);
            var joburgCycleTour = NewEvent("Joburg City Cycle Tour", new DateOnly(2026, 11, 15),
                "Johannesburg", "FNB Stadium", 60.00m, "Cycle", 280.00m, "Upcoming",
                organiserOne.UserAccountId, now);

            db.events.AddRange(capeTownMarathon, twoOceansHalf, joburgCycleTour);
            await db.SaveChangesAsync();

            // ------------------------------------------------------------ Categories
            var seniorMen = NewCategory(capeTownMarathon.EventId, "Senior Men (18-39)", 18, 39,
                "Male", 1500, 450.00m, "organiser@raceday.com", now);
            var seniorWomen = NewCategory(capeTownMarathon.EventId, "Senior Women (18-39)", 18, 39,
                "Female", 1500, 450.00m, "organiser@raceday.com", now);
            var veterans = NewCategory(capeTownMarathon.EventId, "Veterans (40+)", 40, 99,
                "Open", 800, 400.00m, "organiser@raceday.com", now);
            var halfOpen = NewCategory(twoOceansHalf.EventId, "Open Half Marathon", 18, 99,
                "Open", 2000, 320.00m, "events@capetownmarathon.co.za", now);
            var cycleElite = NewCategory(joburgCycleTour.EventId, "Elite Cycling", 18, 45,
                "Open", 500, 280.00m, "organiser@raceday.com", now);

            db.categories.AddRange(seniorMen, seniorWomen, veterans, halfOpen, cycleElite);
            await db.SaveChangesAsync();

            // ------------------------------------------------------------ Enrolments
            db.enrolments.AddRange(
                NewEnrolment(participantOneProfile.UserId, capeTownMarathon.EventId, seniorMen.CategoryId,
                    new DateOnly(2026, 1, 20), "Confirmed", "Paid", 450.00m),
                NewEnrolment(participantTwoProfile.UserId, twoOceansHalf.EventId, halfOpen.CategoryId,
                    new DateOnly(2026, 1, 28), "Pending", "Pending", 0.00m),
                NewEnrolment(participantThreeProfile.UserId, capeTownMarathon.EventId, seniorWomen.CategoryId,
                    new DateOnly(2026, 2, 3), "Confirmed", "Paid", 450.00m));
            await db.SaveChangesAsync();

            // ------------------------------------- Participant_Event + Result examples
            var raceEntry = new Participant_Event
            {
                UserId = participantOneProfile.UserId,
                EventId = capeTownMarathon.EventId,
                CategoryId = seniorMen.CategoryId,
                bib_number = 1042,
                fining_position = 12,
                finish_time_seconds = new TimeOnly(3, 12, 45),
                entry_status = "Completed",
                entered_at = new DateOnly(2026, 1, 20)
            };
            db.participant_Events.Add(raceEntry);
            await db.SaveChangesAsync();

            db.results.Add(new Result
            {
                ParticipantEventId = raceEntry.Participant_EventId,
                finish_position = "12",
                finish_time_seconds = "11645",
                pace_pace_km = "4:36",
                overall_rank = "12 / 1500",
                category_rank = "5 / 640",
                prize_money = 0.00m
            });
            await db.SaveChangesAsync();
        }

        private static UserAccount NewAccount(string email, string role, IPasswordHasher<UserAccount> hasher)
        {
            var account = new UserAccount { Email = email, Role = role, PasswordHash = string.Empty };
            account.PasswordHash = hasher.HashPassword(account, DemoPassword);
            return account;
        }

        private static User NewProfile(string email, string firstName, string lastName,
            DateOnly dateOfBirth, string phoneNumber, DateTime now) => new()
        {
            Username = email,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            PhoneNumber = phoneNumber,
            CreatedAt = now,
            LastLogin = now,
            IsActive = true
        };

        private static UserRole NewUserRole(Guid userId, Guid roleId, DateTime now) => new()
        {
            userrole_id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            assigned_at = now,
            assinged_by = "system"
        };

        private static Event NewEvent(string name, DateOnly date, string location, string venue,
            decimal distanceKm, string eventType, decimal eventFee, string status, int createdBy, DateTime now) => new()
        {
            EventName = name,
            EventDate = date,
            Now = now,
            Location = location,
            Venue = venue,
            DistanceKm = distanceKm,
            EventType = eventType,
            EventFee = eventFee,
            Status = status,
            CreatedBy = createdBy,
            CreatedAt = now,
            UpdatedAt = now
        };

        private static Category NewCategory(Guid eventId, string name, int minAge, int maxAge,
            string gender, int maxParticipants, decimal entryFee, string createdBy, DateTime now) => new()
        {
            EventId = eventId,
            CategoryName = name,
            MinAge = minAge,
            MaxAge = maxAge,
            GenderRestrictions = gender,
            MaxParticipants = maxParticipants,
            EntryFee = entryFee,
            CreatedBy = createdBy,
            CreatedAt = now
        };

        private static Enrolment NewEnrolment(Guid userId, Guid eventId, Guid categoryId,
            DateOnly enrolmentDate, string enrolmentStatus, string paymentStatus, decimal amountPaid) => new()
        {
            UserId = userId,
            EventId = eventId,
            CategoryId = categoryId,
            enrolment_date = enrolmentDate,
            enrolment_status = enrolmentStatus,
            payment_status = paymentStatus,
            amount_paid = amountPaid,
            paid_date = amountPaid > 0 ? enrolmentDate : DateOnly.MinValue
        };
    }
}
