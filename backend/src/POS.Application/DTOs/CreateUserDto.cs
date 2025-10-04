namespace POS.Application.DTOs
{
    public class CreateUserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Pin { get; set; }
        public string Role { get; set; }
        public long? StoreId { get; set; }
    }
}
