using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;

namespace Marathon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbcontext dbcontext;

        public ResultsController(ApplicationDbcontext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        // ─────────────────────────────────────────────
        // GET: api/results
        // Admin / Organiser only
        // ─────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> GetAllResults()
        {
            var results = await dbcontext.results
                .Select(r => new
                {
                    r.ResultId,
                    r.ParticipantEventId,
                    r.finish_position,
                    r.finish_time_seconds,
                    r.pace_pace_km,
                    r.overall_rank,
                    r.prize_money,
                    r.category_rank
                })
                .ToListAsync();

            return Ok(results);
        }

        // ─────────────────────────────────────────────
        // GET: api/results/{id}
        // ─────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResultById(Guid id)
        {
            var result = await dbcontext.results.FindAsync(id);

            if (result == null)
                return NotFound($"Result with id {id} not found");

            return Ok(result);
        }

        // ─────────────────────────────────────────────
        // GET: api/results/enrolment/{participantEventId}
        // ─────────────────────────────────────────────
        [HttpGet("enrolment/{participantEventId}")]
        public async Task<IActionResult> GetResultsForEnrolment(Guid participantEventId)
        {
            var enrolment = await dbcontext.participant_Events
                .FindAsync(participantEventId);

            if (enrolment == null)
                return NotFound($"Enrolment with id {participantEventId} not found");

            var results = await dbcontext.results
                .Where(r => r.ParticipantEventId == participantEventId)
                .ToListAsync();

            return Ok(new
            {
                participantEventId,
                count = results.Count,
                results
            });
        }

        // ─────────────────────────────────────────────
        // POST: api/results
        // Admin / Organiser only
        // ─────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> CreateResult([FromBody] CreateResultDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var enrolment = await dbcontext.participant_Events
                .FindAsync(dto.ParticipantEventId);

            if (enrolment == null)
                return NotFound($"Enrolment with id {dto.ParticipantEventId} not found");

            var exists = await dbcontext.results
                .AnyAsync(r => r.ParticipantEventId == dto.ParticipantEventId);

            if (exists)
                return BadRequest("A result for this enrolment already exists");

            var result = new Result
            {
                ParticipantEventId  = dto.ParticipantEventId,
                finish_position     = dto.finish_position,
                finish_time_seconds = dto.finish_time_seconds,
                pace_pace_km        = dto.pace_pace_km,
                overall_rank        = dto.overall_rank,
                prize_money         = dto.prize_money,
                category_rank       = dto.category_rank
            };

            dbcontext.results.Add(result);
            await dbcontext.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResultById),
                new { id = result.ResultId },
                result);
        }

        // ─────────────────────────────────────────────
        // PUT: api/results/{id}
        // Admin / Organiser only
        // ─────────────────────────────────────────────
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> UpdateResult(Guid id, [FromBody] UpdateResultDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await dbcontext.results.FindAsync(id);
            if (result == null)
                return NotFound($"Result with id {id} not found");

            result.finish_position     = dto.finish_position;
            result.finish_time_seconds = dto.finish_time_seconds;
            result.pace_pace_km        = dto.pace_pace_km;
            result.overall_rank        = dto.overall_rank;
            result.prize_money         = dto.prize_money;
            result.category_rank       = dto.category_rank;

            await dbcontext.SaveChangesAsync();

            return Ok(result);
        }

        
        // Admin only
       
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteResult(Guid id)
        {
            var result = await dbcontext.results.FindAsync(id);
            if (result == null)
                return NotFound($"Result with id {id} not found");

            dbcontext.results.Remove(result);
            await dbcontext.SaveChangesAsync();

            return NoContent();
        }
    }
}