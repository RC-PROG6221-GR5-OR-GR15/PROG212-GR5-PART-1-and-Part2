namespace Marathon.Models.DTOs
{
    public class Add_New_UserDTO
    {
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateOnly DateOfBirth { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
