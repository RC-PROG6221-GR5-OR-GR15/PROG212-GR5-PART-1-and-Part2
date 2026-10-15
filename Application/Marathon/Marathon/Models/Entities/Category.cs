using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Marathon.Models
{
    public class Category
    {
        [Key]
        public Guid CategoryId { get; set; } = Guid.NewGuid();

        [ForeignKey("Event")]
        public  Guid EventId { get; set; }
        public required string CategoryName { get; set; }
        public required int MinAge { get; set; }
        public required int MaxAge { get; set; }
        public required string GenderRestrictions { get; set; }
        public required int MaxParticipants { get; set; }
        public required string CreatedBy { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
