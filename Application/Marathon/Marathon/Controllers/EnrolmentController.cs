using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;
using System.Security.Claims;

namespace Marathon.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentController : ControllerBase
    {
        public readonly ApplicationDbcontext dbcontext;
        public EnrolmentController(ApplicationDbcontext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        /// <summary>
        /// Resolves the caller's UserAccount together with the linked User profile.
        /// The JWT carries the integer UserAccountId, while enrolments reference the
        /// User profile, so both are needed.
        /// </summary>
        private (UserAccount? account, User? profile) ResolveCurrentUser()
        {
            var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(raw, out var accountId)) return (null, null);

            var account = dbcontext.accounts.Find(accountId);
            if (account is null) return (null, null);

            var profile = dbcontext.users.FirstOrDefault(u => u.Username == account.Email);
            return (account, profile);
        }

        private bool IsStaff() => User.IsInRole("Organiser") || User.IsInRole("Admin");

        [Authorize(Roles = "Participant")]
        [HttpPost]
        public IActionResult EnrolUserInCategory(EnrollDTO enrollDTO)
        {
            var (account, user) = ResolveCurrentUser();
            if (account is null) return Unauthorized("User account not found.");
            if (user is null) return NotFound("No participant profile is linked to this account.");
            if (!user.IsActive) return BadRequest("This participant profile is inactive.");

            var category = dbcontext.categories.Find(enrollDTO.CategoryId);
            if (category is null) return NotFound($"Category with id {enrollDTO.CategoryId} was not found.");

            if (enrollDTO.AmountPaid < 0) return BadRequest("Amount paid cannot be negative.");

            var existingEnrolment = dbcontext.enrolments
                .FirstOrDefault(e => e.UserId == user.UserId && e.CategoryId == enrollDTO.CategoryId);
            if (existingEnrolment is not null)
                return Conflict("You are already enrolled in this category.");

            var placesTaken = dbcontext.enrolments.Count(e => e.CategoryId == category.CategoryId);
            if (placesTaken >= category.MaxParticipants)
                return Conflict("This category is full.");

            var enrolment = new Enrolment
            {
                UserId = user.UserId,
                CategoryId = category.CategoryId,
                EventId = category.EventId,
                enrolment_date = DateOnly.FromDateTime(DateTime.UtcNow),
                amount_paid = enrollDTO.AmountPaid,

                // server-controlled
                enrolment_status = "Pending",
                payment_status = enrollDTO.AmountPaid >= category.EntryFee ? "Paid" : "Pending",
                paid_date = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            dbcontext.enrolments.Add(enrolment);
            dbcontext.SaveChanges();

            return Ok(new { message = "Enrolment successful", enrolmentId = enrolment.EnrolmentId });
        }

        /// <summary>The signed-in participant's own enrolments.</summary>
        [HttpGet]
        [Route("me")]
        public IActionResult GetMyEnrolments()
        {
            var (account, user) = ResolveCurrentUser();
            if (account is null) return Unauthorized();

            if (user is null) return Ok(new List<Enrolment>());

            var enrolments = dbcontext.enrolments
                .Where(e => e.UserId == user.UserId)
                .ToList();

            return Ok(enrolments);
        }

        /// <summary>Organisers and admins only: the full enrolment register.</summary>
        [Authorize(Roles = "Organiser, Admin")]
        [HttpGet]
        public IActionResult GetAllEnrolments()
        {
            return Ok(dbcontext.enrolments.ToList());
        }

        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetEnrolmentById(Guid id)
        {
            var enrolment = dbcontext.enrolments.Find(id);
            if (enrolment is null) return NotFound($"Enrolment with id {id} was not found.");

            if (!CanAccess(enrolment)) return Forbid();

            return Ok(enrolment);
        }

        [HttpGet]
        [Route("user/{userId:guid}")]
        public IActionResult GetEnrolmentsByUserId(Guid userId)
        {
            var (_, caller) = ResolveCurrentUser();
            if (!IsStaff() && (caller is null || caller.UserId != userId)) return Forbid();

            var enrolments = dbcontext.enrolments.Where(e => e.UserId == userId).ToList();
            return Ok(enrolments);
        }

        /// <summary>Approving or rejecting an enrolment is an organiser action.</summary>
        [Authorize(Roles = "Organiser, Admin")]
        [HttpPatch]
        [Route("{id:guid}")]
        public IActionResult UpdateEnrolmentStatus(Guid id, StatusDTO statusDTO)
        {
            var enrolment = dbcontext.enrolments.Find(id);
            if (enrolment is null) return NotFound($"Enrolment with id {id} was not found.");

            enrolment.enrolment_status = statusDTO.EnrolmentStatus;
            enrolment.payment_status = statusDTO.PaymentStatus;
            dbcontext.SaveChanges();

            return Ok($"Enrolment with id {id} has been updated.");
        }

        private bool CanAccess(Enrolment enrolment)
        {
            if (IsStaff()) return true;

            var (_, caller) = ResolveCurrentUser();
            return caller is not null && caller.UserId == enrolment.UserId;
        }
    }
}
