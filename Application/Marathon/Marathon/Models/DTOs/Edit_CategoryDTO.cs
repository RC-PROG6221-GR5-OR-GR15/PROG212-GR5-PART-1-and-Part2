namespace Marathon.Models.DTOs
{
    public class Edit_CategoryDTO
    {
        public string? CategoryName { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? GenderRestrictions { get; set; }
        public int? MaxParticipants { get; set; }
    }
}
