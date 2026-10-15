using System.ComponentModel.DataAnnotations;

namespace Marathon.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateOnly DateOfBirth { get; set; }
        public required string PhoneNumber { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime LastLogin { get; set; }
        public required bool IsActive { get; set; }

    }
}
