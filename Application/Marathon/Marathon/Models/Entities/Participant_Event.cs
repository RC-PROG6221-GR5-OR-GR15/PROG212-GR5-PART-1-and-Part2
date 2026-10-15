using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Race_Day.Models
{
    public class Participant_Event
    {
        [Key]
        public Guid Participant_EventId { get; set; } = Guid.NewGuid();
        [ForeignKey("User")]
        public required Guid UserId { get; set; }
        [ForeignKey("Event")]
        public required Guid EventId { get; set; }
        [ForeignKey("Category")]
        public required Guid CategoryId { get; set; }
        public required int bib_number { get; set; }
        public required int fining_position { get; set; }
        public required TimeOnly finish_time_seconds { get; set; }
        public required string entry_status { get; set; }
        public required DateOnly entered_at { get; set; }



    }
}
