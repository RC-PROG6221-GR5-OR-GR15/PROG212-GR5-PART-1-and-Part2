namespace Marathon.Models.DTOs
{
    public class RegisterDTO
    {
        public required string Email { get; set; }
        public required string Password{ get; set; }
        public required string Role { get; set; }
    }
}
