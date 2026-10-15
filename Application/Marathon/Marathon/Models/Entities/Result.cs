using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marathon.Models
{
    public class Result
    {
    

        [Key]
        public Guid ResultId { get; set; } = Guid.NewGuid();
        [ForeignKey("participant_event_id")]
        public required Guid ParticipantEventId { get; set; }
        public required string finish_position { get; set; }
        public required string finish_time_seconds { get; set; }
        public required string pace_pace_km { get; set; }
        public required string overall_rank { get; set; }
        public required decimal prize_money { get; set; }
        public required string category_rank { get; set; }
        
    }
}
