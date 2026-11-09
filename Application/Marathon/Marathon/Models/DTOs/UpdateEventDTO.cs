namespace Marathon.Models.DTOs
{
    /// <summary>
    /// Payload for updating an event. Status is optional and only applied when supplied.
    /// </summary>
    public class UpdateEventDTO
    {
        public required string EventName { get; set; }
        public required DateOnly EventDate { get; set; }
        public required string Location { get; set; }
        public required string Venue { get; set; }
        public required decimal DistanceKm { get; set; }
        public required string EventType { get; set; }
        public required decimal EventFee { get; set; }
        public string? Status { get; set; }

        /// <summary>Optional URL of a banner image previously uploaded to Blob Storage.</summary>
        public string? BannerImageUrl { get; set; }
    }
}
