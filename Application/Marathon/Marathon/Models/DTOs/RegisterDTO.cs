namespace Marathon.Models.DTOs
{
    /// <summary>
    /// Registration payload. The profile fields are required when registering as a
    /// Participant because they populate the linked User profile record.
    /// </summary>
    public class RegisterDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
