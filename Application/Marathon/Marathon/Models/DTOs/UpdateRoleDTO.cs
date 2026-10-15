namespace Marathon.Models.DTOs
{
    public class UpdateRoleDTO
    {

        public required string RoleName { get; set; } //Either Participant or Organiser
        public required string Description { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
