using System.ComponentModel.DataAnnotations;

namespace Maraton.mvc.Models.ViewModels
{
    /// <summary>Used by the Organiser for both creating and editing an event.</summary>
    public class EventFormViewModel
    {
        /// <summary>Null when creating a new event.</summary>
        public Guid? EventId { get; set; }

        public bool IsEdit => EventId.HasValue;

        [Required(ErrorMessage = "Please enter an event name.")]
        [StringLength(120)]
        [Display(Name = "Event name")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose an event date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Event date")]
        public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

        [Required(ErrorMessage = "Please enter the town or city.")]
        [StringLength(120)]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the venue.")]
        [StringLength(120)]
        [Display(Name = "Venue")]
        public string Venue { get; set; } = string.Empty;

        [Required]
        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public decimal DistanceKm { get; set; }

        [Required(ErrorMessage = "Please choose an event type.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = "Run";

        [Required]
        [Range(0, 100000, ErrorMessage = "Entry fee must be between R0 and R100 000.")]
        [Display(Name = "Entry fee (R)")]
        public decimal EventFee { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Upcoming";
    }
}
