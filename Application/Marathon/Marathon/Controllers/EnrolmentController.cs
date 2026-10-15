using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;

namespace Marathon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentController : ControllerBase
    {
        public readonly ApplicationDbcontext dbcontext;
        public EnrolmentController(ApplicationDbcontext dbcontext)
        {
            this.dbcontext = dbcontext;
        }
        [HttpPost]
        public IActionResult EnrolUserInCategory(Guid userId, Guid categoryId, EnrollDTO enrollDTO)
        {
            var user = dbcontext.users.Find(userId);
            if (user == null) return NotFound($"User with id {userId} was not found.");
            var category = dbcontext.categories.Find(categoryId);
            if (category == null) return NotFound($"Category with id {categoryId} was not found.");
           
            var existingEnrolment = dbcontext.enrolments
                .FirstOrDefault(e => e.UserId == userId && e.CategoryId == categoryId);
            if (existingEnrolment != null)
                return Conflict($"User with id {userId} is already enrolled in category {categoryId}.");
          
            var enrolment = new Enrolment
            {
                UserId = userId,
                CategoryId = categoryId,
                EventId = category.EventId,
                enrolment_date = DateOnly.FromDateTime(DateTime.UtcNow),
                amount_paid = enrollDTO.AmountPaid,

                // server-controlled
                enrolment_status = "Pending",
                payment_status = "Pending",
                paid_date = DateOnly.FromDateTime(DateTime.UtcNow)

            };
            dbcontext.enrolments.Add(enrolment);
            dbcontext.SaveChanges();
            return Ok($"User with id {userId} has been enrolled in category {categoryId}.");
        }
        [HttpGet]
        public IActionResult GetAllEnrolments()
        {
            var enrolments = dbcontext.enrolments.ToList();
            return Ok(enrolments);
        }
        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetEnrolmentById(Guid id)
        {
             var enrolment = dbcontext.enrolments.Find(id);
            if (enrolment == null) return NotFound($"Enrolment with id {id} was not found.");
            return Ok(enrolment);
        }
        [HttpGet]
        [Route("user/{userId:guid}")]
        public IActionResult GetEnrolmentsByUserId(Guid userId)
        {
            var enrolments = dbcontext.enrolments.Where(e => e.UserId == userId).ToList();
            if (!enrolments.Any()) return NotFound($"No enrolments found for user with id {userId}.");
            return Ok(enrolments);
        }
        [HttpPatch]
        [Route("{id:guid}")]
        public IActionResult UpdateEnrolmentStatus(Guid id, StatusDTO statusDTO)
        {
            var enrolment = dbcontext.enrolments.Find(id);
            if (enrolment == null) return NotFound($"Enrolment with id {id} was not found.");
            enrolment.enrolment_status = statusDTO.EnrolmentStatus;
            enrolment.payment_status = statusDTO.PaymentStatus;
            dbcontext.SaveChanges();
            return Ok($"Enrolment with id {id} has been updated.");
        }
    }
}
