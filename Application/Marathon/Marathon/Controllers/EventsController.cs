using System.Security.Claims;
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
    public class EventsController : ControllerBase
    {
        public readonly ApplicationDbcontext dbcontext;

        public string EventStatus { get; private set; }

        public EventsController(ApplicationDbcontext dbcontext)
        {
            this.dbcontext = dbcontext;
        }
        [HttpGet]
        public IActionResult GetEvents()
        {
            var events = dbcontext.events.ToList();
            return Ok(events);
        }
        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetEventById(Guid id)
        {
            var eventItem = dbcontext.events.Find(id);
            if (eventItem == null) return NotFound($"Event with id {id} was not found.");
            return Ok(eventItem);
        }

        
        [HttpPost]
        public IActionResult CreateEvent(NewEventDTO newEventDTO)
        {
            
            var newEvent = new Event
            {
                EventName = newEventDTO.EventName,
                EventDate = newEventDTO.EventDate,
                Location = newEventDTO.Location,
                Venue = newEventDTO.Venue,
                DistanceKm = newEventDTO.DistanceKm,
                EventFee = newEventDTO.EventFee,
                EventType = newEventDTO.EventType,
                Status = "Scheduled",


                // server-controlled
                CreatedBy = 1235, // This should be replaced with the actual Employee ID from the authenticated user "aAdmin"
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Now = DateTime.UtcNow
            };

            dbcontext.events.Add(newEvent);
            dbcontext.SaveChanges();

            return CreatedAtAction(
                nameof(GetEventById),
                new { id = newEvent.EventId },
                newEvent
            );
        }
        [HttpPut]
        [Route("{id:guid}")]
        public IActionResult UpdateEvent(Guid id, UpdateEventDTO updatedEventDTO)
        {
            var existingEvent = dbcontext.events.Find(id);
            if (existingEvent == null) return NotFound($"Event with id {id} was not found.");
            existingEvent.EventName = updatedEventDTO.EventName;
            existingEvent.EventDate = updatedEventDTO.EventDate;
            existingEvent.Location = updatedEventDTO.Location;
            existingEvent.Venue = updatedEventDTO.Venue;
            existingEvent.DistanceKm = updatedEventDTO.DistanceKm;
            existingEvent.EventFee = updatedEventDTO.EventFee;
            existingEvent.EventType = updatedEventDTO.EventType;
            existingEvent.UpdatedAt = DateTime.UtcNow;
            dbcontext.SaveChanges();
            return Ok("Event was updated successfully.");
        }
        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult RemoveEvent(Guid id)
        {
            var existingEvent = dbcontext.events.Find(id);
            if (existingEvent == null) return NotFound($"Event with id {id} was not found.");
            dbcontext.events.Remove(existingEvent);
            dbcontext.SaveChanges();
            return Ok("Event was removed successfully.");
        }
    }
}
