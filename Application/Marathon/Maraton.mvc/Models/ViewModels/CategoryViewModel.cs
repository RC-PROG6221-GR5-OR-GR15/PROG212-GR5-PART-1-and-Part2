namespace Maraton.mvc.Models.ViewModels
{
    public class CategoryViewModel
    {
        public Guid CategoryId { get; set; }
        public Guid EventId { get; set; }
        public string CategoryName { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string GenderRestrictions { get; set; }
        public int MaxParticipants { get; set; }
        public decimal EntryFee { get; set; }
    }
}
