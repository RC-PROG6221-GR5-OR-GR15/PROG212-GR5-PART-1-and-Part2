namespace Marathon.Models.DTOs
{
    public class Edit_Existing_UserDTO
    {
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
    }
}
