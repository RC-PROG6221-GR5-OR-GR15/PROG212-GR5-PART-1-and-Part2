using System.ComponentModel.DataAnnotations;

namespace Race_Day.Models
{
    public class Role
    {
        [Key]
        public Guid RoleId { get; set; } = Guid.NewGuid();

        public required string RoleName { get; set; } //Either Participant or Organiser
        public required string Description { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
