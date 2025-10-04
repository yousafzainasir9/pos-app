using System;
using System.Collections.Generic;

namespace POS.WebAPI.DTOs
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public long? StoreId { get; set; }
        public string StoreName { get; set; }
    }

    public class UserDetailDto
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public long? StoreId { get; set; }
        public string StoreName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool HasPin { get; set; }
    }

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

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; }
    }

    public class ResetPinDto
    {
        public string NewPin { get; set; }
    }

    public class UserActivityDto
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string IpAddress { get; set; }
    }
}
