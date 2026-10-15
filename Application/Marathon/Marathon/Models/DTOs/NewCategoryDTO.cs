namespace Race_Day.Models.DTOs
{
    public class NewCategoryDTO
    {
        public required Guid EventId { get; set; }
        public required string CategoryName { get; set; }
        public required int MinAge { get; set; }
        public required int MaxAge { get; set; }
        public required string GenderRestrictions { get; set; }
        public required int MaxParticipants { get; set; }
        public required string CreatedBy { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
