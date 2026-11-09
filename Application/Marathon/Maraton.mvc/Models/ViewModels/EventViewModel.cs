using System.ComponentModel.DataAnnotations;

namespace Maraton.mvc.Models.ViewModels
{
    public class EventViewModel
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string Venue { get; set; }
        public string Location { get; set; }
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; }
        public decimal EventFee { get; set; }
        public string Status { get; set; }

        /// <summary>UserAccountId of the organiser who created the event.</summary>
        public int CreatedBy { get; set; }

        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }
}
