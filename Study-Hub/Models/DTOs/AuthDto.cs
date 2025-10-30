﻿using System.ComponentModel.DataAnnotations;

namespace Study_Hub.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }

    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [MinLength(1)]
        public string Name { get; set; }
    }

    public class AuthResponseDto
    {
        public UserDto User { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public bool EmailVerified { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}