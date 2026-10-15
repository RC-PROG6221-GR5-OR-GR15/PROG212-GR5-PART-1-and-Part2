namespace Race_Day.Models.DTOs
{
    public class UpdateEventDTO
    {
        public required string EventName { get; set; }
        public required DateOnly EventDate { get; set; }
        public required DateTime Now { get; set; }
        public required string Location { get; set; }
        public required string Venue { get; set; }
        public required float DistanceKm { get; set; }
        public required string EventType { get; set; }
        public required float EventFee { get; set; }
        public required string Status { get; set; }
        public required int CreatedBy { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }
}
