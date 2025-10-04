namespace POS.Application.DTOs
{
    public class UpdateUserDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public bool? IsActive { get; set; }
        public long? StoreId { get; set; }
    }
}
